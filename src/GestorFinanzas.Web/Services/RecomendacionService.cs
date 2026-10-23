using GestorFinanzas.Web.Data.Repositories;
using GestorFinanzas.Web.Infrastructure;
using GestorFinanzas.Web.Models.Entities;
using GestorFinanzas.Web.Services.IA;

namespace GestorFinanzas.Web.Services;

// Guarda en la tabla RecomendacionesIA lo que genera el módulo de IA. Una recomendación con el mismo
// título no se vuelve a guardar durante el mismo mes, aunque sus montos cambien.
public class RecomendacionService : IRecomendacionService
{
    private const int MaximoMostradas = 20;
    private const int LongitudMaximaTipo = 50;
    private const int LongitudMaximaDescripcion = 300;

    private readonly IServicioIA _ia;
    private readonly IRecomendacionRepository _recomendaciones;
    private readonly IUnitOfWork _unidadDeTrabajo;
    private readonly ICurrentUserService _usuarioActual;

    public RecomendacionService(
        IServicioIA ia,
        IRecomendacionRepository recomendaciones,
        IUnitOfWork unidadDeTrabajo,
        ICurrentUserService usuarioActual)
    {
        _ia = ia;
        _recomendaciones = recomendaciones;
        _unidadDeTrabajo = unidadDeTrabajo;
        _usuarioActual = usuarioActual;
    }

    public async Task<IReadOnlyList<RecomendacionIA>> GenerarYListarAsync(CancellationToken cancellationToken = default)
    {
        var usuarioId = _usuarioActual.UsuarioId;
        var hoy = DateTime.Today;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

        var generadas = await _ia.GenerarRecomendacionesAsync(cancellationToken);
        var hayNuevas = false;

        foreach (var g in generadas)
        {
            var tipo = Formato.Recortar(g.Tipo, LongitudMaximaTipo);
            var titulo = $"{g.Titulo}.";

            if (await _recomendaciones.ExisteAsync(usuarioId, tipo, titulo, inicioMes)) continue;

            await _recomendaciones.AgregarAsync(new RecomendacionIA
            {
                UsuarioId = usuarioId,
                TipoRecomendacion = tipo,
                Descripcion = Formato.Recortar($"{titulo} {g.Mensaje}", LongitudMaximaDescripcion),
                FechaGeneracion = DateTime.Now,
                Aplicada = false
            });
            hayNuevas = true;
        }

        if (hayNuevas) await _unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);

        return await _recomendaciones.ListarAsync(usuarioId, MaximoMostradas);
    }

    public async Task MarcarAplicadaAsync(int id)
    {
        var recomendacion = await _recomendaciones.ObtenerAsync(id, _usuarioActual.UsuarioId)
            ?? throw new ReglaNegocioException("La recomendación no existe.");

        recomendacion.Aplicada = true;
        await _unidadDeTrabajo.GuardarCambiosAsync();
    }
}
