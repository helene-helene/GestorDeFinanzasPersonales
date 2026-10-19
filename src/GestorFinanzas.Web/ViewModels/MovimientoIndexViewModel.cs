using GestorFinanzas.Web.Models.Entities;
using GestorFinanzas.Web.Models.Filtros;

namespace GestorFinanzas.Web.ViewModels;

/// <summary>Datos de la pantalla de historial de movimientos.</summary>
public class MovimientoIndexViewModel
{
    public FiltroMovimientos Filtro { get; set; } = new();
    public ResultadoPaginado<Transaccion> Resultado { get; set; } = new();
    public ResumenMovimientos Resumen { get; set; } = new();
    public IReadOnlyList<Cuenta> Cuentas { get; set; } = Array.Empty<Cuenta>();
    public IReadOnlyList<Categoria> Categorias { get; set; } = Array.Empty<Categoria>();
}
