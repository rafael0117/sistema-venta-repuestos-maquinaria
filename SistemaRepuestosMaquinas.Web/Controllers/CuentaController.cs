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

    [HttpGet]
    public IActionResult Registro()
    {
        return View(new RegisterPageViewModel
        {
            Message = TempData["Message"] as string,
            IsError = (TempData["IsError"] as string) == "1"
        });
    }

    [HttpGet]
    public IActionResult RecuperarContrasena()
    {
        return View(new ForgotPasswordPageViewModel
        {
            Message = TempData["Message"] as string,
            IsError = (TempData["IsError"] as string) == "1"
        });
    }

    [HttpGet]
    public IActionResult RestablecerContrasena(string token)
    {
        return View(new ResetPasswordPageViewModel
        {
            ResetPassword = new ResetPasswordViewModel { Token = token ?? string.Empty },
            Message = TempData["Message"] as string,
            IsError = (TempData["IsError"] as string) == "1"
        });
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
        if (auth.IdCliente.HasValue)
            HttpContext.Session.SetString("idCliente", auth.IdCliente.Value.ToString());

        TempData["Message"] = $"Bienvenido, sesión iniciada ({auth.Rol}).";
        TempData["IsError"] = "0";
        return RedirectToAction("Index", "Catalogo");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register([Bind(Prefix = "Register")] RegisterViewModel register, CancellationToken cancellationToken)
    {
        if (!TryValidateModel(register, nameof(RegisterPageViewModel.Register)))
        {
            return View("Registro", new RegisterPageViewModel
            {
                Register = register,
                Message = "Completa todos los campos del registro con datos válidos.",
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
            return View("Registro", new RegisterPageViewModel
            {
                Register = register,
                Message = "No hay conexión con la API (verifica que esté ejecutándose en la URL configurada).",
                IsError = true
            });
        }

        if (!response.IsSuccessStatusCode)
        {
            var message = await TryReadMessageAsync(response, "No se pudo registrar la cuenta. Verifica tus datos.", cancellationToken);
            return View("Registro", new RegisterPageViewModel
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
    public async Task<IActionResult> SolicitarRecuperacion([Bind(Prefix = "ForgotPassword")] ForgotPasswordViewModel forgotPassword, CancellationToken cancellationToken)
    {
        if (!TryValidateModel(forgotPassword, nameof(ForgotPasswordPageViewModel.ForgotPassword)))
        {
            return View("RecuperarContrasena", new ForgotPasswordPageViewModel
            {
                ForgotPassword = forgotPassword,
                Message = "Ingresa un correo válido para recuperar tu contraseña.",
                IsError = true
            });
        }

        try
        {
            await apiClient.PostAsync("api/auth/forgot-password", forgotPassword, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return View("RecuperarContrasena", new ForgotPasswordPageViewModel
            {
                ForgotPassword = forgotPassword,
                Message = "No hay conexión con la API (verifica que esté ejecutándose en la URL configurada).",
                IsError = true
            });
        }

        TempData["Message"] = "Si el correo existe, te enviamos instrucciones para restablecer la contraseña.";
        TempData["IsError"] = "0";
        return RedirectToAction(nameof(RecuperarContrasena));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestablecerContrasena([Bind(Prefix = "ResetPassword")] ResetPasswordViewModel resetPassword, CancellationToken cancellationToken)
    {
        if (!TryValidateModel(resetPassword, nameof(ResetPasswordPageViewModel.ResetPassword)))
        {
            return View(new ResetPasswordPageViewModel
            {
                ResetPassword = resetPassword,
                Message = "Completa correctamente el formulario para actualizar la contraseña.",
                IsError = true
            });
        }

        HttpResponseMessage response;
        try
        {
            response = await apiClient.PostAsync("api/auth/reset-password", resetPassword, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return View(new ResetPasswordPageViewModel
            {
                ResetPassword = resetPassword,
                Message = "No hay conexión con la API (verifica que esté ejecutándose en la URL configurada).",
                IsError = true
            });
        }

        if (!response.IsSuccessStatusCode)
        {
            var message = await TryReadMessageAsync(response, "No se pudo actualizar la contraseña.", cancellationToken);
            return View(new ResetPasswordPageViewModel
            {
                ResetPassword = resetPassword,
                Message = message,
                IsError = true
            });
        }

        TempData["Message"] = "Contraseña actualizada. Ahora ya puedes iniciar sesión.";
        TempData["IsError"] = "0";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("jwt");
        HttpContext.Session.Remove("rol");
        HttpContext.Session.Remove("idCliente");
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

    private sealed record AuthResponseDto(string Token, DateTime Expiration, string Rol, int? IdCliente);
    private sealed record ErrorDto(string? Message);
}
