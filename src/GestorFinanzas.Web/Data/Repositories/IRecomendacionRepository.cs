using GestorFinanzas.Web.Models.Entities;

namespace GestorFinanzas.Web.Data.Repositories;

public interface IRecomendacionRepository
{
    Task<IReadOnlyList<RecomendacionIA>> ListarAsync(int usuarioId, int maximo);
    Task<RecomendacionIA?> ObtenerAsync(int id, int usuarioId);

    // Indica si ya hay una recomendación de ese tipo cuya descripción empiece con el texto dado desde la fecha indicada.
    Task<bool> ExisteAsync(int usuarioId, string tipo, string prefijoDescripcion, DateTime desde);

    Task AgregarAsync(RecomendacionIA recomendacion);
}
