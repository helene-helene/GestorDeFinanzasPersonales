using GestorFinanzas.Web.Models.Entities;

namespace GestorFinanzas.Web.Services;

/// <summary>
/// Operaciones de "Mi perfil". Ningún método recibe un Id: siempre se trabaja con el usuario
/// que inició sesión (ICurrentUserService), así no se puede ver ni modificar el perfil de otra persona.
/// </summary>
public interface IPerfilService
{
    Task<Usuario?> ObtenerAsync(CancellationToken cancellationToken = default);

    /// <exception cref="Infrastructure.ReglaNegocioException">Si los datos no son válidos o el correo ya lo usa otra persona.</exception>
    Task<Usuario> ActualizarAsync(string nombre, string correo, CancellationToken cancellationToken = default);

    /// <exception cref="Infrastructure.ReglaNegocioException">Si la contraseña actual no coincide o la nueva no cumple las reglas.</exception>
    Task CambiarContraseniaAsync(string contraseniaActual, string contraseniaNueva, CancellationToken cancellationToken = default);
}
