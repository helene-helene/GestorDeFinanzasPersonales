using GestorFinanzas.Web.Models.Entities;

namespace GestorFinanzas.Web.Data.Repositories;

public interface INotificacionRepository
{
    Task<IReadOnlyList<Notificacion>> ListarAsync(int usuarioId, int maximo);
    Task<int> ContarNoLeidasAsync(int usuarioId);

    // Indica si ya se envió una alerta de ese tipo cuyo mensaje empiece con el texto dado desde la fecha indicada.
    Task<bool> ExisteAsync(int usuarioId, string tipo, string prefijoMensaje, DateTime desde);

    Task<IReadOnlyList<Notificacion>> ListarNoLeidasConSeguimientoAsync(int usuarioId);
    Task AgregarAsync(Notificacion notificacion);
}
