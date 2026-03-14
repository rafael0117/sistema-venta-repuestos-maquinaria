using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaRepuestosMaquinas.Business.DTOs;
using SistemaRepuestosMaquinas.Data.Context;
using SistemaRepuestosMaquinas.Entity;

namespace SistemaRepuestosMaquinas.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CarritoController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet("cliente/{idCliente:int}")]
    public async Task<IActionResult> Get(int idCliente, CancellationToken cancellationToken)
    {
        var carrito = await context.Carritos
            .Include(x => x.Detalles)
            .ThenInclude(x => x.Producto)
            .FirstOrDefaultAsync(x => x.IdCliente == idCliente && x.Estado == "Abierto", cancellationToken);

        if (carrito is null)
        {
            return Ok(new { idCliente, detalles = Array.Empty<object>(), total = 0m });
        }

        var detalles = carrito.Detalles.Select(x => new
        {
            x.IdCarritoDetalle,
            x.IdProducto,
            Producto = x.Producto?.Nombre,
            x.Cantidad,
            x.PrecioUnitario,
            SubTotal = x.Cantidad * x.PrecioUnitario
        }).ToList();

        return Ok(new { carrito.IdCarrito, carrito.IdCliente, carrito.Estado, detalles, total = detalles.Sum(x => x.SubTotal) });
    }

    [HttpPost("cliente/{idCliente:int}/items")]
    public async Task<IActionResult> AddItem(int idCliente, [FromBody] AddCarritoItemRequest request, CancellationToken cancellationToken)
    {
        var producto = await context.Productos.FirstOrDefaultAsync(x => x.IdProducto == request.IdProducto && x.Estado, cancellationToken);
        if (producto is null) return NotFound(new { message = "Producto no encontrado." });
        if (request.Cantidad <= 0) return BadRequest(new { message = "Cantidad inválida." });

        var carrito = await context.Carritos
            .Include(x => x.Detalles)
            .FirstOrDefaultAsync(x => x.IdCliente == idCliente && x.Estado == "Abierto", cancellationToken);

        if (carrito is null)
        {
            carrito = new Carrito { IdCliente = idCliente, Estado = "Abierto" };
            context.Carritos.Add(carrito);
            await context.SaveChangesAsync(cancellationToken);
        }

        var detalle = carrito.Detalles.FirstOrDefault(x => x.IdProducto == request.IdProducto);
        if (detalle is null)
        {
            detalle = new CarritoDetalle
            {
                IdCarrito = carrito.IdCarrito,
                IdProducto = request.IdProducto,
                Cantidad = request.Cantidad,
                PrecioUnitario = producto.PrecioVenta
            };
            context.CarritoDetalles.Add(detalle);
        }
        else
        {
            detalle.Cantidad += request.Cantidad;
            detalle.PrecioUnitario = producto.PrecioVenta;
        }

        await context.SaveChangesAsync(cancellationToken);
        return Ok(detalle);
    }

    [HttpPut("items/{idCarritoDetalle:int}")]
    public async Task<IActionResult> UpdateItem(int idCarritoDetalle, [FromBody] UpdateCarritoItemRequest request, CancellationToken cancellationToken)
    {
        var detalle = await context.CarritoDetalles.FirstOrDefaultAsync(x => x.IdCarritoDetalle == idCarritoDetalle, cancellationToken);
        if (detalle is null) return NotFound();

        if (request.Cantidad <= 0)
        {
            context.CarritoDetalles.Remove(detalle);
        }
        else
        {
            detalle.Cantidad = request.Cantidad;
        }

        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("items/{idCarritoDetalle:int}")]
    public async Task<IActionResult> DeleteItem(int idCarritoDetalle, CancellationToken cancellationToken)
    {
        var detalle = await context.CarritoDetalles.FirstOrDefaultAsync(x => x.IdCarritoDetalle == idCarritoDetalle, cancellationToken);
        if (detalle is null) return NotFound();

        context.CarritoDetalles.Remove(detalle);
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("cliente/{idCliente:int}/checkout")]
    public async Task<IActionResult> Checkout(int idCliente, [FromBody] CheckoutRequest request, CancellationToken cancellationToken)
    {
        var carrito = await context.Carritos
            .Include(x => x.Detalles)
            .FirstOrDefaultAsync(x => x.IdCliente == idCliente && x.Estado == "Abierto", cancellationToken);

        if (carrito is null || carrito.Detalles.Count == 0)
            return BadRequest(new { message = "El carrito está vacío." });

        var productIds = carrito.Detalles.Select(x => x.IdProducto).Distinct().ToList();
        var productos = await context.Productos.Where(x => productIds.Contains(x.IdProducto)).ToDictionaryAsync(x => x.IdProducto, cancellationToken);

        foreach (var item in carrito.Detalles)
        {
            if (!productos.TryGetValue(item.IdProducto, out var p) || p.Stock < item.Cantidad)
                return BadRequest(new { message = $"Stock insuficiente para producto {item.IdProducto}." });
        }

        var pedido = new Pedido
        {
            IdCliente = idCliente,
            DireccionEntrega = request.DireccionEntrega,
            MetodoPago = request.MetodoPago,
            EstadoPedido = "Pendiente",
            Total = carrito.Detalles.Sum(x => x.Cantidad * x.PrecioUnitario),
            Detalles = carrito.Detalles.Select(x => new PedidoDetalle
            {
                IdProducto = x.IdProducto,
                Cantidad = x.Cantidad,
                PrecioUnitario = x.PrecioUnitario
            }).ToList()
        };

        foreach (var item in carrito.Detalles)
        {
            productos[item.IdProducto].Stock -= item.Cantidad;
        }

        carrito.Estado = "Cerrado";
        context.Pedidos.Add(pedido);
        await context.SaveChangesAsync(cancellationToken);

        return Ok(new { pedido.IdPedido, pedido.Total, pedido.EstadoPedido });
    }

    public record AddCarritoItemRequest(int IdProducto, int Cantidad);
    public record UpdateCarritoItemRequest(int Cantidad);
}
