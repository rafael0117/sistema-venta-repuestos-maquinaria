using Microsoft.AspNetCore.Mvc;
using SistemaRepuestosMaquinas.Web.Models;
using SistemaRepuestosMaquinas.Web.Services;

namespace SistemaRepuestosMaquinas.Web.Controllers;

public class CatalogoController(ApiClient apiClient) : Controller
{
    private const string DefaultProductImage = "/images/producto-default.svg";

    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] CatalogoPageViewModel filtro, CancellationToken cancellationToken)
    {
        var query = $"api/producto?Texto={Uri.EscapeDataString(filtro.Texto ?? string.Empty)}&IdCategoria={filtro.IdCategoria}&IdMarca={filtro.IdMarca}&Page={filtro.Page}&PageSize={filtro.PageSize}";
        var data = await apiClient.GetAsync<CatalogoResponseDto>(query, cancellationToken);
        var categorias = await apiClient.GetAsync<List<CatalogOptionDto>>("api/categoria", cancellationToken) ?? [];
        var marcas = await apiClient.GetAsync<List<CatalogOptionDto>>("api/marca", cancellationToken) ?? [];

        var vm = new CatalogoPageViewModel
        {
            Texto = filtro.Texto,
            IdCategoria = filtro.IdCategoria,
            IdMarca = filtro.IdMarca,
            Page = data?.Page ?? filtro.Page,
            PageSize = data?.PageSize ?? filtro.PageSize,
            Total = data?.Total ?? 0,
            Categorias = categorias
                .Where(x => x.Estado)
                .Select(x => new CatalogOptionItem { Id = x.Id, Nombre = x.Nombre ?? string.Empty })
                .ToList(),
            Marcas = marcas
                .Where(x => x.Estado)
                .Select(x => new CatalogOptionItem { Id = x.Id, Nombre = x.Nombre ?? string.Empty })
                .ToList(),
            Productos = data?.Data?.Select(x => new ProductoCatalogoItemViewModel
            {
                IdProducto = x.IdProducto,
                Codigo = x.Codigo ?? string.Empty,
                Nombre = x.Nombre ?? string.Empty,
                Descripcion = x.Descripcion,
                PrecioVenta = x.PrecioVenta,
                Stock = x.Stock,
                Categoria = x.Categoria,
                Marca = x.Marca,
                ImagenUrl = ResolveImageUrl(x.ImagenUrl)
            }).ToList() ?? []
        };

        return View(vm);
    }

    [HttpGet("catalogo/detalle/{idProducto:int}")]
    public async Task<IActionResult> Detalle(int idProducto, CancellationToken cancellationToken)
    {
        var producto = await apiClient.GetAsync<ProductoDto>($"api/producto/{idProducto}", cancellationToken);
        if (producto is null)
        {
            TempData["Message"] = "No se encontró el producto.";
            TempData["IsError"] = "1";
            return RedirectToAction(nameof(Index));
        }

        var vm = new ProductoDetalleViewModel
        {
            IdProducto = producto.IdProducto,
            Codigo = producto.Codigo ?? string.Empty,
            Nombre = producto.Nombre ?? string.Empty,
            Descripcion = producto.Descripcion,
            PrecioVenta = producto.PrecioVenta,
            Stock = producto.Stock,
            Categoria = producto.Categoria,
            Marca = producto.Marca,
            ImagenUrl = ResolveImageUrl(producto.ImagenUrl)
        };

        return View(vm);
    }

    private static string ResolveImageUrl(string? imageUrl)
        => string.IsNullOrWhiteSpace(imageUrl) ? DefaultProductImage : imageUrl;

    private sealed class CatalogoResponseDto
    {
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<ProductoDto>? Data { get; set; }
    }

    private sealed class ProductoDto
    {
        public int IdProducto { get; set; }
        public string? Codigo { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public decimal PrecioVenta { get; set; }
        public int Stock { get; set; }
        public string? Categoria { get; set; }
        public string? Marca { get; set; }
        public string? ImagenUrl { get; set; }
    }

    private sealed class CatalogOptionDto
    {
        public int IdCategoria { get; set; }
        public int IdMarca { get; set; }
        public string? Nombre { get; set; }
        public bool Estado { get; set; } = true;
        public int Id => IdCategoria != 0 ? IdCategoria : IdMarca;
    }
}
