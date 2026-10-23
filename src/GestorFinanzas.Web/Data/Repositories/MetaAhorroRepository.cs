using GestorFinanzas.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanzas.Web.Data.Repositories;

public class MetaAhorroRepository : IMetaAhorroRepository
{
    private readonly AppDbContext _contexto;

    public MetaAhorroRepository(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<IReadOnlyList<MetaAhorro>> ListarAsync(int usuarioId) =>
        await _contexto.MetasAhorro.AsNoTracking()
            .Where(m => m.UsuarioId == usuarioId)
            .OrderByDescending(m => m.Id)
            .ToListAsync();

    public Task<MetaAhorro?> ObtenerAsync(int id, int usuarioId) =>
        _contexto.MetasAhorro.FirstOrDefaultAsync(m => m.Id == id && m.UsuarioId == usuarioId);

    public async Task AgregarAsync(MetaAhorro meta) => await _contexto.MetasAhorro.AddAsync(meta);

    public void Eliminar(MetaAhorro meta) => _contexto.MetasAhorro.Remove(meta);
}
