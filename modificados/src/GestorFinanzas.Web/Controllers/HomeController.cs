using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestorFinanzas.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    public IActionResult Index() => View();

    // Página para errores no controlados. No muestra detalles técnicos; el error queda en el log del servidor.
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View();

    // Página para códigos como 404 (no existe) o 400 (petición inválida).
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Estado(int id)
    {
        ViewData["Codigo"] = id;
        return View();
    }
}
