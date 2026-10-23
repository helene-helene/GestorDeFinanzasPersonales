using GestorFinanzas.Web.Models.Entities;

namespace GestorFinanzas.Web.Data.Repositories;

public interface IUsuarioRepository
{
    // Busca un usuario por su correo. La entidad queda rastreada para poder actualizarla.
    Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken cancellationToken = default);

    // Busca un usuario por su Id. Con conSeguimiento en true se puede modificar y guardar.
    Task<Usuario?> ObtenerPorIdAsync(int id, bool conSeguimiento = false, CancellationToken cancellationToken = default);

    // Indica si el correo ya lo usa otro usuario (se puede excluir al propio usuario).
    Task<bool> ExisteCorreoAsync(string correo, int? excluirUsuarioId = null, CancellationToken cancellationToken = default);

    Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default);
}
