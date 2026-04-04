using Microsoft.AspNetCore.Mvc;
using SistemaRepuestosMaquinas.Web.Models;
using SistemaRepuestosMaquinas.Web.Services;

namespace SistemaRepuestosMaquinas.Web.Controllers;

public class AdminProductoController(ApiClient apiClient) : AdminBaseController(apiClient)
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (!TryAuthorizeAdmin(out var unauthorized)) return unauthorized!;

        var vm = new AdminProductoPageViewModel
        {
            Message = TempData["Message"] as string,
            IsError = (TempData["IsError"] as string) == "1"
        };

        var data = await ApiClient.GetAsync<ProductoListDto>("api/producto?Page=1&PageSize=400&IncludeInactive=true", cancellationToken);
        vm.Items = data?.Data?.Select(x => new AdminProductoItem
        {
            IdProducto = x.IdProducto,
            IdCategoria = x.IdCategoria,
            IdMarca = x.IdMarca,
            Codigo = x.Codigo ?? string.Empty,
            Nombre = x.Nombre ?? string.Empty,
            Descripcion = x.Descripcion,
            PrecioVenta = x.PrecioVenta,
            Stock = x.Stock,
            StockMinimo = x.StockMinimo,
            ImagenUrl = x.ImagenUrl,
            Estado = x.Estado
        }).ToList() ?? [];

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminProductoPageViewModel model, CancellationToken cancellationToken)
    {
        if (!TryAuthorizeAdmin(out var unauthorized)) return unauthorized!;

        var response = await ApiClient.PostAsync("api/producto", model.Form, cancellationToken);
        return response.IsSuccessStatusCode
            ? RedirectWithMessage(nameof(Index), "AdminProducto", "Producto creado correctamente.")
            : RedirectWithMessage(nameof(Index), "AdminProducto", "No se pudo crear el producto.", true);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(AdminProductoItem item, CancellationToken cancellationToken)
    {
        if (!TryAuthorizeAdmin(out var unauthorized)) return unauthorized!;

        var response = await ApiClient.PutAsync($"api/producto/{item.IdProducto}", item, cancellationToken);
        return response.IsSuccessStatusCode
            ? RedirectWithMessage(nameof(Index), "AdminProducto", "Producto actualizado correctamente.")
            : RedirectWithMessage(nameof(Index), "AdminProducto", "No se pudo actualizar el producto.", true);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleEstado(int id, bool estadoActual, CancellationToken cancellationToken)
    {
        if (!TryAuthorizeAdmin(out var unauthorized)) return unauthorized!;

        var product = await ApiClient.GetAsync<AdminProductoItem>($"api/producto/{id}", cancellationToken);
        if (product is null)
        {
            return RedirectWithMessage(nameof(Index), "AdminProducto", "Producto no encontrado.", true);
        }

        product.Estado = !estadoActual;
        var response = await ApiClient.PutAsync($"api/producto/{id}", product, cancellationToken);

        return response.IsSuccessStatusCode
            ? RedirectWithMessage(nameof(Index), "AdminProducto", $"Estado cambiado a {(product.Estado ? "Activo" : "Inactivo")}.")
            : RedirectWithMessage(nameof(Index), "AdminProducto", "No se pudo cambiar el estado.", true);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!TryAuthorizeAdmin(out var unauthorized)) return unauthorized!;

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
        public int IdCategoria { get; set; }
        public int IdMarca { get; set; }
        public string? Codigo { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public decimal PrecioVenta { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; }
        public string? ImagenUrl { get; set; }
        public bool Estado { get; set; }
    }
}
