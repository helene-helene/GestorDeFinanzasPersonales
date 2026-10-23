using GestorFinanzas.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestorFinanzas.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IInicioService _inicio;

    public HomeController(IInicioService inicio)
    {
        _inicio = inicio;
    }

    // Pantalla de Inicio: resumen del mes con los datos del usuario que inició sesión.
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await _inicio.ObtenerResumenAsync(User.Identity?.Name ?? string.Empty, cancellationToken));

    // Errores no controlados: no muestra detalles técnicos; el error queda en el log del servidor.
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();

    // Códigos como 404 (no existe) o 400 (petición inválida).
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Estado(int id)
    {
        ViewData["Codigo"] = id;
        return View();
    }
}
