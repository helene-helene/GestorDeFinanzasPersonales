using GestorFinanzas.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanzas.Web.Data.Repositories;

public class RecomendacionRepository : IRecomendacionRepository
{
    private readonly AppDbContext _contexto;

    public RecomendacionRepository(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<IReadOnlyList<RecomendacionIA>> ListarAsync(int usuarioId, int maximo) =>
        await _contexto.RecomendacionesIA.AsNoTracking()
            .Where(r => r.UsuarioId == usuarioId)
            .OrderByDescending(r => r.FechaGeneracion).ThenByDescending(r => r.Id)
            .Take(maximo)
            .ToListAsync();

    public Task<RecomendacionIA?> ObtenerAsync(int id, int usuarioId) =>
        _contexto.RecomendacionesIA.FirstOrDefaultAsync(r => r.Id == id && r.UsuarioId == usuarioId);

    public Task<bool> ExisteAsync(int usuarioId, string tipo, string prefijoDescripcion, DateTime desde) =>
        _contexto.RecomendacionesIA.AnyAsync(r =>
            r.UsuarioId == usuarioId && r.TipoRecomendacion == tipo
            && r.FechaGeneracion >= desde && r.Descripcion.StartsWith(prefijoDescripcion));

    public async Task AgregarAsync(RecomendacionIA recomendacion) => await _contexto.RecomendacionesIA.AddAsync(recomendacion);
}
