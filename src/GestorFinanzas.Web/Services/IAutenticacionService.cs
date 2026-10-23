using GestorFinanzas.Web.Models.Entities;

namespace GestorFinanzas.Web.Services;

public interface IAutenticacionService
{
    // Devuelve el usuario si el correo y la contraseña son correctos, o null si no lo son.
    // No indica cuál de los dos datos falló, para no revelar qué correos están registrados.
    Task<Usuario?> ValidarCredencialesAsync(string correo, string contrasenia, CancellationToken cancellationToken = default);

    // Crea un usuario nuevo con su cuenta de efectivo inicial.
    Task<Usuario> RegistrarAsync(string nombre, string correo, string contrasenia, CancellationToken cancellationToken = default);
}
