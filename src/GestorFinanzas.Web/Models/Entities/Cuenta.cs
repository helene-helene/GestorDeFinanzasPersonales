namespace GestorFinanzas.Web.Models.Entities;

// Cuenta financiera del usuario (efectivo, ahorro, tarjeta...).
public class Cuenta
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string NombreCuenta { get; set; } = string.Empty;

    // Ahorro, Corriente, Efectivo o Tarjeta (ver TipoCuenta).
    public string TipoCuenta { get; set; } = Models.TipoCuenta.Efectivo;

    public decimal SaldoActual { get; set; }
    public string Moneda { get; set; } = "USD";

    public Usuario Usuario { get; set; } = null!;
    public ICollection<Transaccion> Transacciones { get; set; } = new List<Transaccion>();
}
