namespace GestorFinanzas.Web.Models.Entities;

/// <summary>Categoría de ingreso o de gasto. Las categorías iniciales se cargan con la migración.</summary>
public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Ingreso o Gasto (ver <see cref="TipoMovimiento"/>).</summary>
    public string Tipo { get; set; } = TipoMovimiento.Gasto;

    /// <summary>Nombre del ícono de Bootstrap Icons (por ejemplo "bi-bus-front").</summary>
    public string? Icono { get; set; }

    public ICollection<Transaccion> Transacciones { get; set; } = new List<Transaccion>();
    public ICollection<Presupuesto> Presupuestos { get; set; } = new List<Presupuesto>();
}
