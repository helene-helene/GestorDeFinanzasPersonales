using GestorFinanzas.Web.Models;
using GestorFinanzas.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanzas.Web.Data.Repositories;

public class PresupuestoRepository : IPresupuestoRepository
{
    private readonly AppDbContext _contexto;

    public PresupuestoRepository(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<IReadOnlyList<Presupuesto>> ListarAsync(int usuarioId)
    {
        return await _contexto.Presupuestos
            .Include(p => p.Categoria)
            .Where(p => p.UsuarioId == usuarioId)
            .OrderByDescending(p => p.FechaInicio)
            .ThenBy(p => p.Categoria.Nombre)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Presupuesto?> ObtenerAsync(int id, int usuarioId, bool conSeguimiento = false)
    {
        IQueryable<Presupuesto> consulta = _contexto.Presupuestos
            .Include(p => p.Categoria)
            .Where(p => p.Id == id && p.UsuarioId == usuarioId);

        if (!conSeguimiento)
        {
            consulta = consulta.AsNoTracking();
        }

        return await consulta.FirstOrDefaultAsync();
    }

    public Task<bool> HaySolapamientoAsync(int usuarioId, int categoriaId, DateTime inicio, DateTime fin, int? excluirId = null)
    {
        IQueryable<Presupuesto> consulta = _contexto.Presupuestos
            .Where(p => p.UsuarioId == usuarioId
                        && p.CategoriaId == categoriaId
                        && p.FechaInicio <= fin
                        && p.FechaFin >= inicio);

        if (excluirId.HasValue)
        {
            var id = excluirId.Value;
            consulta = consulta.Where(p => p.Id != id);
        }

        return consulta.AnyAsync();
    }

    public async Task<decimal> TotalGastadoAsync(int usuarioId, int categoriaId, DateTime desde, DateTime hasta)
    {
        var desdeFecha = desde.Date;
        var hastaExclusivo = hasta.Date.AddDays(1);

        return await _contexto.Transacciones
            .Where(t => t.Cuenta.UsuarioId == usuarioId
                        && t.CategoriaId == categoriaId
                        && t.Tipo == TipoMovimiento.Gasto
                        && t.Fecha >= desdeFecha
                        && t.Fecha < hastaExclusivo)
            .SumAsync(t => (decimal?)t.Monto) ?? 0m;
    }

    public async Task AgregarAsync(Presupuesto presupuesto) =>
        await _contexto.Presupuestos.AddAsync(presupuesto);

    public void Eliminar(Presupuesto presupuesto) =>
        _contexto.Presupuestos.Remove(presupuesto);
}
