using GestorFinanzas.Web.Models.Entities;

namespace GestorFinanzas.Web.Data.Repositories;

public interface ICuentaRepository
{
    /// <summary>Cuentas del usuario, solo lectura, ordenadas por nombre.</summary>
    Task<IReadOnlyList<Cuenta>> ListarPorUsuarioAsync(int usuarioId);

    /// <summary>Busca una cuenta del usuario. La entidad queda rastreada para poder modificar su saldo.</summary>
    Task<Cuenta?> ObtenerAsync(int id, int usuarioId);
}
