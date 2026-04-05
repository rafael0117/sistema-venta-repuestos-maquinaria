using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SistemaRepuestosMaquinas.API.Configuration;
using SistemaRepuestosMaquinas.API.Services;
using SistemaRepuestosMaquinas.Business.DTOs;
using SistemaRepuestosMaquinas.Data.Context;
using SistemaRepuestosMaquinas.Entity;

namespace SistemaRepuestosMaquinas.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CarritoController(
    ApplicationDbContext context,
    IMercadoPagoService mercadoPagoService,
    IOptions<MercadoPagoOptions> mercadoPagoOptions,
    ILogger<CarritoController> logger) : ControllerBase
{
    [HttpGet("cliente/{idCliente:int}")]
    public async Task<IActionResult> Get(int idCliente, CancellationToken cancellationToken)
    {
        var carrito = await context.Carritos
            .Include(x => x.Detalles)
            .ThenInclude(x => x.Producto)
            .FirstOrDefaultAsync(x => x.IdCliente == idCliente && x.Estado == "Abierto", cancellationToken);

        if (carrito is null)
            return Ok(new { idCliente, detalles = Array.Empty<object>(), total = 0m });

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
            context.CarritoDetalles.Remove(detalle);
        else
            detalle.Cantidad = request.Cantidad;

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
            .ThenInclude(x => x.Producto)
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

        if (!request.MetodoPago.Equals("MERCADO_PAGO", StringComparison.OrdinalIgnoreCase) &&
            !request.MetodoPago.Equals("CHECKOUT_PRO", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Método de pago no soportado para checkout en línea." });
        }

        var email = string.IsNullOrWhiteSpace(request.EmailPago) ? "cliente@demo.com" : request.EmailPago.Trim();
        var baseWeb = mercadoPagoOptions.Value.WebAppBaseUrl.TrimEnd('/');

        var preference = await mercadoPagoService.CreateCheckoutProPreferenceAsync(
            new MercadoPagoPreferenceRequest(
                IdCliente: idCliente,
                Email: email,
                Items: carrito.Detalles.Select(x => new MercadoPagoPreferenceItem(
                    Title: x.Producto?.Nombre ?? $"Producto {x.IdProducto}",
                    Quantity: x.Cantidad,
                    UnitPrice: x.PrecioUnitario)).ToList(),
                SuccessUrl: $"{baseWeb}/Carrito/ConfirmacionCheckoutPro",
                FailureUrl: $"{baseWeb}/Carrito/ConfirmacionCheckoutPro",
                PendingUrl: $"{baseWeb}/Carrito/ConfirmacionCheckoutPro"),
            cancellationToken);

        if (!preference.IsSuccess || string.IsNullOrWhiteSpace(preference.RedirectUrl))
            return BadRequest(new { message = preference.Message });

        return Ok(new
        {
            redirectUrl = preference.RedirectUrl,
            preferenceId = preference.PreferenceId,
            message = preference.Message
        });
    }

    [HttpPost("cliente/{idCliente:int}/confirmacion-checkout-pro")]
    public async Task<IActionResult> ConfirmarCheckoutPro(int idCliente, [FromBody] ConfirmarCheckoutProRequest request, CancellationToken cancellationToken)
    {
        var status = request.Status;
        if (!string.Equals(status, "approved", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(request.PaymentId))
        {
            var lookup = await mercadoPagoService.GetPaymentByIdAsync(request.PaymentId, cancellationToken);
            if (lookup.IsSuccess)
                status = lookup.Status;
        }

        if (!string.Equals(status, "approved", StringComparison.OrdinalIgnoreCase))
        {
            return Ok(new
            {
                pedidoCreado = false,
                estado = status ?? "pending",
                message = "Pago aún no aprobado por Mercado Pago."
            });
        }

        var result = await RegistrarPedidoDesdeCarritoAsync(
            idCliente,
            request.DireccionEntrega,
            request.PaymentId,
            cancellationToken);

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("webhook/mercadopago")]
    [HttpPost("webhook/mercadopago")]
    public async Task<IActionResult> WebhookMercadoPago(
        [FromQuery(Name = "id")] string? paymentId,
        [FromQuery(Name = "data.id")] string? dataId,
        [FromQuery(Name = "topic")] string? topic,
        [FromQuery(Name = "type")] string? type,
        CancellationToken cancellationToken)
    {
        var resolvedPaymentId = string.IsNullOrWhiteSpace(paymentId) ? dataId : paymentId;
        var eventType = string.IsNullOrWhiteSpace(type) ? topic : type;

        if (!string.Equals(eventType, "payment", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(resolvedPaymentId))
            return Ok(new { received = true, ignored = true });

        var lookup = await mercadoPagoService.GetPaymentByIdAsync(resolvedPaymentId, cancellationToken);
        if (!lookup.IsSuccess)
        {
            logger.LogWarning("Webhook MP: no se pudo consultar payment {PaymentId}. {Message}", resolvedPaymentId, lookup.Message);
            return Ok(new { received = true, processed = false, message = lookup.Message });
        }

        if (!string.Equals(lookup.Status, "approved", StringComparison.OrdinalIgnoreCase))
            return Ok(new { received = true, processed = false, status = lookup.Status });

        var idCliente = ExtractClienteId(lookup.ExternalReference);
        if (!idCliente.HasValue)
        {
            logger.LogWarning("Webhook MP: external_reference inválido para payment {PaymentId}. ExternalReference={ExternalReference}", resolvedPaymentId, lookup.ExternalReference);
            return Ok(new { received = true, processed = false, message = "external_reference inválido" });
        }

        var result = await RegistrarPedidoDesdeCarritoAsync(idCliente.Value, null, lookup.PaymentId, cancellationToken);
        return Ok(new { received = true, processed = result.PedidoCreado, result.Message, result.Pedido });
    }

    private async Task<ConfirmacionCheckoutResult> RegistrarPedidoDesdeCarritoAsync(int idCliente, string? direccionEntrega, string? paymentId, CancellationToken cancellationToken)
    {
        var methodTag = string.IsNullOrWhiteSpace(paymentId) ? "CHECKOUT_PRO" : $"CHECKOUT_PRO:{paymentId}";

        if (!string.IsNullOrWhiteSpace(paymentId))
        {
            var duplicado = await context.Pedidos
                .AsNoTracking()
                .Where(x => x.IdCliente == idCliente && x.MetodoPago == methodTag)
                .OrderByDescending(x => x.IdPedido)
                .Select(x => new PedidoData(x.IdPedido, x.EstadoPedido, x.Total, x.FechaPedido))
                .FirstOrDefaultAsync(cancellationToken);

            if (duplicado is not null)
            {
                return new ConfirmacionCheckoutResult(true, "El pedido ya estaba registrado para este pago.", duplicado);
            }
        }

        var carrito = await context.Carritos
            .Include(x => x.Detalles)
            .ThenInclude(x => x.Producto)
            .FirstOrDefaultAsync(x => x.IdCliente == idCliente && x.Estado == "Abierto", cancellationToken);

        if (carrito is null || carrito.Detalles.Count == 0)
        {
            var ultimoPedido = await context.Pedidos
                .AsNoTracking()
                .Where(x => x.IdCliente == idCliente)
                .OrderByDescending(x => x.IdPedido)
                .Select(x => new PedidoData(x.IdPedido, x.EstadoPedido, x.Total, x.FechaPedido))
                .FirstOrDefaultAsync(cancellationToken);

            return new ConfirmacionCheckoutResult(false, "No hay carrito abierto para procesar.", ultimoPedido);
        }

        var productIds = carrito.Detalles.Select(x => x.IdProducto).Distinct().ToList();
        var productos = await context.Productos.Where(x => productIds.Contains(x.IdProducto)).ToDictionaryAsync(x => x.IdProducto, cancellationToken);

        foreach (var item in carrito.Detalles)
        {
            if (!productos.TryGetValue(item.IdProducto, out var p) || !p.Estado || p.Stock < item.Cantidad)
                return new ConfirmacionCheckoutResult(false, $"Stock insuficiente para producto {item.IdProducto}.", null);
        }

        var direccion = string.IsNullOrWhiteSpace(direccionEntrega)
            ? "Dirección no especificada"
            : direccionEntrega.Trim();

        var total = carrito.Detalles.Sum(x => x.Cantidad * x.PrecioUnitario);

        await using var tx = await context.Database.BeginTransactionAsync(cancellationToken);

        var pedido = new Pedido
        {
            IdCliente = idCliente,
            FechaPedido = DateTime.UtcNow,
            Total = total,
            EstadoPedido = "Pagado",
            DireccionEntrega = direccion,
            MetodoPago = methodTag
        };

        context.Pedidos.Add(pedido);
        await context.SaveChangesAsync(cancellationToken);

        foreach (var item in carrito.Detalles)
        {
            var producto = productos[item.IdProducto];
            producto.Stock -= item.Cantidad;

            context.PedidoDetalles.Add(new PedidoDetalle
            {
                IdPedido = pedido.IdPedido,
                IdProducto = item.IdProducto,
                Cantidad = item.Cantidad,
                PrecioUnitario = item.PrecioUnitario
            });
        }

        carrito.Estado = "Cerrado";
        await context.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return new ConfirmacionCheckoutResult(true, "Pedido registrado correctamente.", new PedidoData(pedido.IdPedido, pedido.EstadoPedido, pedido.Total, pedido.FechaPedido));
    }

    private static int? ExtractClienteId(string? externalReference)
    {
        if (string.IsNullOrWhiteSpace(externalReference)) return null;

        var tokens = externalReference.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (tokens.Length < 2) return null;

        return int.TryParse(tokens[1], out var idCliente) ? idCliente : null;
    }

    public record AddCarritoItemRequest(int IdProducto, int Cantidad);
    public record UpdateCarritoItemRequest(int Cantidad);
    public record ConfirmarCheckoutProRequest(string? Status, string? PaymentId, string? PreferenceId, string? DireccionEntrega);
    private sealed record PedidoData(int IdPedido, string EstadoPedido, decimal Total, DateTime FechaPedido);
    private sealed record ConfirmacionCheckoutResult(bool PedidoCreado, string Message, PedidoData? Pedido);
}
