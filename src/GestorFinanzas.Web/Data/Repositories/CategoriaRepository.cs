using GestorFinanzas.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanzas.Web.Data.Repositories;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly AppDbContext _contexto;

    public CategoriaRepository(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<IReadOnlyList<Categoria>> ListarAsync(string? tipo = null)
    {
        var consulta = _contexto.Categorias.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(tipo))
        {
            consulta = consulta.Where(c => c.Tipo == tipo);
        }

        return await consulta.OrderBy(c => c.Tipo).ThenBy(c => c.Nombre).ToListAsync();
    }

    public Task<Categoria?> ObtenerAsync(int id) =>
        _contexto.Categorias.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
}
