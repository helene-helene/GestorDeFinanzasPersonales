using GestorFinanzas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestorFinanzas.Web.Controllers;

// Módulo de Notificaciones: alertas de presupuesto y de metas de ahorro.
public class NotificacionesController : Controller
{
    private readonly INotificacionService _servicio;

    public NotificacionesController(INotificacionService servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        await _servicio.RevisarAlertasAsync();
        var notificaciones = await _servicio.ListarAsync();

        // La lista ya se cargó con su estado original (las nuevas se ven resaltadas); ahora se marcan como leídas.
        await _servicio.MarcarTodasComoLeidasAsync();
        return View(notificaciones);
    }
}
