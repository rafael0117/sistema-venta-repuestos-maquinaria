using Microsoft.AspNetCore.Mvc;
using SistemaRepuestosMaquinas.Web.Models;
using SistemaRepuestosMaquinas.Web.Services;

namespace SistemaRepuestosMaquinas.Web.Controllers;

public class AdminProveedorController(ApiClient apiClient) : AdminBaseController(apiClient)
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (!TryAuthorizeAdminOrVendedor(out var unauthorized)) return unauthorized!;

        var vm = new AdminProveedorPageViewModel
        {
            Message = TempData["Message"] as string,
            IsError = (TempData["IsError"] as string) == "1",
            Items = await ApiClient.GetAsync<List<AdminProveedorItem>>("api/proveedor", cancellationToken) ?? []
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminProveedorPageViewModel model, CancellationToken cancellationToken)
    {
        if (!TryAuthorizeAdminOrVendedor(out var unauthorized)) return unauthorized!;

        var response = await ApiClient.PostAsync("api/proveedor", model.Form, cancellationToken);
        return response.IsSuccessStatusCode
            ? RedirectWithMessage(nameof(Index), "AdminProveedor", "Proveedor creado correctamente.")
            : RedirectWithMessage(nameof(Index), "AdminProveedor", "No se pudo crear el proveedor.", true);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!TryAuthorizeAdminOrVendedor(out var unauthorized)) return unauthorized!;

        var response = await ApiClient.DeleteAsync($"api/proveedor/{id}", cancellationToken);
        return response.IsSuccessStatusCode
            ? RedirectWithMessage(nameof(Index), "AdminProveedor", "Proveedor eliminado.")
            : RedirectWithMessage(nameof(Index), "AdminProveedor", "No se pudo eliminar el proveedor.", true);
    }
}
