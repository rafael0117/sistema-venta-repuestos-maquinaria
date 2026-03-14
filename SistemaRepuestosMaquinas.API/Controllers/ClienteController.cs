using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaRepuestosMaquinas.Data.Context;

namespace SistemaRepuestosMaquinas.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClienteController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var data = await context.Clientes
            .AsNoTracking()
            .Include(x => x.Usuario)
            .Select(x => new
            {
                x.IdCliente,
                x.IdUsuario,
                x.TipoDocumento,
                x.NumeroDocumento,
                x.Telefono,
                x.Direccion,
                x.Estado,
                Nombres = x.Usuario != null ? x.Usuario.Nombres : string.Empty,
                Apellidos = x.Usuario != null ? x.Usuario.Apellidos : string.Empty,
                Correo = x.Usuario != null ? x.Usuario.Correo : string.Empty
            })
            .ToListAsync(cancellationToken);

        return Ok(data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await context.Clientes
            .AsNoTracking()
            .Include(x => x.Usuario)
            .Where(x => x.IdCliente == id)
            .Select(x => new
            {
                x.IdCliente,
                x.IdUsuario,
                x.TipoDocumento,
                x.NumeroDocumento,
                x.Telefono,
                x.Direccion,
                x.Estado,
                Nombres = x.Usuario != null ? x.Usuario.Nombres : string.Empty,
                Apellidos = x.Usuario != null ? x.Usuario.Apellidos : string.Empty,
                Correo = x.Usuario != null ? x.Usuario.Correo : string.Empty
            })
            .FirstOrDefaultAsync(cancellationToken);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClienteRequest request, CancellationToken cancellationToken)
    {
        var cliente = await context.Clientes.FirstOrDefaultAsync(x => x.IdCliente == id, cancellationToken);
        if (cliente is null) return NotFound();

        cliente.TipoDocumento = request.TipoDocumento;
        cliente.NumeroDocumento = request.NumeroDocumento;
        cliente.Telefono = request.Telefono;
        cliente.Direccion = request.Direccion;
        await context.SaveChangesAsync(cancellationToken);

        return Ok(cliente);
    }

    public record UpdateClienteRequest(string TipoDocumento, string NumeroDocumento, string Telefono, string Direccion);
}
