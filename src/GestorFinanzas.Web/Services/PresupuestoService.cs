using GestorFinanzas.Web.Data.Repositories;
using GestorFinanzas.Web.Infrastructure;
using GestorFinanzas.Web.Models;
using GestorFinanzas.Web.Models.Entities;
using GestorFinanzas.Web.Models.Resumenes;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanzas.Web.Services;

// Reglas de negocio de los presupuestos:
//  1. Es para una categoría de gasto (no de ingreso).
//  2. El límite es mayor que 0, con máximo 2 decimales.
//  3. El período es Mensual, Quincenal o Semanal; el sistema calcula las fechas del período actual.
//  4. No puede haber dos presupuestos de la misma categoría que empiecen el mismo día.
//  5. Lo gastado se calcula con los movimientos del usuario en esa categoría dentro del período.
public class PresupuestoService : IPresupuestoService
{
    private const decimal MontoMaximo = 9_999_999_999.99m;

    private readonly IPresupuestoRepository _presupuestos;
    private readonly ICategoriaRepository _categorias;
    private readonly IResumenRepository _resumen;
    private readonly IUnitOfWork _unidadDeTrabajo;
    private readonly ICurrentUserService _usuarioActual;

    public PresupuestoService(
        IPresupuestoRepository presupuestos,
        ICategoriaRepository categorias,
        IResumenRepository resumen,
        IUnitOfWork unidadDeTrabajo,
        ICurrentUserService usuarioActual)
    {
        _presupuestos = presupuestos;
        _categorias = categorias;
        _resumen = resumen;
        _unidadDeTrabajo = unidadDeTrabajo;
        _usuarioActual = usuarioActual;
    }

    public async Task<ResumenPresupuesto> ObtenerResumenAsync()
    {
        var usuarioId = _usuarioActual.UsuarioId;
        var vigentes = await _presupuestos.ListarVigentesAsync(usuarioId, DateTime.Today);

        var lineas = new List<LineaPresupuesto>();
        foreach (var p in vigentes)
        {
            var gastado = await _resumen.GastoDeCategoriaAsync(
                usuarioId, p.CategoriaId, p.FechaInicio.Date, p.FechaFin.Date.AddDays(1));

            lineas.Add(new LineaPresupuesto
            {
                Id = p.Id,
                CategoriaId = p.CategoriaId,
                Categoria = p.Categoria.Nombre,
                Icono = p.Categoria.Icono,
                Periodo = p.Periodo,
                FechaInicio = p.FechaInicio,
                FechaFin = p.FechaFin,
                Limite = p.MontoLimite,
                Gastado = gastado
            });
        }

        return new ResumenPresupuesto { Lineas = lineas };
    }

    public Task<IReadOnlyList<Categoria>> ListarCategoriasDeGastoAsync() =>
        _categorias.ListarAsync(TipoMovimiento.Gasto);

    public async Task<Presupuesto> CrearAsync(PresupuestoDatos datos)
    {
        var usuarioId = _usuarioActual.UsuarioId;

        if (datos.MontoLimite <= 0) throw new ReglaNegocioException("El límite debe ser mayor que 0.");
        if (datos.MontoLimite > MontoMaximo) throw new ReglaNegocioException("El límite es demasiado grande.");
        if (decimal.Round(datos.MontoLimite, 2) != datos.MontoLimite)
            throw new ReglaNegocioException("El límite admite como máximo 2 decimales.");

        var periodosValidos = new[] { PeriodoPresupuesto.Mensual, PeriodoPresupuesto.Quincenal, PeriodoPresupuesto.Semanal };
        if (!periodosValidos.Contains(datos.Periodo))
            throw new ReglaNegocioException("El período debe ser Mensual, Quincenal o Semanal.");

        var categoria = await _categorias.ObtenerAsync(datos.CategoriaId)
            ?? throw new ReglaNegocioException("La categoría seleccionada no existe.");

        if (categoria.Tipo != TipoMovimiento.Gasto)
            throw new ReglaNegocioException("Solo puedes poner un presupuesto a una categoría de gasto.");

        var (inicio, fin) = CalcularPeriodo(datos.Periodo, DateTime.Today);

        if (await _presupuestos.ExisteAsync(usuarioId, categoria.Id, inicio))
            throw new ReglaNegocioException(
                $"Ya tienes un presupuesto de {categoria.Nombre} para este período. Elimínalo si quieres crear otro.");

        var presupuesto = new Presupuesto
        {
            UsuarioId = usuarioId,
            CategoriaId = categoria.Id,
            MontoLimite = datos.MontoLimite,
            Periodo = datos.Periodo,
            FechaInicio = inicio,
            FechaFin = fin
        };

        await _presupuestos.AgregarAsync(presupuesto);
        await GuardarAsync();
        return presupuesto;
    }

    public async Task EliminarAsync(int id)
    {
        var presupuesto = await _presupuestos.ObtenerAsync(id, _usuarioActual.UsuarioId)
            ?? throw new ReglaNegocioException("El presupuesto que intentas eliminar no existe.");

        _presupuestos.Eliminar(presupuesto);
        await GuardarAsync();
    }

    // Primer y último día del período que contiene la fecha indicada.
    public static (DateTime Inicio, DateTime Fin) CalcularPeriodo(string periodo, DateTime fecha)
    {
        var dia = fecha.Date;
        var primerDiaDelMes = new DateTime(dia.Year, dia.Month, 1);
        var ultimoDiaDelMes = primerDiaDelMes.AddMonths(1).AddDays(-1);

        return periodo switch
        {
            PeriodoPresupuesto.Semanal => SemanaDe(dia),
            PeriodoPresupuesto.Quincenal => dia.Day <= 15
                ? (primerDiaDelMes, new DateTime(dia.Year, dia.Month, 15))
                : (new DateTime(dia.Year, dia.Month, 16), ultimoDiaDelMes),
            _ => (primerDiaDelMes, ultimoDiaDelMes)
        };
    }

    // La semana va de lunes a domingo.
    private static (DateTime Inicio, DateTime Fin) SemanaDe(DateTime dia)
    {
        var desdeElLunes = ((int)dia.DayOfWeek + 6) % 7;
        var lunes = dia.AddDays(-desdeElLunes);
        return (lunes, lunes.AddDays(6));
    }

    private async Task GuardarAsync()
    {
        try
        {
            await _unidadDeTrabajo.GuardarCambiosAsync();
        }
        catch (DbUpdateException)
        {
            throw new ReglaNegocioException("No se pudo guardar el presupuesto. Revisa que no esté repetido e inténtalo de nuevo.");
        }
    }
}
