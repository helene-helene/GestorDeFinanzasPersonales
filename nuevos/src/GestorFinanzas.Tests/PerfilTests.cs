using System.Net;
using GestorFinanzas.Tests.Infraestructura;
using GestorFinanzas.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace GestorFinanzas.Tests;

/// <summary>Pruebas del módulo Mi perfil y de la protección de los datos personales.</summary>
public class PerfilTests
{
    [Fact]
    public async Task ConsultarPerfil_UsuarioAutenticado_MuestraSusPropiosDatos()
    {
        using var app = new AplicacionDePrueba();
        var navegador = new Navegador(app);
        await navegador.IniciarSesionCorrectamenteAsync(AplicacionDePrueba.CorreoAna, AplicacionDePrueba.ContraseniaAna);

        var html = await navegador.ObtenerHtmlAsync("/Perfil");

        Assert.Contains("Ana Prueba", html);
        Assert.Contains(AplicacionDePrueba.CorreoAna, html);
        Assert.DoesNotContain("Beto Prueba", html);
        Assert.DoesNotContain("AQAAAA", html);   // el hash de la contraseña nunca llega a la vista
    }

    [Fact]
    public async Task ActualizarPerfil_ConDatosValidos_GuardaLosCambiosYMantieneLaSesion()
    {
        using var app = new AplicacionDePrueba();
        var navegador = new Navegador(app);
        await navegador.IniciarSesionCorrectamenteAsync(AplicacionDePrueba.CorreoAna, AplicacionDePrueba.ContraseniaAna);

        var respuesta = await navegador.EnviarFormularioAsync("/Perfil", "/Perfil", new Dictionary<string, string>
        {
            ["Nombre"] = "Ana Maria Prueba",
            ["Correo"] = "ana.maria@prueba.com"
        });

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);

        var ana = app.UsarBaseDeDatos(bd => bd.Usuarios.Single(u => u.Id == app.AnaId));
        Assert.Equal("Ana Maria Prueba", ana.Nombre);
        Assert.Equal("ana.maria@prueba.com", ana.Correo);

