using Microsoft.AspNetCore.Mvc;

namespace SistemaRepuestosMaquinas.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
}
