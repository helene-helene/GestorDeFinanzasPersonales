namespace GestorFinanzas.Web.Models.Filtros;

// Totales de los movimientos que cumplen un filtro.
public class ResumenMovimientos
{
    public decimal TotalIngresos { get; init; }
    public decimal TotalGastos { get; init; }
    public decimal Balance => TotalIngresos - TotalGastos;
}
