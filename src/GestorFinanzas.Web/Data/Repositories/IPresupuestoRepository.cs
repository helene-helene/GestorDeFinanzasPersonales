using GestorFinanzas.Web.Models.Entities;

namespace GestorFinanzas.Web.Data.Repositories;

public interface IPresupuestoRepository
{
    Task<IReadOnlyList<Presupuesto>> ListarAsync(int usuarioId);

    // con conSeguimiento en true la entidad queda rastreada para editarla o borrarla
    Task<Presupuesto?> ObtenerAsync(int id, int usuarioId, bool conSeguimiento = false);

    // true si ya hay un presupuesto de esa categoría que se cruza con las fechas
    Task<bool> HaySolapamientoAsync(int usuarioId, int categoriaId, DateTime inicio, DateTime fin, int? excluirId = null);

    // suma de los gastos de la categoría entre las dos fechas (ambas incluidas)
    Task<decimal> TotalGastadoAsync(int usuarioId, int categoriaId, DateTime desde, DateTime hasta);

    Task AgregarAsync(Presupuesto presupuesto);

    void Eliminar(Presupuesto presupuesto);
}
