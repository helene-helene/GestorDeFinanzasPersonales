namespace GestorFinanzas.Web.Models.Resumenes;

// Total gastado en una categoría durante un período.
public class GastoCategoria
{
    public int CategoriaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Icono { get; set; }
    public decimal Total { get; set; }
}

// Un presupuesto con lo que el usuario lleva gastado en su categoría y período.
public class LineaPresupuesto
{
    public int Id { get; set; }
    public int CategoriaId { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public string? Icono { get; set; }
    public string Periodo { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal Limite { get; set; }
    public decimal Gastado { get; set; }

    public decimal Restante => Limite - Gastado;
    public bool Superado => Gastado > Limite;
    public decimal Porcentaje => Limite <= 0 ? 0m : Math.Round(Gastado / Limite * 100m, 0);
}

// Presupuestos vigentes de un usuario con sus totales.
public class ResumenPresupuesto
{
    public IReadOnlyList<LineaPresupuesto> Lineas { get; init; } = Array.Empty<LineaPresupuesto>();

    public decimal TotalLimite => Lineas.Sum(l => l.Limite);
    public decimal TotalGastado => Lineas.Sum(l => l.Gastado);
    public decimal Disponible => TotalLimite - TotalGastado;
    public decimal Porcentaje => TotalLimite <= 0 ? 0m : Math.Round(TotalGastado / TotalLimite * 100m, 0);
}

// Una meta de ahorro con su avance calculado.
public class AvanceMeta
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Objetivo { get; set; }
    public decimal Ahorrado { get; set; }
    public DateTime? FechaLimite { get; set; }

    public decimal Faltante => Math.Max(0m, Objetivo - Ahorrado);
    public bool Completa => Ahorrado >= Objetivo;
    public decimal Porcentaje => Objetivo <= 0 ? 0m : Math.Min(100m, Math.Round(Ahorrado / Objetivo * 100m, 0));
}

// Datos que muestra la pantalla de Inicio.
public class ResumenInicio
{
    public string Nombre { get; set; } = string.Empty;
    public decimal SaldoTotal { get; set; }
    public decimal IngresosMes { get; set; }
    public decimal GastosMes { get; set; }
    public decimal PresupuestoMes { get; set; }
    public int MetasActivas { get; set; }
    public IReadOnlyList<GastoCategoria> Categorias { get; set; } = Array.Empty<GastoCategoria>();
    public string Consejo { get; set; } = string.Empty;

    public decimal Disponible => PresupuestoMes > 0 ? PresupuestoMes - GastosMes : SaldoTotal;
    public decimal PorcentajeUsado =>
        PresupuestoMes <= 0 ? 0m : Math.Min(100m, Math.Round(GastosMes / PresupuestoMes * 100m, 0));
}
