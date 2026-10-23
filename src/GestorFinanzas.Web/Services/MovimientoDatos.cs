namespace GestorFinanzas.Web.Services;

// Datos que se necesitan para registrar o modificar un movimiento.
public class MovimientoDatos
{
    public int CuentaId { get; set; }
    public int CategoriaId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }
    public string? Descripcion { get; set; }
}
