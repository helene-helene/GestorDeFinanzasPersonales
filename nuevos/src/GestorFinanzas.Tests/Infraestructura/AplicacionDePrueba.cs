using GestorFinanzas.Web.Data;
using GestorFinanzas.Web.Models;
using GestorFinanzas.Web.Models.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace GestorFinanzas.Tests.Infraestructura;

/// <summary>
/// Levanta la aplicación completa en memoria (controladores, servicios, repositorios, cookies y filtros)
/// pero cambia SQL Server por una base de datos en memoria, nueva y vacía para cada prueba.
/// Así las pruebas nunca tocan la base de datos real.
/// </summary>
public class AplicacionDePrueba : WebApplicationFactory<Program>
{
    public const string CorreoAna = "ana@prueba.com";
    public const string ContraseniaAna = "Ana12345";
    public const string CorreoBeto = "beto@prueba.com";
    public const string ContraseniaBeto = "Beto12345";

    // Cada instancia usa su propia base en memoria, así una prueba no afecta a otra.
    private readonly string _nombreBaseDeDatos = "GestorFinanzasPruebas_" + Guid.NewGuid();

    public int AnaId { get; private set; }
    public int BetoId { get; private set; }
    public int CuentaAnaId { get; private set; }
    public int CuentaBetoId { get; private set; }
    public int MovimientoAnaId { get; private set; }
    public int MovimientoBetoId { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // No se aplican migraciones ni datos demo: la base en memoria se prepara abajo.
        builder.ConfigureAppConfiguration((_, configuracion) =>
        {
            configuracion.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["BaseDeDatos:AplicarMigracionesAlIniciar"] = "false",
                ["BaseDeDatos:CargarDatosDemo"] = "false"
            });
        });

        builder.ConfigureTestServices(servicios =>
        {
            servicios.RemoveAll<DbContextOptions<AppDbContext>>();
            servicios.AddDbContext<AppDbContext>(opciones => opciones.UseInMemoryDatabase(_nombreBaseDeDatos));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var alcance = host.Services.CreateScope();
        var contexto = alcance.ServiceProvider.GetRequiredService<AppDbContext>();
        contexto.Database.EnsureCreated();   // crea las tablas y las 14 categorías iniciales
        CargarDatos(contexto);

        return host;
    }

    /// <summary>Permite revisar o preparar datos directamente en la base de prueba.</summary>
    public T UsarBaseDeDatos<T>(Func<AppDbContext, T> accion)
    {
        using var alcance = Services.CreateScope();
        var contexto = alcance.ServiceProvider.GetRequiredService<AppDbContext>();
        return accion(contexto);
    }

    /// <summary>Dos estudiantes, cada uno con una cuenta de $100 y un gasto propio.</summary>
    private void CargarDatos(AppDbContext contexto)
    {
        var hasher = new PasswordHasher<Usuario>();

        var ana = new Usuario { Nombre = "Ana Prueba", Correo = CorreoAna, FechaRegistro = DateTime.Today };
        ana.Contrasenia = hasher.HashPassword(ana, ContraseniaAna);

        var beto = new Usuario { Nombre = "Beto Prueba", Correo = CorreoBeto, FechaRegistro = DateTime.Today };
        beto.Contrasenia = hasher.HashPassword(beto, ContraseniaBeto);

        var cuentaAna = new Cuenta { Usuario = ana, NombreCuenta = "Efectivo", TipoCuenta = TipoCuenta.Efectivo, SaldoActual = 95m };
        var cuentaBeto = new Cuenta { Usuario = beto, NombreCuenta = "Efectivo", TipoCuenta = TipoCuenta.Efectivo, SaldoActual = 80m };

        // Categoría 1 = Alimentación (Gasto), 5 = Materiales de estudio (Gasto).
        var movimientoAna = new Transaccion
        {
            Cuenta = cuentaAna, CategoriaId = 1, Monto = 5m, Tipo = TipoMovimiento.Gasto,
            Fecha = DateTime.Today, Descripcion = "Almuerzo de Ana"
        };
        var movimientoBeto = new Transaccion
        {
            Cuenta = cuentaBeto, CategoriaId = 5, Monto = 20m, Tipo = TipoMovimiento.Gasto,
            Fecha = DateTime.Today, Descripcion = "Libro de Beto"
        };

        contexto.AddRange(ana, beto, cuentaAna, cuentaBeto, movimientoAna, movimientoBeto);
        contexto.SaveChanges();

        AnaId = ana.Id;
        BetoId = beto.Id;
        CuentaAnaId = cuentaAna.Id;
        CuentaBetoId = cuentaBeto.Id;
        MovimientoAnaId = movimientoAna.Id;
        MovimientoBetoId = movimientoBeto.Id;
    }
}
