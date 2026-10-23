using GestorFinanzas.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanzas.Web.Data.Repositories;

public class CuentaRepository : ICuentaRepository
{
    private readonly AppDbContext _contexto;

    public CuentaRepository(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<IReadOnlyList<Cuenta>> ListarPorUsuarioAsync(int usuarioId) =>
        await _contexto.Cuentas
            .AsNoTracking()
            .Where(c => c.UsuarioId == usuarioId)
            .OrderBy(c => c.NombreCuenta)
            .ToListAsync();

    public Task<Cuenta?> ObtenerAsync(int id, int usuarioId) =>
        _contexto.Cuentas.FirstOrDefaultAsync(c => c.Id == id && c.UsuarioId == usuarioId);
}
