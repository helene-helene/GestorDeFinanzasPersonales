using GestorFinanzas.Web.Models.Entities;

namespace GestorFinanzas.Web.Data.Repositories;

public interface ICuentaRepository
{
    // Cuentas del usuario, solo lectura, ordenadas por nombre.
    Task<IReadOnlyList<Cuenta>> ListarPorUsuarioAsync(int usuarioId);

    // Busca una cuenta del usuario. La entidad queda rastreada para poder modificar su saldo.
    Task<Cuenta?> ObtenerAsync(int id, int usuarioId);
}
