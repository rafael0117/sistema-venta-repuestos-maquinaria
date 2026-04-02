using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaRepuestosMaquinas.Data.Context;
using SistemaRepuestosMaquinas.Entity;

namespace SistemaRepuestosMaquinas.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProveedorController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var data = await context.Proveedores.AsNoTracking().OrderBy(x => x.RazonSocial).ToListAsync(cancellationToken);
        return Ok(data);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> Create([FromBody] Proveedor proveedor, CancellationToken cancellationToken)
    {
        context.Proveedores.Add(proveedor);
        await context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = proveedor.IdProveedor }, proveedor);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> Update(int id, [FromBody] Proveedor request, CancellationToken cancellationToken)
    {
        var proveedor = await context.Proveedores.FirstOrDefaultAsync(x => x.IdProveedor == id, cancellationToken);
        if (proveedor is null) return NotFound();

        proveedor.Ruc = request.Ruc;
        proveedor.RazonSocial = request.RazonSocial;
        proveedor.Telefono = request.Telefono;
        proveedor.Correo = request.Correo;
        proveedor.Direccion = request.Direccion;
        proveedor.Estado = request.Estado;

        await context.SaveChangesAsync(cancellationToken);
        return Ok(proveedor);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var proveedor = await context.Proveedores.FirstOrDefaultAsync(x => x.IdProveedor == id, cancellationToken);
        if (proveedor is null) return NotFound();

        context.Proveedores.Remove(proveedor);
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
