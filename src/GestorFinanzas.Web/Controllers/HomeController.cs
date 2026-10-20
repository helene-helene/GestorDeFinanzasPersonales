using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestorFinanzas.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    public IActionResult Index() => View();

    // Página para errores no controlados. No muestra detalles técnicos; el error queda en el log del servidor.
    // IgnoreAntiforgeryToken: si el error ocurrió en un formulario POST, esta página también se pide
    // como POST y no debe volver a exigir el token (si no, se mostraría en blanco).
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();

    // Página para códigos como 404 (no existe) o 400 (petición inválida).
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Estado(int id)
    {
        ViewData["Codigo"] = id;
        return View();
    }
}