        // La sesión sigue activa y el menú ya muestra el nombre nuevo.
        var html = await navegador.ObtenerHtmlAsync("/Perfil");
        Assert.Contains("Tus datos se actualizaron correctamente.", html);
        Assert.Contains("Ana Maria Prueba", html);
    }

    [Fact]
    public async Task ConsultarPerfil_ConIdDeOtroUsuarioEnLaUrl_SoloMuestraElPerfilPropio()
    {
        using var app = new AplicacionDePrueba();
        var navegador = new Navegador(app);
        await navegador.IniciarSesionCorrectamenteAsync(AplicacionDePrueba.CorreoAna, AplicacionDePrueba.ContraseniaAna);

        var html = await navegador.ObtenerHtmlAsync($"/Perfil/Index/{app.BetoId}");

        Assert.Contains("Ana Prueba", html);
        Assert.DoesNotContain("Beto Prueba", html);
        Assert.DoesNotContain(AplicacionDePrueba.CorreoBeto, html);
    }

    [Fact]
    public async Task ActualizarPerfil_EnviandoElIdDeOtroUsuario_SoloModificaElPerfilPropio()
    {
        using var app = new AplicacionDePrueba();
        var navegador = new Navegador(app);
        await navegador.IniciarSesionCorrectamenteAsync(AplicacionDePrueba.CorreoAna, AplicacionDePrueba.ContraseniaAna);

        // Se intenta modificar a Beto agregando su Id en la URL y en el formulario.
        var respuesta = await navegador.EnviarFormularioAsync("/Perfil", $"/Perfil/Index/{app.BetoId}", new Dictionary<string, string>
        {
            ["Id"] = app.BetoId.ToString(),
            ["UsuarioId"] = app.BetoId.ToString(),
            ["Nombre"] = "Nombre Cambiado",
            ["Correo"] = "cambiado@prueba.com"
        });

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);

        var beto = app.UsarBaseDeDatos(bd => bd.Usuarios.Single(u => u.Id == app.BetoId));
        Assert.Equal("Beto Prueba", beto.Nombre);
        Assert.Equal(AplicacionDePrueba.CorreoBeto, beto.Correo);

        var ana = app.UsarBaseDeDatos(bd => bd.Usuarios.Single(u => u.Id == app.AnaId));
        Assert.Equal("Nombre Cambiado", ana.Nombre);
    }

    [Fact]
    public async Task ActualizarPerfil_ConDatosInvalidos_NoGuardaNada()
    {
        using var app = new AplicacionDePrueba();
        var navegador = new Navegador(app);
        await navegador.IniciarSesionCorrectamenteAsync(AplicacionDePrueba.CorreoAna, AplicacionDePrueba.ContraseniaAna);

        var respuesta = await navegador.EnviarFormularioAsync("/Perfil", "/Perfil", new Dictionary<string, string>
        {
            ["Nombre"] = "",
            ["Correo"] = "correo-sin-arroba"
        });
        var html = await Navegador.LeerHtmlAsync(respuesta);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("Escribe tu nombre.", html);
        Assert.Contains("El correo no tiene un formato válido.", html);

        var ana = app.UsarBaseDeDatos(bd => bd.Usuarios.Single(u => u.Id == app.AnaId));
        Assert.Equal("Ana Prueba", ana.Nombre);
        Assert.Equal(AplicacionDePrueba.CorreoAna, ana.Correo);
    }

    [Fact]
    public async Task ActualizarPerfil_ConElCorreoDeOtroUsuario_SeRechaza()
    {
        using var app = new AplicacionDePrueba();
        var navegador = new Navegador(app);
        await navegador.IniciarSesionCorrectamenteAsync(AplicacionDePrueba.CorreoAna, AplicacionDePrueba.ContraseniaAna);

        var respuesta = await navegador.EnviarFormularioAsync("/Perfil", "/Perfil", new Dictionary<string, string>
        {
            ["Nombre"] = "Ana Prueba",
            ["Correo"] = AplicacionDePrueba.CorreoBeto
        });
        var html = await Navegador.LeerHtmlAsync(respuesta);

        Assert.Contains("Ese correo ya está registrado por otro usuario.", html);
        var ana = app.UsarBaseDeDatos(bd => bd.Usuarios.Single(u => u.Id == app.AnaId));
        Assert.Equal(AplicacionDePrueba.CorreoAna, ana.Correo);
    }

    [Fact]
    public async Task ActualizarPerfil_SinTokenAntifalsificacion_SeRechaza()
    {
        using var app = new AplicacionDePrueba();
        var navegador = new Navegador(app);
        await navegador.IniciarSesionCorrectamenteAsync(AplicacionDePrueba.CorreoAna, AplicacionDePrueba.ContraseniaAna);

        // Envío directo, como lo haría un formulario de otro sitio (sin token).
        var respuesta = await navegador.Cliente.PostAsync("/Perfil", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Nombre"] = "Ataque",
            ["Correo"] = "ataque@prueba.com"
        }));

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        var ana = app.UsarBaseDeDatos(bd => bd.Usuarios.Single(u => u.Id == app.AnaId));
        Assert.Equal("Ana Prueba", ana.Nombre);
    }

    [Fact]
    public async Task CambiarContrasenia_ConLaContraseniaActualCorrecta_PermiteEntrarConLaNueva()
    {
        using var app = new AplicacionDePrueba();
        var navegador = new Navegador(app);
        await navegador.IniciarSesionCorrectamenteAsync(AplicacionDePrueba.CorreoAna, AplicacionDePrueba.ContraseniaAna);

        var respuesta = await navegador.EnviarFormularioAsync("/Perfil/CambiarContrasenia", "/Perfil/CambiarContrasenia", new Dictionary<string, string>
        {
            ["ContraseniaActual"] = AplicacionDePrueba.ContraseniaAna,
            ["ContraseniaNueva"] = "NuevaClave2026",
            ["ConfirmarContrasenia"] = "NuevaClave2026"
        });
        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);

        // El hash guardado corresponde a la contraseña nueva.
        var ana = app.UsarBaseDeDatos(bd => bd.Usuarios.Single(u => u.Id == app.AnaId));
        var resultado = new PasswordHasher<Usuario>().VerifyHashedPassword(ana, ana.Contrasenia, "NuevaClave2026");
        Assert.NotEqual(PasswordVerificationResult.Failed, resultado);

        // Con la contraseña anterior ya no se puede entrar; con la nueva sí.
        var otroNavegador = new Navegador(app);
        var conAnterior = await otroNavegador.IniciarSesionAsync(AplicacionDePrueba.CorreoAna, AplicacionDePrueba.ContraseniaAna);
        Assert.Equal(HttpStatusCode.OK, conAnterior.StatusCode);
        await otroNavegador.IniciarSesionCorrectamenteAsync(AplicacionDePrueba.CorreoAna, "NuevaClave2026");
    }

    [Fact]
    public async Task CambiarContrasenia_ConLaContraseniaActualIncorrecta_SeRechaza()
    {
        using var app = new AplicacionDePrueba();
        var navegador = new Navegador(app);
        await navegador.IniciarSesionCorrectamenteAsync(AplicacionDePrueba.CorreoAna, AplicacionDePrueba.ContraseniaAna);
        var hashAnterior = app.UsarBaseDeDatos(bd => bd.Usuarios.Single(u => u.Id == app.AnaId).Contrasenia);

        var respuesta = await navegador.EnviarFormularioAsync("/Perfil/CambiarContrasenia", "/Perfil/CambiarContrasenia", new Dictionary<string, string>
        {
            ["ContraseniaActual"] = "NoEsLaMia123",
            ["ContraseniaNueva"] = "NuevaClave2026",
            ["ConfirmarContrasenia"] = "NuevaClave2026"
        });
        var html = await Navegador.LeerHtmlAsync(respuesta);

        Assert.Contains("La contraseña actual no es correcta.", html);
        Assert.Equal(hashAnterior, app.UsarBaseDeDatos(bd => bd.Usuarios.Single(u => u.Id == app.AnaId).Contrasenia));
    }
}
