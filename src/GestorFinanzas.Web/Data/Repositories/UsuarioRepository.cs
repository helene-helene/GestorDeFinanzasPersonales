using GestorFinanzas.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanzas.Web.Data.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _contexto;

    public UsuarioRepository(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken cancellationToken = default) =>
        _contexto.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo, cancellationToken);

    public Task<Usuario?> ObtenerPorIdAsync(int id, bool conSeguimiento = false, CancellationToken cancellationToken = default)
    {
        IQueryable<Usuario> consulta = _contexto.Usuarios.Where(u => u.Id == id);

        if (!conSeguimiento)
        {
            consulta = consulta.AsNoTracking();
        }

        return consulta.FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> ExisteCorreoAsync(string correo, int? excluirUsuarioId = null, CancellationToken cancellationToken = default) =>
        _contexto.Usuarios.AnyAsync(
            u => u.Correo == correo && (excluirUsuarioId == null || u.Id != excluirUsuarioId),
            cancellationToken);

    public async Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default) =>
        await _contexto.Usuarios.AddAsync(usuario, cancellationToken);
}
