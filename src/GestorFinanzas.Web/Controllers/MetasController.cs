using GestorFinanzas.Web.Infrastructure;
using GestorFinanzas.Web.Services;
using GestorFinanzas.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GestorFinanzas.Web.Controllers;

// Pantalla «Metas y ahorros»: crear metas, registrar aportes y ver el avance.
public class MetasController : Controller
{
    private readonly IMetaAhorroService _servicio;

    public MetasController(IMetaAhorroService servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<IActionResult> Index() => View(await _servicio.ListarAsync());

    [HttpGet]
    public IActionResult Create() => View(new MetaFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MetaFormViewModel modelo)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _servicio.CrearAsync(new MetaDatos
                {
                    Nombre = modelo.Nombre,
                    MontoObjetivo = modelo.MontoObjetivo!.Value,
                    MontoActual = modelo.MontoActual ?? 0m,
                    FechaLimite = modelo.FechaLimite
                });
                TempData["Exito"] = "La meta se guardó. ¡Cada aporte cuenta!";
                return RedirectToAction(nameof(Index));
            }
            catch (ReglaNegocioException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aportar(int id, decimal monto)
    {
        try
        {
            var meta = await _servicio.AportarAsync(id, monto);
            TempData["Exito"] = meta.Completa
                ? $"¡Completaste tu meta «{meta.Nombre}»!"
                : $"Aporte registrado en «{meta.Nombre}».";
        }
        catch (ReglaNegocioException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _servicio.EliminarAsync(id);
            TempData["Exito"] = "La meta se eliminó.";
        }
        catch (ReglaNegocioException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
