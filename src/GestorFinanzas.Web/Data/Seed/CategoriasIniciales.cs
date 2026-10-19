using GestorFinanzas.Web.Models;
using GestorFinanzas.Web.Models.Entities;

namespace GestorFinanzas.Web.Data.Seed;

/// <summary>
/// Categorías que existen desde el primer arranque. Si se cambia esta lista hay que generar
/// una migración nueva (dotnet ef migrations add NombreDelCambio) para que llegue a la base de datos.
/// </summary>
public static class CategoriasIniciales
{
    public static readonly Categoria[] Todas =
    {
        new() { Id = 1, Nombre = "Alimentación", Tipo = TipoMovimiento.Gasto, Icono = "bi-cup-hot" },
        new() { Id = 2, Nombre = "Transporte", Tipo = TipoMovimiento.Gasto, Icono = "bi-bus-front" },
        new() { Id = 3, Nombre = "Salud", Tipo = TipoMovimiento.Gasto, Icono = "bi-heart-pulse" },
        new() { Id = 4, Nombre = "Entretenimiento", Tipo = TipoMovimiento.Gasto, Icono = "bi-controller" },
        new() { Id = 5, Nombre = "Materiales de estudio", Tipo = TipoMovimiento.Gasto, Icono = "bi-book" },
        new() { Id = 6, Nombre = "Educación", Tipo = TipoMovimiento.Gasto, Icono = "bi-mortarboard" },
        new() { Id = 7, Nombre = "Servicios", Tipo = TipoMovimiento.Gasto, Icono = "bi-wifi" },
        new() { Id = 8, Nombre = "Ropa y compras", Tipo = TipoMovimiento.Gasto, Icono = "bi-bag" },
        new() { Id = 9, Nombre = "Gastos hormiga", Tipo = TipoMovimiento.Gasto, Icono = "bi-cookie" },
        new() { Id = 10, Nombre = "Otros gastos", Tipo = TipoMovimiento.Gasto, Icono = "bi-three-dots" },
        new() { Id = 11, Nombre = "Mesada o apoyo familiar", Tipo = TipoMovimiento.Ingreso, Icono = "bi-house-heart" },
        new() { Id = 12, Nombre = "Beca", Tipo = TipoMovimiento.Ingreso, Icono = "bi-award" },
        new() { Id = 13, Nombre = "Trabajo de medio tiempo", Tipo = TipoMovimiento.Ingreso, Icono = "bi-briefcase" },
        new() { Id = 14, Nombre = "Otros ingresos", Tipo = TipoMovimiento.Ingreso, Icono = "bi-cash-coin" }
    };
}
