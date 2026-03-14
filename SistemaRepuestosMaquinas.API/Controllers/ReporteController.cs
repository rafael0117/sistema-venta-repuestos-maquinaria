using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaRepuestosMaquinas.Data.Context;

namespace SistemaRepuestosMaquinas.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador,Vendedor")]
public class ReporteController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet("resumen")]
    public async Task<IActionResult> GetResumen(CancellationToken cancellationToken)
    {
        var hoy = DateTime.UtcNow.Date;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

        var totalProductos = await context.Productos.CountAsync(cancellationToken);
        var totalClientes = await context.Clientes.CountAsync(cancellationToken);
        var totalPedidos = await context.Pedidos.CountAsync(cancellationToken);
        var ventasMes = await context.Pedidos
            .Where(x => x.FechaPedido >= inicioMes)
            .SumAsync(x => (decimal?)x.Total, cancellationToken) ?? 0m;

        var comprasMes = await context.Compras
            .Where(x => x.FechaCompra >= inicioMes)
            .SumAsync(x => (decimal?)x.Total, cancellationToken) ?? 0m;

        return Ok(new
        {
            totalProductos,
            totalClientes,
            totalPedidos,
            ventasMes,
            comprasMes,
            utilidadBrutaMes = ventasMes - comprasMes
        });
    }
}
