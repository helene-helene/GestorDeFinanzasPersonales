namespace GestorFinanzas.Web.Models;

// Valores válidos para el tipo de una transacción o de una categoría.
public static class TipoMovimiento
{
    public const string Ingreso = "Ingreso";
    public const string Gasto = "Gasto";

    public static readonly string[] Todos = { Ingreso, Gasto };
}

// Tipos de cuenta permitidos (coinciden con la restricción CHECK de la tabla Cuentas).
public static class TipoCuenta
{
    public const string Ahorro = "Ahorro";
    public const string Corriente = "Corriente";
    public const string Efectivo = "Efectivo";
    public const string Tarjeta = "Tarjeta";

    public static readonly string[] Todos = { Ahorro, Corriente, Efectivo, Tarjeta };
}

// Periodicidades que puede tener un presupuesto.
public static class PeriodoPresupuesto
{
    public const string Mensual = "Mensual";
    public const string Quincenal = "Quincenal";
    public const string Semanal = "Semanal";
}
