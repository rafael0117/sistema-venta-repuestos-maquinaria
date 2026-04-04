using Microsoft.AspNetCore.Mvc;
using SistemaRepuestosMaquinas.Web.Models;
using SistemaRepuestosMaquinas.Web.Services;

namespace SistemaRepuestosMaquinas.Web.Controllers;

public class AdminCompraController(ApiClient apiClient) : AdminBaseController(apiClient)
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (!TryAuthorizeAdmin(out var unauthorized)) return unauthorized!;

        var vm = new AdminCompraPageViewModel
        {
            Message = TempData["Message"] as string,
            IsError = (TempData["IsError"] as string) == "1",
            Items = await ApiClient.GetAsync<List<AdminCompraItem>>("api/compra", cancellationToken) ?? []
        };

        return View(vm);
    }
}
