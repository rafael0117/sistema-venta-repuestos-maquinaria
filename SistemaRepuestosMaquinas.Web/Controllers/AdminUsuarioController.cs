using Microsoft.AspNetCore.Mvc;
using SistemaRepuestosMaquinas.Web.Models;
using SistemaRepuestosMaquinas.Web.Services;

namespace SistemaRepuestosMaquinas.Web.Controllers;

public class AdminUsuarioController(ApiClient apiClient) : AdminBaseController(apiClient)
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (!TryAuthorizeAdmin(out var unauthorized)) return unauthorized!;

        var vm = new AdminUsuarioPageViewModel
        {
            Message = TempData["Message"] as string,
            IsError = (TempData["IsError"] as string) == "1",
            Items = await ApiClient.GetAsync<List<AdminUsuarioItem>>("api/usuario", cancellationToken) ?? [],
            Roles = await ApiClient.GetAsync<List<AdminRolItem>>("api/usuario/roles", cancellationToken) ?? []
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminUsuarioPageViewModel model, CancellationToken cancellationToken)
    {
        if (!TryAuthorizeAdmin(out var unauthorized)) return unauthorized!;

        var response = await ApiClient.PostAsync("api/usuario", model.Form, cancellationToken);
        return response.IsSuccessStatusCode
            ? RedirectWithMessage(nameof(Index), "AdminUsuario", "Usuario creado correctamente.")
            : RedirectWithMessage(nameof(Index), "AdminUsuario", "No se pudo crear el usuario.", true);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(AdminUsuarioItem item, CancellationToken cancellationToken)
    {
        if (!TryAuthorizeAdmin(out var unauthorized)) return unauthorized!;

        var response = await ApiClient.PutAsync($"api/usuario/{item.IdUsuario}", item, cancellationToken);
        return response.IsSuccessStatusCode
            ? RedirectWithMessage(nameof(Index), "AdminUsuario", "Usuario actualizado.")
            : RedirectWithMessage(nameof(Index), "AdminUsuario", "No se pudo actualizar el usuario.", true);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleEstado(int id, bool estadoActual, CancellationToken cancellationToken)
    {
        if (!TryAuthorizeAdmin(out var unauthorized)) return unauthorized!;

        var item = (await ApiClient.GetAsync<List<AdminUsuarioItem>>("api/usuario", cancellationToken))?.FirstOrDefault(x => x.IdUsuario == id);
        if (item is null) return RedirectWithMessage(nameof(Index), "AdminUsuario", "Usuario no encontrado.", true);

        item.Estado = !estadoActual;
        var response = await ApiClient.PutAsync($"api/usuario/{id}", item, cancellationToken);

        return response.IsSuccessStatusCode
            ? RedirectWithMessage(nameof(Index), "AdminUsuario", $"Estado cambiado a {(item.Estado ? "Activo" : "Inactivo")}.")
            : RedirectWithMessage(nameof(Index), "AdminUsuario", "No se pudo cambiar el estado.", true);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(int id, string newPassword, CancellationToken cancellationToken)
    {
        if (!TryAuthorizeAdmin(out var unauthorized)) return unauthorized!;

        var response = await ApiClient.PostAsync($"api/usuario/{id}/reset-password", new { newPassword }, cancellationToken);
        return response.IsSuccessStatusCode
            ? RedirectWithMessage(nameof(Index), "AdminUsuario", "Contraseña restablecida correctamente.")
            : RedirectWithMessage(nameof(Index), "AdminUsuario", "No se pudo restablecer la contraseña.", true);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!TryAuthorizeAdmin(out var unauthorized)) return unauthorized!;

        var response = await ApiClient.DeleteAsync($"api/usuario/{id}", cancellationToken);
        return response.IsSuccessStatusCode
            ? RedirectWithMessage(nameof(Index), "AdminUsuario", "Usuario eliminado.")
            : RedirectWithMessage(nameof(Index), "AdminUsuario", "No se pudo eliminar el usuario.", true);
    }
}
