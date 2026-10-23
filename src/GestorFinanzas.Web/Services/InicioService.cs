using GestorFinanzas.Web.Data.Repositories;
using GestorFinanzas.Web.Models.Resumenes;
using GestorFinanzas.Web.Services.IA;

namespace GestorFinanzas.Web.Services;

// Reúne los datos del mes actual: saldo, ingresos, gastos, presupuesto, metas y un consejo de la IA.
public class InicioService : IInicioService
{
    private const string ConsejoPorDefecto = "Registra tus ingresos y gastos para ver un resumen de tu dinero.";

    private readonly IResumenRepository _resumen;
    private readonly IPresupuestoService _presupuestos;
    private readonly IMetaAhorroService _metas;
    private readonly IServicioIA _ia;
    private readonly ICurrentUserService _usuarioActual;

    public InicioService(
        IResumenRepository resumen,
        IPresupuestoService presupuestos,
        IMetaAhorroService metas,
        IServicioIA ia,
        ICurrentUserService usuarioActual)
    {
        _resumen = resumen;
        _presupuestos = presupuestos;
        _metas = metas;
        _ia = ia;
        _usuarioActual = usuarioActual;
    }

    public async Task<ResumenInicio> ObtenerResumenAsync(string nombreUsuario, CancellationToken cancellationToken = default)
    {
        var usuarioId = _usuarioActual.UsuarioId;
        var hoy = DateTime.Today;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
        var finMes = inicioMes.AddMonths(1);

        var totales = await _resumen.TotalesAsync(usuarioId, inicioMes, finMes);
        var categorias = await _resumen.GastosPorCategoriaAsync(usuarioId, inicioMes, finMes);
        var saldo = await _resumen.SaldoTotalAsync(usuarioId);
        var presupuestos = await _presupuestos.ObtenerResumenAsync();
        var metas = await _metas.ListarAsync();
        var recomendaciones = await _ia.GenerarRecomendacionesAsync(cancellationToken);

        var consejo = recomendaciones.FirstOrDefault(r => r.Tipo != "Bienestar")?.Mensaje ?? ConsejoPorDefecto;

        return new ResumenInicio
        {
            Nombre = nombreUsuario,
            SaldoTotal = saldo,
            IngresosMes = totales.TotalIngresos,
            GastosMes = totales.TotalGastos,
            PresupuestoMes = presupuestos.TotalLimite,
            MetasActivas = metas.Count(m => !m.Completa),
            Categorias = categorias,
            Consejo = consejo
        };
    }
}
