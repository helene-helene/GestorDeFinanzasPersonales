namespace GestorFinanzas.Web.Models.Entities;

/// <summary>Alerta dirigida al usuario (límite de presupuesto, meta alcanzada, etc.).</summary>
public class Notificacion
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public DateTime FechaEnvio { get; set; }
    public bool Leida { get; set; }

    public Usuario Usuario { get; set; } = null!;
}
