namespace GestorFinanzas.Web.Services.IA;

// Resultado de clasificar un gasto: la categoría elegida y por qué.
public sealed record ClasificacionIA(int CategoriaId, string Categoria, string Motivo, string Proveedor);

// Una recomendación de ahorro o de control de gastos.
public sealed record RecomendacionGenerada(
    string Tipo, string Titulo, string Mensaje, string Prioridad, decimal? AhorroEstimado, string Proveedor);

// Módulo de IA: clasificación automática de gastos y recomendaciones de ahorro.
public interface IServicioIA
{
    // Sugiere la categoría de gasto que mejor describe el texto; null si no hay texto o categorías.
    Task<ClasificacionIA?> ClasificarGastoAsync(string descripcion, CancellationToken cancellationToken = default);

    // Recomendaciones personalizadas con los gastos, presupuestos y metas del usuario actual.
    Task<IReadOnlyList<RecomendacionGenerada>> GenerarRecomendacionesAsync(CancellationToken cancellationToken = default);
}
