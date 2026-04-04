using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaRepuestosMaquinas.Business.DTOs;
using SistemaRepuestosMaquinas.Data.Context;
using SistemaRepuestosMaquinas.Entity;

namespace SistemaRepuestosMaquinas.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductoController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCatalogo([FromQuery] ProductoFiltroRequest filtro, CancellationToken cancellationToken)
    {
        var query = context.Productos
            .AsNoTracking()
            .Include(x => x.Categoria)
            .Include(x => x.Marca)
            .AsQueryable();

        if (!filtro.IncludeInactive)
        {
            query = query.Where(x => x.Estado);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            var texto = filtro.Texto.Trim().ToLower();
            query = query.Where(x => x.Nombre.ToLower().Contains(texto) || (x.Descripcion ?? string.Empty).ToLower().Contains(texto));
        }

        if (filtro.IdCategoria.HasValue)
            query = query.Where(x => x.IdCategoria == filtro.IdCategoria.Value);

        if (filtro.IdMarca.HasValue)
            query = query.Where(x => x.IdMarca == filtro.IdMarca.Value);

        var page = Math.Max(1, filtro.Page);
        var pageSize = Math.Clamp(filtro.PageSize, 1, 100);
        var total = await query.CountAsync(cancellationToken);

        var data = await query
            .OrderBy(x => x.Nombre)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.IdProducto,
                x.Codigo,
                x.Nombre,
                x.Descripcion,
                x.PrecioVenta,
                x.Stock,
                x.StockMinimo,
                x.ImagenUrl,
                x.IdCategoria,
                x.IdMarca,
                x.Estado,
                Categoria = x.Categoria != null ? x.Categoria.Nombre : null,
                Marca = x.Marca != null ? x.Marca.Nombre : null
            })
            .ToListAsync(cancellationToken);

        return Ok(new { total, page, pageSize, data });
    }

    [HttpPost]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> CrearProducto([FromBody] Producto producto, CancellationToken cancellationToken)
    {
        context.Productos.Add(producto);
        await context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = producto.IdProducto }, producto);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await context.Productos
            .AsNoTracking()
            .Include(x => x.Categoria)
            .Include(x => x.Marca)
            .Where(x => x.IdProducto == id)
            .Select(x => new
            {
                x.IdProducto,
                x.Codigo,
                x.Nombre,
                x.Descripcion,
                x.PrecioVenta,
                x.Stock,
                x.StockMinimo,
                x.ImagenUrl,
                x.IdCategoria,
                Categoria = x.Categoria != null ? x.Categoria.Nombre : null,
                x.IdMarca,
                Marca = x.Marca != null ? x.Marca.Nombre : null,
                x.Estado
            })
            .FirstOrDefaultAsync(cancellationToken);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador,Vendedor")]
    public async Task<IActionResult> Update(int id, [FromBody] Producto request, CancellationToken cancellationToken)
    {
        var producto = await context.Productos.FirstOrDefaultAsync(x => x.IdProducto == id, cancellationToken);
        if (producto is null) return NotFound();

        producto.IdCategoria = request.IdCategoria;
        producto.IdMarca = request.IdMarca;
        producto.Codigo = request.Codigo;
        producto.Nombre = request.Nombre;
        producto.Descripcion = request.Descripcion;
        producto.PrecioVenta = request.PrecioVenta;
        producto.Stock = request.Stock;
        producto.StockMinimo = request.StockMinimo;
        producto.ImagenUrl = request.ImagenUrl;
        producto.Estado = request.Estado;

        await context.SaveChangesAsync(cancellationToken);
        return Ok(producto);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var producto = await context.Productos.FirstOrDefaultAsync(x => x.IdProducto == id, cancellationToken);
        if (producto is null) return NotFound();

        context.Productos.Remove(producto);
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
