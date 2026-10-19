namespace GestorFinanzas.Web.Models.Filtros;

/// <summary>Totales de los movimientos que cumplen un filtro.</summary>
public class ResumenMovimientos
{
    public decimal TotalIngresos { get; init; }
    public decimal TotalGastos { get; init; }
    public decimal Balance => TotalIngresos - TotalGastos;
}
