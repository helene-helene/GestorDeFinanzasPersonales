namespace GestorFinanzas.Web.Models.Entities;

/// <summary>Movimiento de dinero: un ingreso o un gasto registrado en una cuenta.</summary>
public class Transaccion
{
    public int Id { get; set; }
    public int CuentaId { get; set; }
    public int CategoriaId { get; set; }
    public decimal Monto { get; set; }

    /// <summary>Ingreso o Gasto. Debe coincidir con el tipo de la categoría.</summary>
    public string Tipo { get; set; } = TipoMovimiento.Gasto;

    public DateTime Fecha { get; set; }
    public string? Descripcion { get; set; }

    public Cuenta Cuenta { get; set; } = null!;
    public Categoria Categoria { get; set; } = null!;
}
