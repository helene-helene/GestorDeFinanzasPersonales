using GestorFinanzas.Web.Data.Repositories;
using GestorFinanzas.Web.Infrastructure;
using GestorFinanzas.Web.Models.Entities;
using GestorFinanzas.Web.Models.Resumenes;

namespace GestorFinanzas.Web.Services;

// Reglas de negocio de las metas de ahorro:
//  1. El nombre es obligatorio (máximo 100 caracteres).
//  2. El objetivo es mayor que 0; lo ahorrado no es negativo ni pasa del objetivo (máximo 2 decimales).
//  3. La fecha límite, si se indica, no puede estar en el pasado.
//  4. Cada aporte suma a lo ahorrado sin pasar del objetivo; una meta completa ya no recibe aportes.
public class MetaAhorroService : IMetaAhorroService
{
    private const decimal MontoMaximo = 9_999_999_999.99m;
    private const int LongitudMaximaNombre = 100;

    private readonly IMetaAhorroRepository _metas;
    private readonly IUnitOfWork _unidadDeTrabajo;
    private readonly ICurrentUserService _usuarioActual;
    private readonly INotificacionService _notificaciones;
    private readonly ILogger<MetaAhorroService> _logger;

    public MetaAhorroService(
        IMetaAhorroRepository metas,
        IUnitOfWork unidadDeTrabajo,
        ICurrentUserService usuarioActual,
        INotificacionService notificaciones,
        ILogger<MetaAhorroService> logger)
    {
        _metas = metas;
        _unidadDeTrabajo = unidadDeTrabajo;
        _usuarioActual = usuarioActual;
        _notificaciones = notificaciones;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AvanceMeta>> ListarAsync()
    {
        var metas = await _metas.ListarAsync(_usuarioActual.UsuarioId);
        return metas.Select(ADto).ToList();
    }

    public async Task<AvanceMeta> CrearAsync(MetaDatos datos)
    {
        var nombre = (datos.Nombre ?? string.Empty).Trim();
        if (nombre.Length == 0) throw new ReglaNegocioException("Escribe el nombre de la meta.");
        if (nombre.Length > LongitudMaximaNombre)
            throw new ReglaNegocioException($"El nombre no puede pasar de {LongitudMaximaNombre} caracteres.");
        if (datos.MontoObjetivo <= 0) throw new ReglaNegocioException("El monto objetivo debe ser mayor que 0.");
        ValidarMonto(datos.MontoObjetivo, "El monto objetivo");
        if (datos.MontoActual < 0) throw new ReglaNegocioException("Lo ahorrado hasta hoy no puede ser negativo.");
        ValidarMonto(datos.MontoActual, "Lo ahorrado");
        if (datos.MontoActual > datos.MontoObjetivo)
            throw new ReglaNegocioException("Lo ahorrado hasta hoy no puede ser mayor que el monto objetivo.");
        if (datos.FechaLimite.HasValue && datos.FechaLimite.Value.Date < DateTime.Today)
            throw new ReglaNegocioException("La fecha límite no puede estar en el pasado.");

        var meta = new MetaAhorro
        {
            UsuarioId = _usuarioActual.UsuarioId,
            NombreMeta = nombre,
            MontoObjetivo = datos.MontoObjetivo,
            MontoActual = datos.MontoActual,
            FechaLimite = datos.FechaLimite?.Date
        };

        await _metas.AgregarAsync(meta);
        await _unidadDeTrabajo.GuardarCambiosAsync();
        await RevisarAlertasSinFallarAsync();
        return ADto(meta);
    }

    public async Task<AvanceMeta> AportarAsync(int id, decimal monto)
    {
        if (monto <= 0) throw new ReglaNegocioException("El aporte debe ser mayor que 0.");
        ValidarMonto(monto, "El aporte");

        var meta = await _metas.ObtenerAsync(id, _usuarioActual.UsuarioId)
            ?? throw new ReglaNegocioException("No se encontró esa meta.");

        if (meta.MontoActual >= meta.MontoObjetivo)
            throw new ReglaNegocioException("Esa meta ya está completa. ¡Felicidades!");

        meta.MontoActual = Math.Min(meta.MontoObjetivo, meta.MontoActual + monto);
        await _unidadDeTrabajo.GuardarCambiosAsync();
        await RevisarAlertasSinFallarAsync();
        return ADto(meta);
    }

    public async Task EliminarAsync(int id)
    {
        var meta = await _metas.ObtenerAsync(id, _usuarioActual.UsuarioId)
            ?? throw new ReglaNegocioException("La meta que intentas eliminar no existe.");

        _metas.Eliminar(meta);
        await _unidadDeTrabajo.GuardarCambiosAsync();
    }

    private static void ValidarMonto(decimal monto, string campo)
    {
        if (monto > MontoMaximo) throw new ReglaNegocioException($"{campo} es demasiado grande.");
        if (decimal.Round(monto, 2) != monto) throw new ReglaNegocioException($"{campo} admite como máximo 2 decimales.");
    }

    private static AvanceMeta ADto(MetaAhorro meta) => new()
    {
        Id = meta.Id,
        Nombre = meta.NombreMeta,
        Objetivo = meta.MontoObjetivo,
        Ahorrado = meta.MontoActual,
        FechaLimite = meta.FechaLimite
    };

    // Revisar las alertas nunca debe impedir que la meta se guarde.
    private async Task RevisarAlertasSinFallarAsync()
    {
        try
        {
            await _notificaciones.RevisarAlertasAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudieron revisar las alertas después de guardar la meta.");
        }
    }
}
