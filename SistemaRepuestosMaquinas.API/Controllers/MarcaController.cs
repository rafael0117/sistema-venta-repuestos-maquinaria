using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SistemaRepuestosMaquinas.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MarcaController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("TODO: endpoints de Marca.");
}
