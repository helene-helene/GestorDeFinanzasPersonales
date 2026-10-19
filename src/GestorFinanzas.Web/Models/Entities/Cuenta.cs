namespace GestorFinanzas.Web.Models.Entities;

/// <summary>Cuenta financiera del usuario (efectivo, ahorro, tarjeta...).</summary>
public class Cuenta
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string NombreCuenta { get; set; } = string.Empty;

    /// <summary>Ahorro, Corriente, Efectivo o Tarjeta (ver <see cref="TipoCuenta"/>).</summary>
    public string TipoCuenta { get; set; } = Models.TipoCuenta.Efectivo;

    public decimal SaldoActual { get; set; }
    public string Moneda { get; set; } = "USD";

    public Usuario Usuario { get; set; } = null!;
    public ICollection<Transaccion> Transacciones { get; set; } = new List<Transaccion>();
}
