using Microsoft.AspNetCore.Mvc;

namespace SistemaRepuestosMaquinas.Web.Controllers;

public class CatalogoController : Controller
{
    public IActionResult Index() => View();
}
