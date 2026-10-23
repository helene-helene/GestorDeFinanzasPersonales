namespace GestorFinanzas.Web.Models.Entities;

// Recomendación de ahorro o de control de gastos generada por el módulo de IA.
public class RecomendacionIA
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string TipoRecomendacion { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaGeneracion { get; set; }
    public bool Aplicada { get; set; }

    public Usuario Usuario { get; set; } = null!;
}
