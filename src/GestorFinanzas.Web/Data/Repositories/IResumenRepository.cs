using GestorFinanzas.Web.Models.Filtros;
using GestorFinanzas.Web.Models.Resumenes;

namespace GestorFinanzas.Web.Data.Repositories;

// Consultas de totales que usan el Inicio, los presupuestos y la IA. Todas son de solo lectura.
public interface IResumenRepository
{
    // Suma de ingresos y gastos entre dos fechas (desde incluida, hasta excluida).
    Task<ResumenMovimientos> TotalesAsync(int usuarioId, DateTime desde, DateTime hastaExclusivo);

    Task<IReadOnlyList<GastoCategoria>> GastosPorCategoriaAsync(int usuarioId, DateTime desde, DateTime hastaExclusivo);

    Task<decimal> GastoDeCategoriaAsync(int usuarioId, int categoriaId, DateTime desde, DateTime hastaExclusivo);

    Task<decimal> SaldoTotalAsync(int usuarioId);
}
