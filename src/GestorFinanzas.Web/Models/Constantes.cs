namespace GestorFinanzas.Web.Models;

/// <summary>Valores válidos para el tipo de una transacción o de una categoría.</summary>
public static class TipoMovimiento
{
    public const string Ingreso = "Ingreso";
    public const string Gasto = "Gasto";

    public static readonly string[] Todos = { Ingreso, Gasto };
}

/// <summary>Tipos de cuenta permitidos (coinciden con la restricción CHECK de la tabla Cuentas).</summary>
public static class TipoCuenta
{
    public const string Ahorro = "Ahorro";
    public const string Corriente = "Corriente";
    public const string Efectivo = "Efectivo";
    public const string Tarjeta = "Tarjeta";

    public static readonly string[] Todos = { Ahorro, Corriente, Efectivo, Tarjeta };
}

/// <summary>Periodicidades que puede tener un presupuesto.</summary>
public static class PeriodoPresupuesto
{
    public const string Mensual = "Mensual";
    public const string Quincenal = "Quincenal";
    public const string Semanal = "Semanal";
}
