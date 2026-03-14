using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SistemaRepuestosMaquinas.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductoController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult GetCatalogo() => Ok("TODO: catálogo público con filtros y paginación.");

    [HttpPost]
    [Authorize(Roles = "Administrador,Vendedor")]
    public IActionResult CrearProducto() => Ok("TODO: crear producto.");
}
