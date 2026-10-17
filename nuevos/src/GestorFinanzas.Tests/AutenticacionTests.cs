using System.Net;
using GestorFinanzas.Tests.Infraestructura;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanzas.Tests;

/// <summary>Pruebas del login, el registro, el cierre de sesión y la protección de rutas.</summary>
public class AutenticacionTests
{
    [Fact]
    public async Task Login_ConCredencialesValidas_CreaLaSesionYRedirigeAlInicio()
    {
        using var app = new AplicacionDePrueba();
        var navegador = new Navegador(app);

        var respuesta = await navegador.IniciarSesionAsync(AplicacionDePrueba.CorreoAna, AplicacionDePrueba.ContraseniaAna);

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.Equal("/", Navegador.RutaDe(respuesta));

        // Con la cookie de sesión ya se puede entrar a una página privada.
        var perfil = await navegador.Cliente.GetAsync("/Perfil");
        Assert.Equal(HttpStatusCode.OK, perfil.StatusCode);
    }

    [Fact]
    public async Task Login_ConContraseniaIncorrecta_NoCreaSesionYMuestraMensajeGenerico()
    {
        using var app = new AplicacionDePrueba();
        var navegador = new Navegador(app);

        var respuesta = await navegador.IniciarSesionAsync(AplicacionDePrueba.CorreoAna, "ClaveEquivocada1");
        var html = await Navegador.LeerHtmlAsync(respuesta);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("Correo o contraseña incorrectos.", html);

        var perfil = await navegador.Cliente.GetAsync("/Perfil");
        Assert.Equal(HttpStatusCode.Redirect, perfil.StatusCode);
    }

    [Fact]
    public async Task Login_ConCorreoNoRegistrado_MuestraElMismoMensajeQueConContraseniaIncorrecta()
    {
        using var app = new AplicacionDePrueba();
        var navegador = new Navegador(app);

        var respuesta = await navegador.IniciarSesionAsync("nadie@prueba.com", "Cualquiera123");
        var html = await Navegador.LeerHtmlAsync(respuesta);

        // No se revela si el correo existe o no.
        Assert.Contains("Correo o contraseña incorrectos.", html);
    }

    [Fact]
    public async Task Login_ConCorreoMalEscrito_MuestraErrorDeValidacion()
    {
        using var app = new AplicacionDePrueba();
        var navegador = new Navegador(app);

        var respuesta = await navegador.IniciarSesionAsync("esto-no-es-un-correo", "Ana12345");
        var html = await Navegador.LeerHtmlAsync(respuesta);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("El correo no tiene un formato válido.", html);
    }

    [Fact]
    public async Task Login_ConReturnUrlLocal_RegresaALaPaginaSolicitada()
    {
        using var app = new AplicacionDePrueba();
        var navegador = new Navegador(app);

        var respuesta = await navegador.IniciarSesionAsync(AplicacionDePrueba.CorreoAna, AplicacionDePrueba.ContraseniaAna, "/Movimientos");

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.Equal("/Movimientos", Navegador.RutaDe(respuesta));
    }

    [Fact]
    public async Task Login_ConReturnUrlExterna_IgnoraLaDireccionYVaAlInicio()
    {
        using var app = new AplicacionDePrueba();
        var navegador = new Navegador(app);

        var respuesta = await navegador.IniciarSesionAsync(
            AplicacionDePrueba.CorreoAna, AplicacionDePrueba.ContraseniaAna, "https://sitio-malicioso.com/robar");

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.Equal("/", Navegador.RutaDe(respuesta));
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Movimientos")]
    [InlineData("/Movimientos/Create")]
    [InlineData("/Perfil")]
    [InlineData("/Perfil/CambiarContrasenia")]
    public async Task RutasPrivadas_SinSesion_RedirigenAlLogin(string ruta)
    {
        using var app = new AplicacionDePrueba();
        var navegador = new Navegador(app);

        var respuesta = await navegador.Cliente.GetAsync(ruta);

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.StartsWith("/Acceso/Login", Navegador.RutaDe(respuesta));
    }

    [Fact]
    public async Task Logout_CierraLaSesion()
    {
        using var app = new AplicacionDePrueba();
        var navegador = new Navegador(app);
        await navegador.IniciarSesionCorrectamenteAsync(AplicacionDePrueba.CorreoAna, AplicacionDePrueba.ContraseniaAna);

        var respuesta = await navegador.EnviarFormularioAsync("/Perfil", "/Acceso/Logout", new Dictionary<string, string>());
        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);

        var perfil = await navegador.Cliente.GetAsync("/Perfil");
        Assert.Equal(HttpStatusCode.Redirect, perfil.StatusCode);
    }

    [Fact]
    public async Task Registro_ConDatosValidos_CreaUsuarioConContraseniaCifradaYCuentaInicial()
    {
        using var app = new AplicacionDePrueba();
        var navegador = new Navegador(app);

        var respuesta = await navegador.EnviarFormularioAsync("/Acceso/Registro", "/Acceso/Registro", new Dictionary<string, string>
        {
            ["Nombre"] = "Carla Nueva",
            ["Correo"] = "  Carla@Prueba.com ",
            ["Contrasenia"] = "Carla2026",
            ["ConfirmarContrasenia"] = "Carla2026"
        });

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);

        var usuario = app.UsarBaseDeDatos(bd => bd.Usuarios.Include(u => u.Cuentas).Single(u => u.Correo == "carla@prueba.com"));
        Assert.NotEqual("Carla2026", usuario.Contrasenia);   // se guardó el hash, no la contraseña
        Assert.Single(usuario.Cuentas);
        Assert.Equal(0m, usuario.Cuentas.First().SaldoActual);

        // El registro deja la sesión iniciada.
        var perfil = await navegador.ObtenerHtmlAsync("/Perfil");
        Assert.Contains("Carla Nueva", perfil);
    }

    [Fact]
    public async Task Registro_ConCorreoRepetido_SeRechaza()
    {
        using var app = new AplicacionDePrueba();
        var navegador = new Navegador(app);

        var respuesta = await navegador.EnviarFormularioAsync("/Acceso/Registro", "/Acceso/Registro", new Dictionary<string, string>
        {
            ["Nombre"] = "Otra Ana",
            ["Correo"] = AplicacionDePrueba.CorreoAna,
            ["Contrasenia"] = "OtraAna123",
            ["ConfirmarContrasenia"] = "OtraAna123"
        });
        var html = await Navegador.LeerHtmlAsync(respuesta);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("Ya existe una cuenta registrada con ese correo.", html);
        Assert.Equal(1, app.UsarBaseDeDatos(bd => bd.Usuarios.Count(u => u.Correo == AplicacionDePrueba.CorreoAna)));
    }

    [Fact]
    public async Task Registro_ConContraseniaDebil_SeRechaza()
    {
        using var app = new AplicacionDePrueba();
        var navegador = new Navegador(app);

        var respuesta = await navegador.EnviarFormularioAsync("/Acceso/Registro", "/Acceso/Registro", new Dictionary<string, string>
        {
            ["Nombre"] = "Daniel",
            ["Correo"] = "daniel@prueba.com",
            ["Contrasenia"] = "solotexto",
            ["ConfirmarContrasenia"] = "solotexto"
        });
        var html = await Navegador.LeerHtmlAsync(respuesta);

        Assert.Contains("La contraseña debe incluir al menos una letra y un número.", html);
        Assert.False(app.UsarBaseDeDatos(bd => bd.Usuarios.Any(u => u.Correo == "daniel@prueba.com")));
    }
}
