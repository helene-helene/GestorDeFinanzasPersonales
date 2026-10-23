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

    public async Task<IReadOnlyList<Presupuesto>> ListarVigentesAsync(int usuarioId, DateTime fecha)
    {
        var dia = fecha.Date;
        var presupuestos = await _contexto.Presupuestos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Where(p => p.UsuarioId == usuarioId && p.FechaInicio <= dia && p.FechaFin >= dia)
            .ToListAsync();

        return presupuestos.OrderBy(p => p.Categoria.Nombre).ToList();
    }

    public Task<Presupuesto?> ObtenerAsync(int id, int usuarioId) =>
        _contexto.Presupuestos.Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id && p.UsuarioId == usuarioId);

    public Task<bool> ExisteAsync(int usuarioId, int categoriaId, DateTime fechaInicio)
    {
        var inicio = fechaInicio.Date;
        return _contexto.Presupuestos.AnyAsync(p =>
            p.UsuarioId == usuarioId && p.CategoriaId == categoriaId && p.FechaInicio == inicio);
    }

    public async Task AgregarAsync(Presupuesto presupuesto) => await _contexto.Presupuestos.AddAsync(presupuesto);

    public void Eliminar(Presupuesto presupuesto) => _contexto.Presupuestos.Remove(presupuesto);
}
