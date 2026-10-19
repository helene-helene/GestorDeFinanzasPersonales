namespace GestorFinanzas.Web.Models.Entities;

/// <summary>Perfil del estudiante que usa el sistema.</summary>
public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;

    /// <summary>Hash de la contraseña. Nunca se guarda la contraseña en texto plano.</summary>
    public string Contrasenia { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; }

    public ICollection<Cuenta> Cuentas { get; set; } = new List<Cuenta>();
    public ICollection<Presupuesto> Presupuestos { get; set; } = new List<Presupuesto>();
    public ICollection<MetaAhorro> MetasAhorro { get; set; } = new List<MetaAhorro>();
    public ICollection<RecomendacionIA> Recomendaciones { get; set; } = new List<RecomendacionIA>();
    public ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();
}
