using GestorFinanzas.Web.Models.Entities;

namespace GestorFinanzas.Web.Services;

public interface INotificacionService
{
    // Revisa presupuestos y metas del usuario y crea las alertas que correspondan, sin repetir las ya enviadas.
    Task RevisarAlertasAsync();
    Task<IReadOnlyList<Notificacion>> ListarAsync();
    Task<int> ContarNoLeidasAsync();
    Task MarcarTodasComoLeidasAsync();
}
