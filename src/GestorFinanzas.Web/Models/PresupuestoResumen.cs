using GestorFinanzas.Web.Models.Entities;

namespace GestorFinanzas.Web.Models;

public static class EstadoPresupuesto
{
    public const string Normal = "Normal";
    public const string Alerta = "Alerta";
    public const string Excedido = "Excedido";
}

/// <summary>Un presupuesto junto con lo que se lleva gastado en su categoría dentro del período.</summary>
public class PresupuestoResumen
{
    public Presupuesto Presupuesto { get; set; } = null!;
    public decimal Gastado { get; set; }
    public decimal Porcentaje { get; set; }
    public string Estado { get; set; } = EstadoPresupuesto.Normal;

    // true si hoy cae dentro de las fechas del presupuesto
    public bool Vigente { get; set; }

    public decimal Disponible => Presupuesto.MontoLimite - Gastado;
}
