using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaRepuestosMaquinas.Data.Context;
using SistemaRepuestosMaquinas.Entity;

namespace SistemaRepuestosMaquinas.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MarcaController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var data = await context.Marcas.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync(cancellationToken);
        return Ok(data);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> Create([FromBody] Marca marca, CancellationToken cancellationToken)
    {
        context.Marcas.Add(marca);
        await context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = marca.IdMarca }, marca);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> Update(int id, [FromBody] Marca request, CancellationToken cancellationToken)
    {
        var marca = await context.Marcas.FirstOrDefaultAsync(x => x.IdMarca == id, cancellationToken);
        if (marca is null) return NotFound();

        marca.Nombre = request.Nombre;
        marca.Estado = request.Estado;
        await context.SaveChangesAsync(cancellationToken);
        return Ok(marca);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var marca = await context.Marcas.FirstOrDefaultAsync(x => x.IdMarca == id, cancellationToken);
        if (marca is null) return NotFound();

        context.Marcas.Remove(marca);
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
