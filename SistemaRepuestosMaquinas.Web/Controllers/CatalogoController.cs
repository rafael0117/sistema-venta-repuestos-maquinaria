using Microsoft.AspNetCore.Mvc;
using SistemaRepuestosMaquinas.Web.Models;
using SistemaRepuestosMaquinas.Web.Services;

namespace SistemaRepuestosMaquinas.Web.Controllers;

public class CatalogoController(ApiClient apiClient) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromQuery] CatalogoPageViewModel filtro, CancellationToken cancellationToken)
    {
        var query = $"api/producto?Texto={Uri.EscapeDataString(filtro.Texto ?? string.Empty)}&IdCategoria={filtro.IdCategoria}&IdMarca={filtro.IdMarca}&Page={filtro.Page}&PageSize={filtro.PageSize}";
        var data = await apiClient.GetAsync<CatalogoResponseDto>(query, cancellationToken);

        var vm = new CatalogoPageViewModel
        {
            Texto = filtro.Texto,
            IdCategoria = filtro.IdCategoria,
            IdMarca = filtro.IdMarca,
            Page = data?.Page ?? filtro.Page,
            PageSize = data?.PageSize ?? filtro.PageSize,
            Total = data?.Total ?? 0,
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
                ImagenUrl = x.ImagenUrl
            }).ToList() ?? []
        };

        return View(vm);
    }

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
}
