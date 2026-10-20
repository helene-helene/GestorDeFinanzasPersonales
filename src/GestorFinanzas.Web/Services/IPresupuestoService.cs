using GestorFinanzas.Web.Models;
using GestorFinanzas.Web.Models.Entities;

namespace GestorFinanzas.Web.Services;

public interface IPresupuestoService
{
    Task<IReadOnlyList<PresupuestoResumen>> ListarAsync();
    Task<Presupuesto?> ObtenerAsync(int id);

    /// <exception cref="Infrastructure.ReglaNegocioException">Si algún dato incumple una regla de negocio.</exception>
    Task<Presupuesto> CrearAsync(PresupuestoDatos datos);

    /// <exception cref="Infrastructure.ReglaNegocioException">Si algún dato incumple una regla de negocio.</exception>
    Task<Presupuesto> ActualizarAsync(int id, PresupuestoDatos datos);

    /// <exception cref="Infrastructure.ReglaNegocioException">Si el presupuesto no existe.</exception>
    Task EliminarAsync(int id);

    // solo categorías de gasto, un presupuesto no tiene sentido para un ingreso
    Task<IReadOnlyList<Categoria>> ListarCategoriasDeGastoAsync();
}
