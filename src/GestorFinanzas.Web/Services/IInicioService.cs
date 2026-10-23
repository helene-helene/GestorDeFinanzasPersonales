using GestorFinanzas.Web.Models.Resumenes;

namespace GestorFinanzas.Web.Services;

public interface IInicioService
{
    // Resumen del mes actual para la pantalla de Inicio.
    Task<ResumenInicio> ObtenerResumenAsync(string nombreUsuario, CancellationToken cancellationToken = default);
}
