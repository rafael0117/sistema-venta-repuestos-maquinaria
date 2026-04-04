using Microsoft.AspNetCore.Mvc;
using SistemaRepuestosMaquinas.Web.Models;
using SistemaRepuestosMaquinas.Web.Services;

namespace SistemaRepuestosMaquinas.Web.Controllers;

public class AdminMarcaController(ApiClient apiClient) : AdminBaseController(apiClient)
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (!TryAuthorizeAdmin(out var unauthorized)) return unauthorized!;

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
        if (!TryAuthorizeAdmin(out var unauthorized)) return unauthorized!;

        var response = await ApiClient.PostAsync("api/marca", model.Form, cancellationToken);
        return response.IsSuccessStatusCode
            ? RedirectWithMessage(nameof(Index), "AdminMarca", "Marca creada correctamente.")
            : RedirectWithMessage(nameof(Index), "AdminMarca", "No se pudo crear la marca.", true);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(AdminMarcaItem item, CancellationToken cancellationToken)
    {
        if (!TryAuthorizeAdmin(out var unauthorized)) return unauthorized!;

        var response = await ApiClient.PutAsync($"api/marca/{item.IdMarca}", item, cancellationToken);
        return response.IsSuccessStatusCode
            ? RedirectWithMessage(nameof(Index), "AdminMarca", "Marca actualizada.")
            : RedirectWithMessage(nameof(Index), "AdminMarca", "No se pudo actualizar la marca.", true);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleEstado(int id, bool estadoActual, CancellationToken cancellationToken)
    {
        if (!TryAuthorizeAdmin(out var unauthorized)) return unauthorized!;

        var item = (await ApiClient.GetAsync<List<AdminMarcaItem>>("api/marca", cancellationToken))?.FirstOrDefault(x => x.IdMarca == id);
        if (item is null)
            return RedirectWithMessage(nameof(Index), "AdminMarca", "Marca no encontrada.", true);

        item.Estado = !estadoActual;
        var response = await ApiClient.PutAsync($"api/marca/{id}", item, cancellationToken);
        return response.IsSuccessStatusCode
            ? RedirectWithMessage(nameof(Index), "AdminMarca", $"Estado cambiado a {(item.Estado ? "Activo" : "Inactivo")}.")
            : RedirectWithMessage(nameof(Index), "AdminMarca", "No se pudo cambiar el estado.", true);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!TryAuthorizeAdmin(out var unauthorized)) return unauthorized!;

        var response = await ApiClient.DeleteAsync($"api/marca/{id}", cancellationToken);
        return response.IsSuccessStatusCode
            ? RedirectWithMessage(nameof(Index), "AdminMarca", "Marca eliminada.")
            : RedirectWithMessage(nameof(Index), "AdminMarca", "No se pudo eliminar la marca.", true);
    }
}
