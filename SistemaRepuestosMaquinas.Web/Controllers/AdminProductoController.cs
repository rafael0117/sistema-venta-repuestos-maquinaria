using Microsoft.AspNetCore.Mvc;
using SistemaRepuestosMaquinas.Web.Models;
using SistemaRepuestosMaquinas.Web.Services;

namespace SistemaRepuestosMaquinas.Web.Controllers;

public class AdminProductoController(ApiClient apiClient) : AdminBaseController(apiClient)
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (!TryAuthorizeAdminOrVendedor(out var unauthorized)) return unauthorized!;

        var vm = new AdminProductoPageViewModel
        {
            Message = TempData["Message"] as string,
            IsError = (TempData["IsError"] as string) == "1"
        };

        var data = await ApiClient.GetAsync<ProductoListDto>("api/producto?Page=1&PageSize=200", cancellationToken);
        vm.Items = data?.Data?.Select(x => new AdminProductoItem
        {
            IdProducto = x.IdProducto,
            Codigo = x.Codigo ?? string.Empty,
            Nombre = x.Nombre ?? string.Empty,
            Descripcion = x.Descripcion,
            PrecioVenta = x.PrecioVenta,
            Stock = x.Stock,
            StockMinimo = x.StockMinimo,
            ImagenUrl = x.ImagenUrl
        }).ToList() ?? [];

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminProductoPageViewModel model, CancellationToken cancellationToken)
    {
        if (!TryAuthorizeAdminOrVendedor(out var unauthorized)) return unauthorized!;

        var response = await ApiClient.PostAsync("api/producto", model.Form, cancellationToken);
        return response.IsSuccessStatusCode
            ? RedirectWithMessage(nameof(Index), "AdminProducto", "Producto creado correctamente.")
            : RedirectWithMessage(nameof(Index), "AdminProducto", "No se pudo crear el producto.", true);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!TryAuthorizeAdminOrVendedor(out var unauthorized)) return unauthorized!;

        var response = await ApiClient.DeleteAsync($"api/producto/{id}", cancellationToken);
        return response.IsSuccessStatusCode
            ? RedirectWithMessage(nameof(Index), "AdminProducto", "Producto eliminado.")
            : RedirectWithMessage(nameof(Index), "AdminProducto", "No se pudo eliminar el producto.", true);
    }

    private sealed class ProductoListDto
    {
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
        public int StockMinimo { get; set; }
        public string? ImagenUrl { get; set; }
    }
}
