using GestorFinanzas.Web.Data.Repositories;
using GestorFinanzas.Web.Infrastructure;
using GestorFinanzas.Web.Models;
using GestorFinanzas.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanzas.Web.Services;

/// <summary>
/// Reglas de negocio de los presupuestos:
///  1. El monto límite es mayor que 0, con máximo 2 decimales.
///  2. El período es Mensual, Quincenal o Semanal y la fecha de fin se calcula a partir de la de inicio.
///  3. Solo se puede presupuestar una categoría de gasto.
///  4. No puede haber dos presupuestos de la misma categoría con fechas que se crucen.
///  5. Lo gastado es la suma de los gastos de esa categoría dentro del período.
///  6. Al llegar al 80% del límite queda en Alerta y al llegar al 100% queda Excedido.
/// </summary>
public class PresupuestoService : IPresupuestoService
{
    private const decimal MontoMaximo = 9_999_999_999.99m;
    private const decimal PorcentajeAlerta = 80m;

    private static readonly string[] Periodos =
    {
        PeriodoPresupuesto.Mensual,
        PeriodoPresupuesto.Quincenal,
        PeriodoPresupuesto.Semanal
    };

    private readonly IPresupuestoRepository _presupuestos;
    private readonly ICategoriaRepository _categorias;
    private readonly IUnitOfWork _unidadDeTrabajo;
    private readonly ICurrentUserService _usuarioActual;

    public PresupuestoService(
        IPresupuestoRepository presupuestos,
        ICategoriaRepository categorias,
        IUnitOfWork unidadDeTrabajo,
        ICurrentUserService usuarioActual)
    {
        _presupuestos = presupuestos;
        _categorias = categorias;
        _unidadDeTrabajo = unidadDeTrabajo;
        _usuarioActual = usuarioActual;
    }

    // ---------------------------------------------------------------- Consultas

    public async Task<IReadOnlyList<PresupuestoResumen>> ListarAsync()
    {
        var usuarioId = _usuarioActual.UsuarioId;
        var presupuestos = await _presupuestos.ListarAsync(usuarioId);
        var hoy = DateTime.Today;

        var resumenes = new List<PresupuestoResumen>();
        foreach (var presupuesto in presupuestos)
        {
            var gastado = await _presupuestos.TotalGastadoAsync(
                usuarioId, presupuesto.CategoriaId, presupuesto.FechaInicio, presupuesto.FechaFin);

            resumenes.Add(Resumir(presupuesto, gastado, hoy));
        }

        return resumenes;
    }

    public Task<Presupuesto?> ObtenerAsync(int id) =>
        _presupuestos.ObtenerAsync(id, _usuarioActual.UsuarioId);

    public Task<IReadOnlyList<Categoria>> ListarCategoriasDeGastoAsync() =>
        _categorias.ListarAsync(TipoMovimiento.Gasto);

    // ---------------------------------------------------------------- Crear

    public async Task<Presupuesto> CrearAsync(PresupuestoDatos datos)
    {
        var usuarioId = _usuarioActual.UsuarioId;

        Validar(datos);
        await ValidarCategoriaAsync(datos.CategoriaId);

        var inicio = datos.FechaInicio.Date;
        var fin = CalcularFechaFin(datos.Periodo, inicio);
        await ValidarSolapamientoAsync(usuarioId, datos.CategoriaId, inicio, fin, null);

        var presupuesto = new Presupuesto
        {
            UsuarioId = usuarioId,
            CategoriaId = datos.CategoriaId,
            MontoLimite = datos.MontoLimite,
            Periodo = datos.Periodo,
            FechaInicio = inicio,
            FechaFin = fin
        };

        await _presupuestos.AgregarAsync(presupuesto);
        await GuardarAsync();
        return presupuesto;
    }

    // ---------------------------------------------------------------- Actualizar

