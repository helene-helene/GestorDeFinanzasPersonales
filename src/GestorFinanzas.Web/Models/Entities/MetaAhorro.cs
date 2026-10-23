namespace GestorFinanzas.Web.Models.Entities;

// Meta de ahorro del usuario (por ejemplo un fondo de emergencia).
public class MetaAhorro
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string NombreMeta { get; set; } = string.Empty;
    public decimal MontoObjetivo { get; set; }
    public decimal MontoActual { get; set; }
    public DateTime? FechaLimite { get; set; }

    public Usuario Usuario { get; set; } = null!;
}
