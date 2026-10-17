using System.Net;
using GestorFinanzas.Tests.Infraestructura;

namespace GestorFinanzas.Tests;

/// <summary>
/// Pruebas del módulo de movimientos (Persona 1) ya integrado con el login:
/// cada estudiante solo ve y modifica lo suyo, y los datos válidos se guardan.
/// </summary>
public class MovimientosIntegracionTests
{
    private static string Hoy => DateTime.Today.ToString("yyyy-MM-dd");

    [Fact]
    public async Task Historial_SoloMuestraLosMovimientosDelUsuarioQueInicioSesion()
    {
        using var app = new AplicacionDePrueba();
        var navegador = new Navegador(app);
        await navegador.IniciarSesionCorrectamenteAsync(AplicacionDePrueba.CorreoAna, AplicacionDePrueba.ContraseniaAna);

        var html = await navegador.ObtenerHtmlAsync("/Movimientos");

        Assert.Contains("Almuerzo de Ana", html);
        Assert.DoesNotContain("Libro de Beto", html);
    }

    [Fact]
    public async Task EditarMovimiento_DeOtroUsuario_DevuelveNoEncontrado()
    {
        using var app = new AplicacionDePrueba();
        var navegador = new Navegador(app);
        await navegador.IniciarSesionCorrectamenteAsync(AplicacionDePrueba.CorreoAna, AplicacionDePrueba.ContraseniaAna);

        var respuesta = await navegador.Cliente.GetAsync($"/Movimientos/Edit/{app.MovimientoBetoId}");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task ModificarMovimiento_DeOtroUsuario_NoCambiaNada()
    {
        using var app = new AplicacionDePrueba();
        var navegador = new Navegador(app);
        await navegador.IniciarSesionCorrectamenteAsync(AplicacionDePrueba.CorreoAna, AplicacionDePrueba.ContraseniaAna);

        // Ana usa el formulario de su propio movimiento pero lo envía con el Id del movimiento de Beto.
        var respuesta = await navegador.EnviarFormularioAsync(
            $"/Movimientos/Edit/{app.MovimientoAnaId}",
            $"/Movimientos/Edit/{app.MovimientoBetoId}",
            new Dictionary<string, string>
            {
                ["Id"] = app.MovimientoBetoId.ToString(),
                ["Tipo"] = "Gasto",
                ["CuentaId"] = app.CuentaAnaId.ToString(),
                ["CategoriaId"] = "1",
                ["Monto"] = "1.00",
                ["Fecha"] = Hoy,
                ["Descripcion"] = "Modificado por Ana"
            });

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var movimientoBeto = app.UsarBaseDeDatos(bd => bd.Transacciones.Single(t => t.Id == app.MovimientoBetoId));
        Assert.Equal(20m, movimientoBeto.Monto);
        Assert.Equal("Libro de Beto", movimientoBeto.Descripcion);
    }

    [Fact]
    public async Task EliminarMovimiento_DeOtroUsuario_NoSeElimina()
    {
        using var app = new AplicacionDePrueba();
        var navegador = new Navegador(app);
        await navegador.IniciarSesionCorrectamenteAsync(AplicacionDePrueba.CorreoAna, AplicacionDePrueba.ContraseniaAna);

        await navegador.EnviarFormularioAsync(
            $"/Movimientos/Delete/{app.MovimientoAnaId}",
            $"/Movimientos/Delete/{app.MovimientoBetoId}",
            new Dictionary<string, string>());

        Assert.True(app.UsarBaseDeDatos(bd => bd.Transacciones.Any(t => t.Id == app.MovimientoBetoId)));
        Assert.Equal(80m, app.UsarBaseDeDatos(bd => bd.Cuentas.Single(c => c.Id == app.CuentaBetoId).SaldoActual));
    }

    [Fact]
    public async Task RegistrarMovimiento_ConDatosValidos_SeGuardaParaElUsuarioYActualizaElSaldo()
    {
        using var app = new AplicacionDePrueba();
        var navegador = new Navegador(app);
        await navegador.IniciarSesionCorrectamenteAsync(AplicacionDePrueba.CorreoAna, AplicacionDePrueba.ContraseniaAna);

        var respuesta = await navegador.EnviarFormularioAsync("/Movimientos/Create", "/Movimientos/Create", new Dictionary<string, string>
        {
            ["Tipo"] = "Gasto",
            ["CuentaId"] = app.CuentaAnaId.ToString(),
            ["CategoriaId"] = "2",
            ["Monto"] = "15.50",
            ["Fecha"] = Hoy,
            ["Descripcion"] = "Pasajes de la semana"
        });

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);

        var guardado = app.UsarBaseDeDatos(bd => bd.Transacciones.Single(t => t.Descripcion == "Pasajes de la semana"));
        Assert.Equal(app.CuentaAnaId, guardado.CuentaId);
        Assert.Equal(15.50m, guardado.Monto);
        Assert.Equal(79.50m, app.UsarBaseDeDatos(bd => bd.Cuentas.Single(c => c.Id == app.CuentaAnaId).SaldoActual));

        // Beto no ve el movimiento nuevo de Ana.
        var navegadorBeto = new Navegador(app);
        await navegadorBeto.IniciarSesionCorrectamenteAsync(AplicacionDePrueba.CorreoBeto, AplicacionDePrueba.ContraseniaBeto);
        var htmlBeto = await navegadorBeto.ObtenerHtmlAsync("/Movimientos");
        Assert.DoesNotContain("Pasajes de la semana", htmlBeto);
    }

