using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaRepuestosMaquinas.Data.Context;

namespace SistemaRepuestosMaquinas.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PedidoController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var data = await context.Pedidos
            .AsNoTracking()
            .Include(x => x.Cliente)
            .OrderByDescending(x => x.FechaPedido)
            .Select(x => new
            {
                x.IdPedido,
                x.IdCliente,
                x.FechaPedido,
                x.Total,
                x.EstadoPedido,
                x.DireccionEntrega,
                x.MetodoPago
            })
            .ToListAsync(cancellationToken);

        return Ok(data);
    }

    [HttpGet("cliente/{idCliente:int}")]
    public async Task<IActionResult> GetByCliente(int idCliente, CancellationToken cancellationToken)
    {
        var data = await context.Pedidos
            .AsNoTracking()
            .Where(x => x.IdCliente == idCliente)
            .Include(x => x.Detalles)
            .ThenInclude(x => x.Producto)
            .OrderByDescending(x => x.FechaPedido)
            .Select(x => new
            {
                x.IdPedido,
                x.IdCliente,
                x.FechaPedido,
                x.Total,
                x.EstadoPedido,
                x.DireccionEntrega,
                x.MetodoPago,
                detalles = x.Detalles
                    .OrderBy(d => d.IdPedidoDetalle)
                    .Select(d => new
                    {
                        d.IdPedidoDetalle,
                        d.IdProducto,
                        producto = d.Producto != null ? d.Producto.Nombre : $"Producto {d.IdProducto}",
                        d.Cantidad,
                        d.PrecioUnitario,
                        d.SubTotal
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return Ok(data);
    }

    [HttpPut("{idPedido:int}/estado")]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> CambiarEstado(int idPedido, [FromBody] UpdateEstadoPedidoRequest request, CancellationToken cancellationToken)
    {
        var pedido = await context.Pedidos.FirstOrDefaultAsync(x => x.IdPedido == idPedido, cancellationToken);
        if (pedido is null) return NotFound();

        pedido.EstadoPedido = request.EstadoPedido;
        await context.SaveChangesAsync(cancellationToken);
        return Ok(pedido);
    }

    public record UpdateEstadoPedidoRequest(string EstadoPedido);
}
