using GestorFinanzas.Web.Models.Entities;

namespace GestorFinanzas.Web.Services;

public interface IAutenticacionService
{
    /// <summary>
    /// Devuelve el usuario si el correo y la contraseña son correctos, o null si no lo son.
    /// No indica cuál de los dos datos falló, para no revelar qué correos están registrados.
    /// </summary>
    Task<Usuario?> ValidarCredencialesAsync(string correo, string contrasenia, CancellationToken cancellationToken = default);

    /// <summary>Crea un usuario nuevo con su cuenta de efectivo inicial.</summary>
    /// <exception cref="Infrastructure.ReglaNegocioException">Si el correo ya existe o la contraseña no cumple las reglas.</exception>
    Task<Usuario> RegistrarAsync(string nombre, string correo, string contrasenia, CancellationToken cancellationToken = default);
}
