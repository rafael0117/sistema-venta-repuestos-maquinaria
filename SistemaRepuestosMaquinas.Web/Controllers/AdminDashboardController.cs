using Microsoft.AspNetCore.Mvc;
using SistemaRepuestosMaquinas.Web.Models;
using SistemaRepuestosMaquinas.Web.Services;

namespace SistemaRepuestosMaquinas.Web.Controllers;

public class AdminDashboardController(ApiClient apiClient) : AdminBaseController(apiClient)
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (!TryAuthorizeAdminOrVendedor(out var unauthorized)) return unauthorized!;

        var vm = new AdminDashboardViewModel
        {
            Message = TempData["Message"] as string,
            IsError = (TempData["IsError"] as string) == "1"
        };

        try
        {
            var resumen = await ApiClient.GetAsync<ResumenDto>("api/reporte/resumen", cancellationToken);
            if (resumen is not null)
            {
                vm.TotalProductos = resumen.TotalProductos;
                vm.TotalClientes = resumen.TotalClientes;
                vm.TotalPedidos = resumen.TotalPedidos;
                vm.VentasMes = resumen.VentasMes;
                vm.ComprasMes = resumen.ComprasMes;
                vm.UtilidadBrutaMes = resumen.UtilidadBrutaMes;
            }
        }
        catch
        {
            vm.Message ??= "No se pudo cargar el resumen del dashboard.";
            vm.IsError = true;
        }

        return View(vm);
    }

    private sealed class ResumenDto
    {
        public int TotalProductos { get; set; }
        public int TotalClientes { get; set; }
        public int TotalPedidos { get; set; }
        public decimal VentasMes { get; set; }
        public decimal ComprasMes { get; set; }
        public decimal UtilidadBrutaMes { get; set; }
    }
}
