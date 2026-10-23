using GestorFinanzas.Web.Infrastructure;
using GestorFinanzas.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestorFinanzas.Web.Controllers;

// Recomendaciones de ahorro generadas por el módulo de IA.
public class RecomendacionesController : Controller
{
    private readonly IRecomendacionService _servicio;

    public RecomendacionesController(IRecomendacionService servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await _servicio.GenerarYListarAsync(cancellationToken));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aplicar(int id)
    {
        try
        {
            await _servicio.MarcarAplicadaAsync(id);
            TempData["Exito"] = "Marcaste la recomendación como aplicada. ¡Buen trabajo!";
        }
        catch (ReglaNegocioException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
