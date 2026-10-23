using GestorFinanzas.Web.Infrastructure;
using GestorFinanzas.Web.Services;
using GestorFinanzas.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestorFinanzas.Web.Controllers;

// Módulo "Mi perfil": ver y editar los datos del usuario que inició sesión.
[Authorize]
public class PerfilController : Controller
{
    private readonly IPerfilService _perfil;

    public PerfilController(IPerfilService perfil)
    {
        _perfil = perfil;
    }

    // GET /Perfil
    // Aunque alguien escriba /Perfil/Index/5, el Id se ignora: siempre se muestra el perfil propio.
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var usuario = await _perfil.ObtenerAsync(cancellationToken);
        if (usuario is null)
        {
            // La cookie es válida pero el usuario ya no existe en la base de datos.
            await HttpContext.CerrarSesionAsync();
            return RedirectToAction("Login", "Acceso");
        }

        var modelo = new PerfilViewModel
        {
            Nombre = usuario.Nombre,
            Correo = usuario.Correo,
            FechaRegistro = usuario.FechaRegistro
        };

        return View(modelo);
    }

    // POST /Perfil
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(PerfilViewModel modelo, CancellationToken cancellationToken)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var usuario = await _perfil.ActualizarAsync(modelo.Nombre, modelo.Correo, cancellationToken);

                // Se vuelve a crear la cookie para que el nuevo nombre aparezca en el menú
                // sin cerrar la sesión. Se respeta si el usuario había marcado "Recordarme".
                var sesionActual = await HttpContext.AuthenticateAsync();
                var recordarme = sesionActual.Properties?.IsPersistent ?? false;
                await HttpContext.IniciarSesionAsync(usuario, recordarme);

                TempData["Exito"] = "Tus datos se actualizaron correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (ReglaNegocioException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }

        // La fecha de registro no viaja en el formulario, se vuelve a leer para mostrarla.
        var actual = await _perfil.ObtenerAsync(cancellationToken);
        modelo.FechaRegistro = actual?.FechaRegistro ?? DateTime.MinValue;
        return View(modelo);
    }

    // GET /Perfil/CambiarContrasenia
    [HttpGet]
    public IActionResult CambiarContrasenia() => View(new CambiarContraseniaViewModel());

    // POST /Perfil/CambiarContrasenia
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarContrasenia(CambiarContraseniaViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(new CambiarContraseniaViewModel());
        }

        try
        {
            await _perfil.CambiarContraseniaAsync(modelo.ContraseniaActual, modelo.ContraseniaNueva, cancellationToken);
            TempData["Exito"] = "Tu contraseña se cambió correctamente.";
            return RedirectToAction(nameof(Index));
        }
        catch (ReglaNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(new CambiarContraseniaViewModel());
        }
    }
}
