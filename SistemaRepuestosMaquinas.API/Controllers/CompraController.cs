using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaRepuestosMaquinas.Data.Context;
using SistemaRepuestosMaquinas.Entity;

namespace SistemaRepuestosMaquinas.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompraController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var data = await context.Compras
            .AsNoTracking()
            .Include(x => x.Proveedor)
            .Include(x => x.Detalles)
            .OrderByDescending(x => x.FechaCompra)
            .ToListAsync(cancellationToken);

        return Ok(data);
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> Create([FromBody] CreateCompraRequest request, CancellationToken cancellationToken)
    {
        if (request.Detalles.Count == 0) return BadRequest(new { message = "La compra no tiene detalles." });

        var productIds = request.Detalles.Select(x => x.IdProducto).Distinct().ToList();
        var productos = await context.Productos.Where(x => productIds.Contains(x.IdProducto)).ToDictionaryAsync(x => x.IdProducto, cancellationToken);

        foreach (var item in request.Detalles)
        {
            if (!productos.ContainsKey(item.IdProducto))
                return BadRequest(new { message = $"Producto {item.IdProducto} no existe." });
        }

        var compra = new Compra
        {
            IdProveedor = request.IdProveedor,
            FechaCompra = DateTime.UtcNow,
            Total = request.Detalles.Sum(x => x.Cantidad * x.PrecioCompra),
            Detalles = request.Detalles.Select(x => new DetalleCompra
            {
                IdProducto = x.IdProducto,
                Cantidad = x.Cantidad,
                PrecioCompra = x.PrecioCompra
            }).ToList()
        };

        foreach (var item in request.Detalles)
        {
            productos[item.IdProducto].Stock += item.Cantidad;
        }

        context.Compras.Add(compra);
        await context.SaveChangesAsync(cancellationToken);
        return Ok(compra);
    }

    public record CreateCompraRequest(int IdProveedor, List<CreateDetalleCompraRequest> Detalles);
    public record CreateDetalleCompraRequest(int IdProducto, int Cantidad, decimal PrecioCompra);
}
