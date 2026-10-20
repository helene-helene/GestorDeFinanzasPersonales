using System.Security.Claims;
using GestorFinanzas.Web.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace GestorFinanzas.Web.Infrastructure;

/// <summary>Crea y cierra la cookie de sesión del usuario.</summary>
public static class SesionUsuario
{
    /// <summary>
    /// Guarda en la cookie los claims del usuario: su Id, su nombre y su correo.
    /// La cookie va cifrada por ASP.NET Core, así que el navegador no puede leerla ni cambiarla.
    /// Nunca se guarda la contraseña ni su hash.
    /// </summary>
    public static Task IniciarSesionAsync(this HttpContext http, Usuario usuario, bool recordarme)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nombre),
            new(ClaimTypes.Email, usuario.Correo)
        };

        var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var propiedades = new AuthenticationProperties
        {
            // Con "Recordarme" la cookie sobrevive al cerrar el navegador (hasta que expire).
            IsPersistent = recordarme
        };

        return http.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidad),
            propiedades);
    }

    public static Task CerrarSesionAsync(this HttpContext http) =>
        http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
}
