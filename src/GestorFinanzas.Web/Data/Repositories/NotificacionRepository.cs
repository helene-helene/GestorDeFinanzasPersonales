using GestorFinanzas.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanzas.Web.Data.Repositories;

public class NotificacionRepository : INotificacionRepository
{
    private readonly AppDbContext _contexto;

    public NotificacionRepository(AppDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<IReadOnlyList<Notificacion>> ListarAsync(int usuarioId, int maximo) =>
        await _contexto.Notificaciones.AsNoTracking()
            .Where(n => n.UsuarioId == usuarioId)
            .OrderByDescending(n => n.FechaEnvio).ThenByDescending(n => n.Id)
            .Take(maximo)
            .ToListAsync();

    public Task<int> ContarNoLeidasAsync(int usuarioId) =>
        _contexto.Notificaciones.CountAsync(n => n.UsuarioId == usuarioId && !n.Leida);

    public Task<bool> ExisteAsync(int usuarioId, string tipo, string prefijoMensaje, DateTime desde) =>
        _contexto.Notificaciones.AnyAsync(n =>
            n.UsuarioId == usuarioId && n.Tipo == tipo && n.FechaEnvio >= desde && n.Mensaje.StartsWith(prefijoMensaje));

    public async Task<IReadOnlyList<Notificacion>> ListarNoLeidasConSeguimientoAsync(int usuarioId) =>
        await _contexto.Notificaciones.Where(n => n.UsuarioId == usuarioId && !n.Leida).ToListAsync();

    public async Task AgregarAsync(Notificacion notificacion) => await _contexto.Notificaciones.AddAsync(notificacion);
}
