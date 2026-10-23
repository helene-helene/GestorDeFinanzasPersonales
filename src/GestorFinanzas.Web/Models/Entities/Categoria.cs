namespace GestorFinanzas.Web.Models.Entities;

// Categoría de ingreso o de gasto. Las categorías iniciales se cargan con la migración.
public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    // Ingreso o Gasto (ver TipoMovimiento).
    public string Tipo { get; set; } = TipoMovimiento.Gasto;

    // Nombre del ícono de Bootstrap Icons (por ejemplo "bi-bus-front").
    public string? Icono { get; set; }

    public ICollection<Transaccion> Transacciones { get; set; } = new List<Transaccion>();
    public ICollection<Presupuesto> Presupuestos { get; set; } = new List<Presupuesto>();
}
