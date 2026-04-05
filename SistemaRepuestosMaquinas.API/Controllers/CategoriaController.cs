using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaRepuestosMaquinas.Data.Context;
using SistemaRepuestosMaquinas.Entity;

namespace SistemaRepuestosMaquinas.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriaController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var data = await context.Categorias.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync(cancellationToken);
        return Ok(data);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> Create([FromBody] Categoria categoria, CancellationToken cancellationToken)
    {
        context.Categorias.Add(categoria);
        await context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = categoria.IdCategoria }, categoria);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> Update(int id, [FromBody] Categoria request, CancellationToken cancellationToken)
    {
        var categoria = await context.Categorias.FirstOrDefaultAsync(x => x.IdCategoria == id, cancellationToken);
        if (categoria is null) return NotFound();

        categoria.Nombre = request.Nombre;
        categoria.Descripcion = request.Descripcion;
        categoria.Estado = request.Estado;
        await context.SaveChangesAsync(cancellationToken);
        return Ok(categoria);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var categoria = await context.Categorias.FirstOrDefaultAsync(x => x.IdCategoria == id, cancellationToken);
        if (categoria is null) return NotFound();

        context.Categorias.Remove(categoria);
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
