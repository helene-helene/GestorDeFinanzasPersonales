using GestorFinanzas.Web.Infrastructure;
using GestorFinanzas.Web.Services;
using GestorFinanzas.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestorFinanzas.Web.Controllers;

/// <summary>Inicio de sesión, registro y cierre de sesión.</summary>
public class AccesoController : Controller
{
    private readonly IAutenticacionService _autenticacion;

    public AccesoController(IAutenticacionService autenticacion)
    {
        _autenticacion = autenticacion;
    }

    // GET /Acceso/Login
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // POST /Acceso/Login
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var usuario = await _autenticacion.ValidarCredencialesAsync(modelo.Correo, modelo.Contrasenia, cancellationToken);
        if (usuario is null)
        {
            // Mismo mensaje si el correo no existe o si la contraseña está mal.
            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
            return View(modelo);
        }

        await HttpContext.IniciarSesionAsync(usuario, modelo.Recordarme);

        // Solo se regresa a direcciones del mismo sitio, para evitar redirecciones a páginas externas.
        if (!string.IsNullOrEmpty(modelo.ReturnUrl) && Url.IsLocalUrl(modelo.ReturnUrl))
        {
            return LocalRedirect(modelo.ReturnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    // GET /Acceso/Registro
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Registro()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new RegistroViewModel());
    }

    // POST /Acceso/Registro
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registro(RegistroViewModel modelo, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        try
        {
            var usuario = await _autenticacion.RegistrarAsync(modelo.Nombre, modelo.Correo, modelo.Contrasenia, cancellationToken);
            await HttpContext.IniciarSesionAsync(usuario, recordarme: false);
            TempData["Exito"] = $"¡Bienvenido, {usuario.Nombre}! Tu cuenta se creó con una cuenta de Efectivo en $0.00.";
            return RedirectToAction("Index", "Home");
        }
        catch (ReglaNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(modelo);
        }
    }

    // POST /Acceso/Logout  (es POST con token para que otro sitio no pueda cerrar la sesión con un simple enlace)
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.CerrarSesionAsync();
        TempData["Exito"] = "Cerraste sesión correctamente.";
        return RedirectToAction(nameof(Login));
    }
}
