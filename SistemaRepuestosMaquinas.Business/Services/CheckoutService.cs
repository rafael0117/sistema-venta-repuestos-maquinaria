using Microsoft.EntityFrameworkCore;
using SistemaRepuestosMaquinas.Business.Interfaces;
using SistemaRepuestosMaquinas.Data.Context;
using SistemaRepuestosMaquinas.Entity;

namespace SistemaRepuestosMaquinas.Business.Services;

public class CheckoutService(ApplicationDbContext context) : ICheckoutService
{
    public async Task<CheckoutConfirmationResult> ConfirmCheckoutProAsync(
        int idCliente,
        string status,
        string? paymentId,
        string? preferenceId,
        string direccionEntrega,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(status, "approved", StringComparison.OrdinalIgnoreCase))
        {
            return new CheckoutConfirmationResult(false, "El pago aún no está aprobado.");
        }

        var carrito = await context.Carritos
            .Include(x => x.Detalles)
            .ThenInclude(x => x.Producto)
            .FirstOrDefaultAsync(x => x.IdCliente == idCliente && x.Estado == "Abierto", cancellationToken);

        if (carrito is null || carrito.Detalles.Count == 0)
            return new CheckoutConfirmationResult(false, "No existe un carrito abierto para confirmar.");

        var productIds = carrito.Detalles.Select(x => x.IdProducto).Distinct().ToList();
        var productos = await context.Productos
            .Where(x => productIds.Contains(x.IdProducto))
            .ToDictionaryAsync(x => x.IdProducto, cancellationToken);

        foreach (var item in carrito.Detalles)
        {
            if (!productos.TryGetValue(item.IdProducto, out var producto) || producto.Stock < item.Cantidad)
            {
                return new CheckoutConfirmationResult(false, $"Stock insuficiente para producto {item.IdProducto}.");
            }
        }

        var total = carrito.Detalles.Sum(x => x.Cantidad * x.PrecioUnitario);

        await using var tx = await context.Database.BeginTransactionAsync(cancellationToken);
        var pedido = new Pedido
        {
            IdCliente = idCliente,
            FechaPedido = DateTime.UtcNow,
            Total = total,
            EstadoPedido = "Pagado",
            DireccionEntrega = direccionEntrega,
            MetodoPago = "CHECKOUT_PRO",
            EstadoPago = "approved",
            ExternalPaymentId = paymentId,
            ExternalPreferenceId = preferenceId,
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
        await tx.CommitAsync(cancellationToken);

        return new CheckoutConfirmationResult(true, "Pago confirmado y pedido generado.", pedido.IdPedido, pedido.Total);
    }
}
