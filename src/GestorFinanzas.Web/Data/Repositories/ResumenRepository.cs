using GestorFinanzas.Web.Models;
using GestorFinanzas.Web.Models.Filtros;
using GestorFinanzas.Web.Models.Resumenes;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanzas.Web.Data.Repositories;

public class ResumenRepository : IResumenRepository
{
    private readonly AppDbContext _contexto;

    public ResumenRepository(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<ResumenMovimientos> TotalesAsync(int usuarioId, DateTime desde, DateTime hastaExclusivo)
    {
        var consulta = _contexto.Transacciones
            .AsNoTracking()
            .Where(t => t.Cuenta.UsuarioId == usuarioId && t.Fecha >= desde && t.Fecha < hastaExclusivo);

        var ingresos = await consulta.Where(t => t.Tipo == TipoMovimiento.Ingreso).SumAsync(t => (decimal?)t.Monto) ?? 0m;
        var gastos = await consulta.Where(t => t.Tipo == TipoMovimiento.Gasto).SumAsync(t => (decimal?)t.Monto) ?? 0m;

        return new ResumenMovimientos { TotalIngresos = ingresos, TotalGastos = gastos };
    }

    public async Task<IReadOnlyList<GastoCategoria>> GastosPorCategoriaAsync(int usuarioId, DateTime desde, DateTime hastaExclusivo)
    {
        var filas = await _contexto.Transacciones
            .AsNoTracking()
            .Where(t => t.Cuenta.UsuarioId == usuarioId
                        && t.Tipo == TipoMovimiento.Gasto
                        && t.Fecha >= desde && t.Fecha < hastaExclusivo)
            .GroupBy(t => new { t.CategoriaId, t.Categoria.Nombre, t.Categoria.Icono })
            .Select(g => new GastoCategoria
            {
                CategoriaId = g.Key.CategoriaId,
                Nombre = g.Key.Nombre,
                Icono = g.Key.Icono,
                Total = g.Sum(t => t.Monto)
            })
            .ToListAsync();

        // Se ordena en memoria: son pocas filas (una por categoría).
        return filas.OrderByDescending(f => f.Total).ToList();
    }

    public async Task<decimal> GastoDeCategoriaAsync(int usuarioId, int categoriaId, DateTime desde, DateTime hastaExclusivo) =>
        await _contexto.Transacciones
            .AsNoTracking()
            .Where(t => t.Cuenta.UsuarioId == usuarioId
                        && t.CategoriaId == categoriaId
                        && t.Tipo == TipoMovimiento.Gasto
                        && t.Fecha >= desde && t.Fecha < hastaExclusivo)
            .SumAsync(t => (decimal?)t.Monto) ?? 0m;

    public async Task<decimal> SaldoTotalAsync(int usuarioId) =>
        await _contexto.Cuentas
            .AsNoTracking()
            .Where(c => c.UsuarioId == usuarioId)
            .SumAsync(c => (decimal?)c.SaldoActual) ?? 0m;
}
