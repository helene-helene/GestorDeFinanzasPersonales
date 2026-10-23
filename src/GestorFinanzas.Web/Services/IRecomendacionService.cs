using GestorFinanzas.Web.Models.Entities;

namespace GestorFinanzas.Web.Services;

public interface IRecomendacionService
{
    // Genera las recomendaciones de hoy, guarda las nuevas y devuelve las más recientes.
    Task<IReadOnlyList<RecomendacionIA>> GenerarYListarAsync(CancellationToken cancellationToken = default);

    Task MarcarAplicadaAsync(int id);
}