    [Fact]
    public async Task RegistrarMovimiento_EnLaCuentaDeOtroUsuario_SeRechaza()
    {
        using var app = new AplicacionDePrueba();
        var navegador = new Navegador(app);
        await navegador.IniciarSesionCorrectamenteAsync(AplicacionDePrueba.CorreoAna, AplicacionDePrueba.ContraseniaAna);

        var respuesta = await navegador.EnviarFormularioAsync("/Movimientos/Create", "/Movimientos/Create", new Dictionary<string, string>
        {
            ["Tipo"] = "Gasto",
            ["CuentaId"] = app.CuentaBetoId.ToString(),
            ["CategoriaId"] = "1",
            ["Monto"] = "10.00",
            ["Fecha"] = Hoy,
            ["Descripcion"] = "Gasto en cuenta ajena"
        });
        var html = await Navegador.LeerHtmlAsync(respuesta);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("La cuenta seleccionada no existe.", html);
        Assert.False(app.UsarBaseDeDatos(bd => bd.Transacciones.Any(t => t.Descripcion == "Gasto en cuenta ajena")));
        Assert.Equal(80m, app.UsarBaseDeDatos(bd => bd.Cuentas.Single(c => c.Id == app.CuentaBetoId).SaldoActual));
    }

    [Fact]
    public async Task RegistrarMovimiento_ConDatosInvalidos_NoSeGuarda()
    {
        using var app = new AplicacionDePrueba();
        var navegador = new Navegador(app);
        await navegador.IniciarSesionCorrectamenteAsync(AplicacionDePrueba.CorreoAna, AplicacionDePrueba.ContraseniaAna);
        var totalAntes = app.UsarBaseDeDatos(bd => bd.Transacciones.Count());

        var respuesta = await navegador.EnviarFormularioAsync("/Movimientos/Create", "/Movimientos/Create", new Dictionary<string, string>
        {
            ["Tipo"] = "Gasto",
            ["CuentaId"] = app.CuentaAnaId.ToString(),
            ["CategoriaId"] = "1",
            ["Monto"] = "0",
            ["Fecha"] = Hoy
        });
        var html = await Navegador.LeerHtmlAsync(respuesta);

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains("El monto debe ser mayor que 0.", html);
        Assert.Equal(totalAntes, app.UsarBaseDeDatos(bd => bd.Transacciones.Count()));
    }

    [Fact]
    public async Task RegistrarMovimiento_SinIniciarSesion_SeRedirigeAlLoginYNoSeGuarda()
    {
        using var app = new AplicacionDePrueba();
        var navegador = new Navegador(app);
        var totalAntes = app.UsarBaseDeDatos(bd => bd.Transacciones.Count());

        // El token se toma de la página de login, que sí es pública.
        var respuesta = await navegador.EnviarFormularioAsync("/Acceso/Login", "/Movimientos/Create", new Dictionary<string, string>
        {
            ["Tipo"] = "Gasto",
            ["CuentaId"] = app.CuentaAnaId.ToString(),
            ["CategoriaId"] = "1",
            ["Monto"] = "10.00",
            ["Fecha"] = Hoy
        });

        Assert.Equal(HttpStatusCode.Redirect, respuesta.StatusCode);
        Assert.StartsWith("/Acceso/Login", Navegador.RutaDe(respuesta));
        Assert.Equal(totalAntes, app.UsarBaseDeDatos(bd => bd.Transacciones.Count()));
    }
}
