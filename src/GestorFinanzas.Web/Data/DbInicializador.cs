using GestorFinanzas.Web.Data.Seed;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanzas.Web.Data;

// Prepara la base de datos al arrancar la aplicación: aplica las migraciones pendientes y,
// si está activado, carga los datos de demostración. Reintenta varias veces porque en Docker
// el contenedor de SQL Server puede tardar unos segundos en aceptar conexiones.
public static class DbInicializador
{
    private const int IntentosMaximos = 10;

    public static async Task InicializarBaseDeDatosAsync(this WebApplication app)
    {
        var seccion = app.Configuration.GetSection("BaseDeDatos");
        var aplicarMigraciones = seccion.GetValue("AplicarMigracionesAlIniciar", true);
        var cargarDatosDemo = seccion.GetValue("CargarDatosDemo", false);

        if (!aplicarMigraciones && !cargarDatosDemo)
        {
            return;
        }

        using var alcance = app.Services.CreateScope();
        var contexto = alcance.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = alcance.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("InicializacionBD");

        for (var intento = 1; ; intento++)
        {
            try
            {
                if (aplicarMigraciones)
                {
                    await contexto.Database.MigrateAsync();
                }

                if (cargarDatosDemo)
                {
                    await DbSeeder.CargarDatosDemoAsync(contexto);
                }

                return;
            }
            catch (Exception ex) when (intento < IntentosMaximos)
            {
                logger.LogWarning(
                    ex,
                    "No se pudo preparar la base de datos (intento {Intento} de {Total}). Se reintenta en 3 segundos.",
                    intento,
                    IntentosMaximos);
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }
    }
}
