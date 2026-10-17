using GestorFinanzas.Web.Models.Entities;

namespace GestorFinanzas.Web.Data.Repositories;

public interface IUsuarioRepository
{
    /// <summary>Busca un usuario por su correo. La entidad queda rastreada para poder actualizarla.</summary>
    Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken cancellationToken = default);

    /// <summary>Busca un usuario por su Id. Con conSeguimiento en true se puede modificar y guardar.</summary>
    Task<Usuario?> ObtenerPorIdAsync(int id, bool conSeguimiento = false, CancellationToken cancellationToken = default);

    /// <summary>Indica si el correo ya lo usa otro usuario (se puede excluir al propio usuario).</summary>
    Task<bool> ExisteCorreoAsync(string correo, int? excluirUsuarioId = null, CancellationToken cancellationToken = default);

    Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default);
}
