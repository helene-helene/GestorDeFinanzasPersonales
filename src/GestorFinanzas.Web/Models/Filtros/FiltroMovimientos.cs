namespace GestorFinanzas.Web.Models.Filtros;

/// <summary>Criterios opcionales para consultar el historial de movimientos.</summary>
public class FiltroMovimientos
{
    public int? CuentaId { get; set; }
    public string? Tipo { get; set; }
    public int? CategoriaId { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public string? Texto { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanioPagina { get; set; } = 10;
}
