using GestorFinanzas.Web.Infrastructure;
using GestorFinanzas.Web.Models;
using GestorFinanzas.Web.Models.Filtros;
using GestorFinanzas.Web.Services;
using GestorFinanzas.Web.Services.IA;
using GestorFinanzas.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestorFinanzas.Web.Controllers;

// Módulo "Registrar movimiento": historial y CRUD de ingresos y gastos.
[Authorize]
public class MovimientosController : Controller
{
    private readonly ITransaccionService _servicio;
    private readonly IServicioIA _ia;

    public MovimientosController(ITransaccionService servicio, IServicioIA ia)
    {
        _servicio = servicio;
        _ia = ia;
    }

    // GET /Movimientos
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] FiltroMovimientos filtro)
    {
        var modelo = new MovimientoIndexViewModel
        {
            Filtro = filtro,
            Resultado = await _servicio.ListarAsync(filtro),   // normaliza el filtro (página, tamaño, tipo)
            Resumen = await _servicio.ResumirAsync(filtro),
            Cuentas = await _servicio.ListarCuentasAsync(),
            Categorias = await _servicio.ListarCategoriasAsync()
        };

        return View(modelo);
    }

    // GET /Movimientos/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var modelo = new MovimientoFormViewModel
        {
            Tipo = TipoMovimiento.Gasto,
            Fecha = DateTime.Today
        };
        await CargarListasAsync(modelo);
        return View(modelo);
    }

    // POST /Movimientos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MovimientoFormViewModel modelo)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _servicio.CrearAsync(ADatos(modelo));
                TempData["Exito"] = "El movimiento se registró correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (ReglaNegocioException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }

        await CargarListasAsync(modelo);
        return View(modelo);
    }

    // GET /Movimientos/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var transaccion = await _servicio.ObtenerAsync(id);
        if (transaccion is null)
        {
            return NotFound();
        }

        var modelo = new MovimientoFormViewModel
        {
            Id = transaccion.Id,
            Tipo = transaccion.Tipo,
            CuentaId = transaccion.CuentaId,
            CategoriaId = transaccion.CategoriaId,
            Monto = transaccion.Monto,
            Fecha = transaccion.Fecha,
            Descripcion = transaccion.Descripcion
        };
        await CargarListasAsync(modelo);
        return View(modelo);
    }

    // POST /Movimientos/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MovimientoFormViewModel modelo)
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
                TempData["Exito"] = "El movimiento se actualizó correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (ReglaNegocioException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }

        await CargarListasAsync(modelo);
        return View(modelo);
    }

    // GET /Movimientos/Delete/5  (pantalla de confirmación)
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var transaccion = await _servicio.ObtenerAsync(id);
        if (transaccion is null)
        {
            return NotFound();
        }

        return View(transaccion);
    }

    // POST /Movimientos/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _servicio.EliminarAsync(id);
            TempData["Exito"] = "El movimiento se eliminó y el saldo de la cuenta se ajustó.";
        }
        catch (ReglaNegocioException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // POST /Movimientos/SugerirCategoria  (alimenta la «Sugerencia inteligente» del formulario)
    [HttpPost]
    public async Task<IActionResult> SugerirCategoria(
        [FromBody] SugerenciaCategoriaSolicitud solicitud, CancellationToken cancellationToken)
    {
        var sugerencia = await _ia.ClasificarGastoAsync(solicitud?.Descripcion ?? string.Empty, cancellationToken);
        if (sugerencia is null) return Json(new { encontrada = false });

        return Json(new
        {
            encontrada = true,
            categoriaId = sugerencia.CategoriaId,
            categoria = sugerencia.Categoria,
            motivo = sugerencia.Motivo,
            proveedor = sugerencia.Proveedor
        });
    }

    private async Task CargarListasAsync(MovimientoFormViewModel modelo)
    {
        modelo.Cuentas = await _servicio.ListarCuentasAsync();
        modelo.Categorias = await _servicio.ListarCategoriasAsync();
    }

    private static MovimientoDatos ADatos(MovimientoFormViewModel modelo) => new()
    {
        CuentaId = modelo.CuentaId!.Value,
        CategoriaId = modelo.CategoriaId!.Value,
        Tipo = modelo.Tipo,
        Monto = modelo.Monto!.Value,
        Fecha = modelo.Fecha!.Value,
        Descripcion = modelo.Descripcion
    };
}