    public async Task<Presupuesto> ActualizarAsync(int id, PresupuestoDatos datos)
    {
        var usuarioId = _usuarioActual.UsuarioId;

        var presupuesto = await _presupuestos.ObtenerAsync(id, usuarioId, conSeguimiento: true)
            ?? throw new ReglaNegocioException("El presupuesto que intentas modificar no existe.");

        Validar(datos);
        await ValidarCategoriaAsync(datos.CategoriaId);

        var inicio = datos.FechaInicio.Date;
        var fin = CalcularFechaFin(datos.Periodo, inicio);
        await ValidarSolapamientoAsync(usuarioId, datos.CategoriaId, inicio, fin, id);

        presupuesto.CategoriaId = datos.CategoriaId;
        presupuesto.MontoLimite = datos.MontoLimite;
        presupuesto.Periodo = datos.Periodo;
        presupuesto.FechaInicio = inicio;
        presupuesto.FechaFin = fin;

        await GuardarAsync();
        return presupuesto;
    }

    // ---------------------------------------------------------------- Eliminar

    public async Task EliminarAsync(int id)
    {
        var presupuesto = await _presupuestos.ObtenerAsync(id, _usuarioActual.UsuarioId, conSeguimiento: true)
            ?? throw new ReglaNegocioException("El presupuesto que intentas eliminar no existe.");

        _presupuestos.Eliminar(presupuesto);
        await GuardarAsync();
    }

    // ---------------------------------------------------------------- Reglas internas

    private static void Validar(PresupuestoDatos datos)
    {
        if (datos.MontoLimite <= 0)
        {
            throw new ReglaNegocioException("El monto límite debe ser mayor que 0.");
        }

        if (datos.MontoLimite > MontoMaximo)
        {
            throw new ReglaNegocioException("El monto límite es demasiado grande.");
        }

        if (decimal.Round(datos.MontoLimite, 2) != datos.MontoLimite)
        {
            throw new ReglaNegocioException("El monto límite admite como máximo 2 decimales.");
        }

        if (!Periodos.Contains(datos.Periodo))
        {
            throw new ReglaNegocioException("El período debe ser Mensual, Quincenal o Semanal.");
        }

        if (datos.FechaInicio.Date < new DateTime(2000, 1, 1))
        {
            throw new ReglaNegocioException("La fecha de inicio no es válida.");
        }
    }

    private async Task ValidarCategoriaAsync(int categoriaId)
    {
        var categoria = await _categorias.ObtenerAsync(categoriaId)
            ?? throw new ReglaNegocioException("La categoría seleccionada no existe.");

        if (categoria.Tipo != TipoMovimiento.Gasto)
        {
            throw new ReglaNegocioException(
                $"La categoría '{categoria.Nombre}' es de ingreso. Solo se pueden presupuestar categorías de gasto.");
        }
    }

    private async Task ValidarSolapamientoAsync(int usuarioId, int categoriaId, DateTime inicio, DateTime fin, int? excluirId)
    {
        if (await _presupuestos.HaySolapamientoAsync(usuarioId, categoriaId, inicio, fin, excluirId))
        {
            throw new ReglaNegocioException(
                "Ya tienes un presupuesto para esa categoría que se cruza con esas fechas.");
        }
    }

    private static DateTime CalcularFechaFin(string periodo, DateTime inicio) => periodo switch
    {
        PeriodoPresupuesto.Semanal => inicio.AddDays(6),
        PeriodoPresupuesto.Quincenal => inicio.AddDays(14),
        _ => inicio.AddMonths(1).AddDays(-1)
    };

    private static PresupuestoResumen Resumir(Presupuesto presupuesto, decimal gastado, DateTime hoy)
    {
        var porcentaje = presupuesto.MontoLimite > 0 ? gastado / presupuesto.MontoLimite * 100m : 0m;

        var estado = EstadoPresupuesto.Normal;
        if (porcentaje >= 100m)
        {
            estado = EstadoPresupuesto.Excedido;
        }
        else if (porcentaje >= PorcentajeAlerta)
        {
            estado = EstadoPresupuesto.Alerta;
        }

        return new PresupuestoResumen
        {
            Presupuesto = presupuesto,
            Gastado = gastado,
            Porcentaje = decimal.Round(porcentaje, 1),
            Estado = estado,
            Vigente = presupuesto.FechaInicio.Date <= hoy && presupuesto.FechaFin.Date >= hoy
        };
    }

    private async Task GuardarAsync()
    {
        try
        {
            await _unidadDeTrabajo.GuardarCambiosAsync();
        }
        catch (DbUpdateException)
        {
            throw new ReglaNegocioException(
                "No se pudo guardar el presupuesto. Revisa que no exista otro igual para esa categoría y esas fechas.");
        }
    }
}
