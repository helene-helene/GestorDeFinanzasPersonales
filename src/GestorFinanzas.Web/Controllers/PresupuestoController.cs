using GestorFinanzas.Web.Infrastructure;
using GestorFinanzas.Web.Models;
using GestorFinanzas.Web.Services;
using GestorFinanzas.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GestorFinanzas.Web.Controllers;

// Módulo Presupuesto: límites de gasto por categoría y lo que el usuario lleva gastado.
public class PresupuestoController : Controller
{
    private readonly IPresupuestoService _servicio;

    public PresupuestoController(IPresupuestoService servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<IActionResult> Index() => View(await _servicio.ObtenerResumenAsync());

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var modelo = new PresupuestoFormViewModel { Periodo = PeriodoPresupuesto.Mensual };
        modelo.Categorias = await _servicio.ListarCategoriasDeGastoAsync();
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PresupuestoFormViewModel modelo)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _servicio.CrearAsync(new PresupuestoDatos
                {
                    CategoriaId = modelo.CategoriaId!.Value,
                    MontoLimite = modelo.MontoLimite!.Value,
                    Periodo = modelo.Periodo
                });
                TempData["Exito"] = "El presupuesto se guardó correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (ReglaNegocioException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }

        modelo.Categorias = await _servicio.ListarCategoriasDeGastoAsync();
        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _servicio.EliminarAsync(id);
            TempData["Exito"] = "El presupuesto se eliminó.";
        }
        catch (ReglaNegocioException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
