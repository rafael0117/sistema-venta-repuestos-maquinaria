using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using SistemaRepuestosMaquinas.Web.Models;
using SistemaRepuestosMaquinas.Web.Services;

namespace SistemaRepuestosMaquinas.Web.Controllers;

public class CuentaController(ApiClient apiClient) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        var vm = new CuentaPageViewModel
        {
            Message = TempData["Message"] as string,
            IsError = (TempData["IsError"] as string) == "1"
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(CuentaPageViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.Message = "Completa los datos de login.";
            model.IsError = true;
            return View("Index", model);
        }

        var response = await apiClient.PostAsync("api/auth/login", model.Login, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            model.Message = "Credenciales inválidas.";
            model.IsError = true;
            return View("Index", model);
        }

        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>(cancellationToken);
        if (auth is null || string.IsNullOrWhiteSpace(auth.Token))
        {
            model.Message = "No se pudo obtener token de acceso.";
            model.IsError = true;
            return View("Index", model);
        }

        HttpContext.Session.SetString("jwt", auth.Token);
        HttpContext.Session.SetString("rol", auth.Rol ?? string.Empty);

        TempData["Message"] = "Inicio de sesión exitoso.";
        TempData["IsError"] = "0";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(CuentaPageViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            model.Message = "Completa los datos de registro.";
            model.IsError = true;
            return View("Index", model);
        }

        var response = await apiClient.PostAsync("api/auth/register", model.Register, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            model.Message = "No se pudo registrar la cuenta. Verifica el correo.";
            model.IsError = true;
            return View("Index", model);
        }

        TempData["Message"] = "Registro exitoso. Ahora puedes iniciar sesión.";
        TempData["IsError"] = "0";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("jwt");
        HttpContext.Session.Remove("rol");
        TempData["Message"] = "Sesión cerrada.";
        TempData["IsError"] = "0";
        return RedirectToAction(nameof(Index));
    }

    private sealed record AuthResponseDto(string Token, DateTime Expiration, string Rol);
}
