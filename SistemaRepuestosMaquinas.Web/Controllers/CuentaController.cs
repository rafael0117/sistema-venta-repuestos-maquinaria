using System.Net.Http.Json;
using System.Text.Json;
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
    public async Task<IActionResult> Login([Bind(Prefix = "Login")] LoginViewModel login, CancellationToken cancellationToken)
    {
        if (!TryValidateModel(login, nameof(CuentaPageViewModel.Login)))
        {
            return View("Index", new CuentaPageViewModel
            {
                Login = login,
                Message = "Completa correctamente correo y contraseña.",
                IsError = true
            });
        }

        HttpResponseMessage response;
        try
        {
            response = await apiClient.PostAsync("api/auth/login", login, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return View("Index", new CuentaPageViewModel
            {
                Login = login,
                Message = "No hay conexión con la API (verifica que esté ejecutándose en la URL configurada).",
                IsError = true
            });
        }

        if (!response.IsSuccessStatusCode)
        {
            var message = await TryReadMessageAsync(response, "Credenciales inválidas. Revisa tu correo y contraseña.", cancellationToken);
            return View("Index", new CuentaPageViewModel
            {
                Login = login,
                Message = message,
                IsError = true
            });
        }

        var auth = await response.Content.ReadFromJsonAsync<AuthResponseDto>(cancellationToken);
        if (auth is null || string.IsNullOrWhiteSpace(auth.Token))
        {
            return View("Index", new CuentaPageViewModel
            {
                Login = login,
                Message = "El servidor no devolvió un token válido.",
                IsError = true
            });
        }

        HttpContext.Session.SetString("jwt", auth.Token);
        HttpContext.Session.SetString("rol", auth.Rol ?? string.Empty);

        TempData["Message"] = $"Bienvenido, sesión iniciada ({auth.Rol}).";
        TempData["IsError"] = "0";
        return RedirectToAction("Index", "Catalogo");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register([Bind(Prefix = "Register")] RegisterViewModel register, CancellationToken cancellationToken)
    {
        if (!TryValidateModel(register, nameof(CuentaPageViewModel.Register)))
        {
            return View("Index", new CuentaPageViewModel
            {
                Register = register,
                Message = "Completa todos los campos del registro.",
                IsError = true
            });
        }

        HttpResponseMessage response;
        try
        {
            response = await apiClient.PostAsync("api/auth/register", register, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return View("Index", new CuentaPageViewModel
            {
                Register = register,
                Message = "No hay conexión con la API (verifica que esté ejecutándose en la URL configurada).",
                IsError = true
            });
        }

        if (!response.IsSuccessStatusCode)
        {
            var message = await TryReadMessageAsync(response, "No se pudo registrar la cuenta. Verifica el correo.", cancellationToken);
            return View("Index", new CuentaPageViewModel
            {
                Register = register,
                Message = message,
                IsError = true
            });
        }

        TempData["Message"] = "Registro exitoso. Ahora inicia sesión con tu correo y contraseña.";
        TempData["IsError"] = "0";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("jwt");
        HttpContext.Session.Remove("rol");
        TempData["Message"] = "Sesión cerrada correctamente.";
        TempData["IsError"] = "0";
        return RedirectToAction(nameof(Index));
    }

    private static async Task<string> TryReadMessageAsync(HttpResponseMessage response, string fallback, CancellationToken cancellationToken)
    {
        try
        {
            var payload = await response.Content.ReadFromJsonAsync<ErrorDto>(cancellationToken);
            return string.IsNullOrWhiteSpace(payload?.Message) ? fallback : payload.Message;
        }
        catch (JsonException)
        {
            return fallback;
        }
    }

    private sealed record AuthResponseDto(string Token, DateTime Expiration, string Rol);
    private sealed record ErrorDto(string? Message);
}