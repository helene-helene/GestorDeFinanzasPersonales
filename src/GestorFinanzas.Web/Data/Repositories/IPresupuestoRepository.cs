using GestorFinanzas.Web.Models.Entities;

namespace GestorFinanzas.Web.Data.Repositories;

public interface IPresupuestoRepository
{
    Task<IReadOnlyList<Presupuesto>> ListarVigentesAsync(int usuarioId, DateTime fecha);
    Task<Presupuesto?> ObtenerAsync(int id, int usuarioId);
    Task<bool> ExisteAsync(int usuarioId, int categoriaId, DateTime fechaInicio);
    Task AgregarAsync(Presupuesto presupuesto);
    void Eliminar(Presupuesto presupuesto);
}
