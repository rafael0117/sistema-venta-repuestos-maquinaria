using Microsoft.AspNetCore.Mvc;
using SistemaRepuestosMaquinas.Common.Constants;
using SistemaRepuestosMaquinas.Web.Services;

namespace SistemaRepuestosMaquinas.Web.Controllers;

public abstract class AdminBaseController(ApiClient apiClient) : Controller
{
    protected ApiClient ApiClient => apiClient;

    protected bool TryAuthorizeAdmin(out IActionResult? unauthorizedResult)
    {
        var token = HttpContext.Session.GetString("jwt");
        var rol = HttpContext.Session.GetString("rol");

        if (string.IsNullOrWhiteSpace(token))
        {
            unauthorizedResult = RedirectToAction("Index", "Cuenta", new { message = "Debes iniciar sesión." });
            return false;
        }

        if (!string.Equals(rol, Roles.Administrador, StringComparison.OrdinalIgnoreCase))
        {
            unauthorizedResult = RedirectToAction("Index", "Cuenta", new { message = "No tienes permisos de administrador." });
            return false;
        }

        apiClient.AttachJwt(token);
        unauthorizedResult = null;
        return true;
    }

    protected IActionResult RedirectWithMessage(string action, string controller, string message, bool isError = false)
    {
        TempData["Message"] = message;
        TempData["IsError"] = isError ? "1" : "0";
        return RedirectToAction(action, controller);
    }
}
