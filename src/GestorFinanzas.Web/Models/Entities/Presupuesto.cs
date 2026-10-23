namespace GestorFinanzas.Web.Models.Entities;

// Límite de gasto que el usuario define para una categoría durante un período.
public class Presupuesto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int CategoriaId { get; set; }
    public decimal MontoLimite { get; set; }
    public string Periodo { get; set; } = PeriodoPresupuesto.Mensual;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public Categoria Categoria { get; set; } = null!;
}
