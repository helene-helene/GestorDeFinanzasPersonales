using GestorFinanzas.Web.Infrastructure;
using GestorFinanzas.Web.Models;
using GestorFinanzas.Web.Services;
using GestorFinanzas.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GestorFinanzas.Web.Controllers;

/// <summary>Módulo de presupuestos: límites de gasto por categoría con su avance y alertas.</summary>
public class PresupuestoController : Controller
{
    private readonly IPresupuestoService _servicio;

    public PresupuestoController(IPresupuestoService servicio)
    {
        _servicio = servicio;
    }

    // GET /Presupuesto
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View(await _servicio.ListarAsync());
    }

    // GET /Presupuesto/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var hoy = DateTime.Today;
        var modelo = new PresupuestoFormViewModel
        {
            Periodo = PeriodoPresupuesto.Mensual,
            FechaInicio = new DateTime(hoy.Year, hoy.Month, 1)
        };
        await CargarCategoriasAsync(modelo);
        return View(modelo);
    }

    // POST /Presupuesto/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PresupuestoFormViewModel modelo)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _servicio.CrearAsync(ADatos(modelo));
                TempData["Exito"] = "El presupuesto se creó correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (ReglaNegocioException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }

        await CargarCategoriasAsync(modelo);
        return View(modelo);
    }

    // GET /Presupuesto/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var presupuesto = await _servicio.ObtenerAsync(id);
        if (presupuesto is null)
        {
            return NotFound();
        }

        var modelo = new PresupuestoFormViewModel
        {
            Id = presupuesto.Id,
            CategoriaId = presupuesto.CategoriaId,
            MontoLimite = presupuesto.MontoLimite,
            Periodo = presupuesto.Periodo,
            FechaInicio = presupuesto.FechaInicio
        };
        await CargarCategoriasAsync(modelo);
        return View(modelo);
    }

    // POST /Presupuesto/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PresupuestoFormViewModel modelo)
    {
        if (id != modelo.Id)
        {
            return BadRequest();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _servicio.ActualizarAsync(id, ADatos(modelo));
                TempData["Exito"] = "El presupuesto se actualizó correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (ReglaNegocioException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }

        await CargarCategoriasAsync(modelo);
        return View(modelo);
    }

    // POST /Presupuesto/Delete/5  (la confirmación se pide en el navegador antes de enviar)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _servicio.EliminarAsync(id);
            TempData["Exito"] = "El presupuesto se eliminó correctamente.";
        }
        catch (ReglaNegocioException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task CargarCategoriasAsync(PresupuestoFormViewModel modelo)
    {
        modelo.Categorias = await _servicio.ListarCategoriasDeGastoAsync();
    }

    private static PresupuestoDatos ADatos(PresupuestoFormViewModel modelo) => new()
    {
        CategoriaId = modelo.CategoriaId!.Value,
        MontoLimite = modelo.MontoLimite!.Value,
        Periodo = modelo.Periodo,
        FechaInicio = modelo.FechaInicio!.Value
    };
}
