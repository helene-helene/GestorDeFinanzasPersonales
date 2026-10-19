using Microsoft.AspNetCore.Mvc;

namespace GestorFinanzas.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();

    public IActionResult Error() => View();
}
