using GestorFinanzas.Web.Models.Entities;

namespace GestorFinanzas.Web.Services;

// Operaciones de "Mi perfil". Ningún método recibe un Id: siempre se trabaja con el usuario
// que inició sesión (ICurrentUserService), así no se puede ver ni modificar el perfil de otra persona.
public interface IPerfilService
{
    Task<Usuario?> ObtenerAsync(CancellationToken cancellationToken = default);

    Task<Usuario> ActualizarAsync(string nombre, string correo, CancellationToken cancellationToken = default);

    Task CambiarContraseniaAsync(string contraseniaActual, string contraseniaNueva, CancellationToken cancellationToken = default);
}
