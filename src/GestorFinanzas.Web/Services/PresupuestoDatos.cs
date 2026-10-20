using GestorFinanzas.Web.Models;

namespace GestorFinanzas.Web.Services;

/// <summary>Datos que llegan del formulario. La fecha de fin la calcula el servicio.</summary>
public class PresupuestoDatos
{
    public int CategoriaId { get; set; }
    public decimal MontoLimite { get; set; }
    public string Periodo { get; set; } = PeriodoPresupuesto.Mensual;
    public DateTime FechaInicio { get; set; }
}
