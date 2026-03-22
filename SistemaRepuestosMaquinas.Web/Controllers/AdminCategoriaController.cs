using Microsoft.AspNetCore.Mvc;
using SistemaRepuestosMaquinas.Web.Models;
using SistemaRepuestosMaquinas.Web.Services;

namespace SistemaRepuestosMaquinas.Web.Controllers;

public class AdminCategoriaController(ApiClient apiClient) : AdminBaseController(apiClient)
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (!TryAuthorizeAdminOrVendedor(out var unauthorized)) return unauthorized!;

        var vm = new AdminCategoriaPageViewModel
        {
            Message = TempData["Message"] as string,
            IsError = (TempData["IsError"] as string) == "1",
            Items = await ApiClient.GetAsync<List<AdminCategoriaItem>>("api/categoria", cancellationToken) ?? []
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminCategoriaPageViewModel model, CancellationToken cancellationToken)
    {
        if (!TryAuthorizeAdminOrVendedor(out var unauthorized)) return unauthorized!;

        var response = await ApiClient.PostAsync("api/categoria", model.Form, cancellationToken);
        return response.IsSuccessStatusCode
            ? RedirectWithMessage(nameof(Index), "AdminCategoria", "Categoría creada correctamente.")
            : RedirectWithMessage(nameof(Index), "AdminCategoria", "No se pudo crear la categoría.", true);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!TryAuthorizeAdminOrVendedor(out var unauthorized)) return unauthorized!;

        var response = await ApiClient.DeleteAsync($"api/categoria/{id}", cancellationToken);
        return response.IsSuccessStatusCode
            ? RedirectWithMessage(nameof(Index), "AdminCategoria", "Categoría eliminada.")
            : RedirectWithMessage(nameof(Index), "AdminCategoria", "No se pudo eliminar la categoría.", true);
    }
}
