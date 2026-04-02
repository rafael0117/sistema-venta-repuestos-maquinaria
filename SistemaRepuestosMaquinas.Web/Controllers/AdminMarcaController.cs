using Microsoft.AspNetCore.Mvc;
using SistemaRepuestosMaquinas.Web.Models;
using SistemaRepuestosMaquinas.Web.Services;

namespace SistemaRepuestosMaquinas.Web.Controllers;

public class AdminMarcaController(ApiClient apiClient) : AdminBaseController(apiClient)
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (!TryAuthorizeAdminOrVendedor(out var unauthorized)) return unauthorized!;

        var vm = new AdminMarcaPageViewModel
        {
            Message = TempData["Message"] as string,
            IsError = (TempData["IsError"] as string) == "1",
            Items = await ApiClient.GetAsync<List<AdminMarcaItem>>("api/marca", cancellationToken) ?? []
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminMarcaPageViewModel model, CancellationToken cancellationToken)
    {
        if (!TryAuthorizeAdminOrVendedor(out var unauthorized)) return unauthorized!;

        var response = await ApiClient.PostAsync("api/marca", model.Form, cancellationToken);
        return response.IsSuccessStatusCode
            ? RedirectWithMessage(nameof(Index), "AdminMarca", "Marca creada correctamente.")
            : RedirectWithMessage(nameof(Index), "AdminMarca", "No se pudo crear la marca.", true);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!TryAuthorizeAdminOrVendedor(out var unauthorized)) return unauthorized!;

        var response = await ApiClient.DeleteAsync($"api/marca/{id}", cancellationToken);
        return response.IsSuccessStatusCode
            ? RedirectWithMessage(nameof(Index), "AdminMarca", "Marca eliminada.")
            : RedirectWithMessage(nameof(Index), "AdminMarca", "No se pudo eliminar la marca.", true);
    }
}
