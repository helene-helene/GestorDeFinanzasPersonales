using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GestorFinanzas.Tests.Infraestructura;

/// <summary>
/// Simula a un estudiante usando el sitio: guarda las cookies, lee el token antifalsificación
/// de cada formulario y envía los datos igual que lo haría el navegador.
/// </summary>
public class Navegador
{
    private static readonly Regex TokenRegex =
        new("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"", RegexOptions.Compiled);

    public HttpClient Cliente { get; }

    public Navegador(AplicacionDePrueba aplicacion)
    {
        // Sin seguir redirecciones automáticamente, para poder comprobar a dónde manda cada respuesta.
        Cliente = aplicacion.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
    }

    public async Task<string> ObtenerHtmlAsync(string url)
    {
        var respuesta = await Cliente.GetAsync(url);
        return await LeerHtmlAsync(respuesta);
    }

    /// <summary>Abre la página del formulario, toma su token y envía los datos a la dirección indicada.</summary>
    public async Task<HttpResponseMessage> EnviarFormularioAsync(string paginaFormulario, string destino, Dictionary<string, string> datos)
    {
        var html = await ObtenerHtmlAsync(paginaFormulario);
        var coincidencia = TokenRegex.Match(html);
        if (!coincidencia.Success)
        {
            throw new InvalidOperationException($"La página {paginaFormulario} no tiene token antifalsificación.");
        }

        var campos = new Dictionary<string, string>(datos)
        {
            ["__RequestVerificationToken"] = coincidencia.Groups[1].Value
        };

        return await Cliente.PostAsync(destino, new FormUrlEncodedContent(campos));
    }

    public async Task<HttpResponseMessage> IniciarSesionAsync(string correo, string contrasenia, string? returnUrl = null)
    {
        var datos = new Dictionary<string, string>
        {
            ["Correo"] = correo,
            ["Contrasenia"] = contrasenia
        };
        if (returnUrl is not null)
        {
            datos["ReturnUrl"] = returnUrl;
        }

        return await EnviarFormularioAsync("/Acceso/Login", "/Acceso/Login", datos);
    }

    /// <summary>Inicia sesión y comprueba que el servidor haya aceptado las credenciales.</summary>
    public async Task IniciarSesionCorrectamenteAsync(string correo, string contrasenia)
    {
        var respuesta = await IniciarSesionAsync(correo, contrasenia);
        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
    }

    /// <summary>Ruta a la que redirige una respuesta (sin "http://localhost" si viene completa).</summary>
    public static string RutaDe(HttpResponseMessage respuesta)
    {
        var destino = respuesta.Headers.Location;
        if (destino is null)
        {
            return string.Empty;
        }

        return destino.IsAbsoluteUri ? destino.PathAndQuery : destino.OriginalString;
    }

    /// <summary>Devuelve el HTML con los acentos ya decodificados (Razor los codifica como &amp;#xE1;).</summary>
    public static async Task<string> LeerHtmlAsync(HttpResponseMessage respuesta) =>
        WebUtility.HtmlDecode(await respuesta.Content.ReadAsStringAsync());
}
