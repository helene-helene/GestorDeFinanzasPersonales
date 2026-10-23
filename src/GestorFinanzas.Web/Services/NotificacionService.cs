using GestorFinanzas.Web.Data.Repositories;
using GestorFinanzas.Web.Infrastructure;
using GestorFinanzas.Web.Models.Entities;

namespace GestorFinanzas.Web.Services;

// Alertas de presupuesto y metas, generadas a partir de los datos del usuario:
//  - Presupuesto: aviso al llegar al 80% del límite y otro al superarlo (una vez por período).
//  - Meta: aviso al llegar al 80% del objetivo y otro al completarlo (una sola vez).
// Cada alerta se identifica por el inicio de su mensaje, así no se repite aunque los montos cambien.
public class NotificacionService : INotificacionService
{
    private const int MaximoMostradas = 50;
    private const int LongitudMaximaMensaje = 250;
    private static readonly DateTime DesdeSiempre = new(2000, 1, 1);

    private readonly INotificacionRepository _notificaciones;
    private readonly IPresupuestoService _presupuestos;
    private readonly IMetaAhorroRepository _metas;
    private readonly IUnitOfWork _unidadDeTrabajo;
    private readonly ICurrentUserService _usuarioActual;

    public NotificacionService(
        INotificacionRepository notificaciones,
        IPresupuestoService presupuestos,
        IMetaAhorroRepository metas,
        IUnitOfWork unidadDeTrabajo,
        ICurrentUserService usuarioActual)
    {
        _notificaciones = notificaciones;
        _presupuestos = presupuestos;
        _metas = metas;
        _unidadDeTrabajo = unidadDeTrabajo;
        _usuarioActual = usuarioActual;
    }

    public async Task RevisarAlertasAsync()
    {
        var usuarioId = _usuarioActual.UsuarioId;
        var hayNuevas = false;

        var resumen = await _presupuestos.ObtenerResumenAsync();
        foreach (var linea in resumen.Lineas.Where(l => l.Limite > 0))
        {
            var periodo = linea.Periodo.ToLowerInvariant();
            string? prefijo = null;
            if (linea.Gastado >= linea.Limite)
                prefijo = $"Superaste tu presupuesto de {linea.Categoria} ({periodo}).";
            else if (linea.Porcentaje >= 80)
                prefijo = $"Ya usaste el 80% de tu presupuesto de {linea.Categoria} ({periodo}).";

            if (prefijo is null) continue;
            if (await _notificaciones.ExisteAsync(usuarioId, "Presupuesto", prefijo, linea.FechaInicio.Date)) continue;

            await AgregarAsync(usuarioId, "Presupuesto", prefijo,
                $"{prefijo} Llevas {Formato.Dinero(linea.Gastado)} de {Formato.Dinero(linea.Limite)}.");
            hayNuevas = true;
        }

        var metas = await _metas.ListarAsync(usuarioId);
        foreach (var meta in metas.Where(m => m.MontoObjetivo > 0))
        {
            string? prefijo = null;
            var detalle = string.Empty;
            if (meta.MontoActual >= meta.MontoObjetivo)
            {
                prefijo = $"¡Alcanzaste tu meta {meta.NombreMeta}!";
                detalle = $" Ahorraste {Formato.Dinero(meta.MontoActual)}.";
            }
            else if (meta.MontoActual / meta.MontoObjetivo >= 0.8m)
            {
                prefijo = $"Estás cerca de tu meta {meta.NombreMeta}.";
                detalle = $" Llevas {Formato.Dinero(meta.MontoActual)} de {Formato.Dinero(meta.MontoObjetivo)}.";
            }

            if (prefijo is null) continue;
            if (await _notificaciones.ExisteAsync(usuarioId, "Meta", prefijo, DesdeSiempre)) continue;

            await AgregarAsync(usuarioId, "Meta", prefijo, prefijo + detalle);
            hayNuevas = true;
        }

        if (hayNuevas) await _unidadDeTrabajo.GuardarCambiosAsync();
    }

    public Task<IReadOnlyList<Notificacion>> ListarAsync() =>
        _notificaciones.ListarAsync(_usuarioActual.UsuarioId, MaximoMostradas);

    public Task<int> ContarNoLeidasAsync() =>
        _notificaciones.ContarNoLeidasAsync(_usuarioActual.UsuarioId);

    public async Task MarcarTodasComoLeidasAsync()
    {
        var pendientes = await _notificaciones.ListarNoLeidasConSeguimientoAsync(_usuarioActual.UsuarioId);
        if (pendientes.Count == 0) return;

        foreach (var n in pendientes) n.Leida = true;
        await _unidadDeTrabajo.GuardarCambiosAsync();
    }

    private Task AgregarAsync(int usuarioId, string tipo, string prefijo, string mensaje) =>
        _notificaciones.AgregarAsync(new Notificacion
        {
            UsuarioId = usuarioId,
            Tipo = tipo,
            Mensaje = Formato.Recortar(mensaje.StartsWith(prefijo) ? mensaje : prefijo, LongitudMaximaMensaje),
            FechaEnvio = DateTime.Now,
            Leida = false
        });
}
