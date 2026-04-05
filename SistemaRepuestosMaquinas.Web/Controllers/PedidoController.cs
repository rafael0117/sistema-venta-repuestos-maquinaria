using Microsoft.AspNetCore.Mvc;
using SistemaRepuestosMaquinas.Web.Models;
using SistemaRepuestosMaquinas.Web.Services;

namespace SistemaRepuestosMaquinas.Web.Controllers;

public class PedidoController(ApiClient apiClient) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var vm = new PedidoPageViewModel();

        var rawIdCliente = HttpContext.Session.GetString("idCliente");
        if (!int.TryParse(rawIdCliente, out var idCliente))
        {
            vm.Message = "Inicia sesión para ver tus pedidos.";
            vm.IsError = true;
            return View(vm);
        }

        try
        {
            apiClient.AttachJwt(HttpContext.Session.GetString("jwt"));
            var response = await apiClient.GetAsync<List<PedidoDto>>($"api/pedido/cliente/{idCliente}", cancellationToken) ?? [];

            vm.Pedidos = response.Select(x => new PedidoTimelineItemViewModel
            {
                IdPedido = x.IdPedido,
                FechaPedido = x.FechaPedido,
                Total = x.Total,
                EstadoPedido = x.EstadoPedido ?? "Pendiente",
                DireccionEntrega = x.DireccionEntrega ?? "No registrada",
                MetodoPago = x.MetodoPago ?? "No especificado",
                Detalles = x.Detalles?.Select(d => new PedidoDetalleItemViewModel
                {
                    IdProducto = d.IdProducto,
                    Producto = string.IsNullOrWhiteSpace(d.Producto) ? $"Producto {d.IdProducto}" : d.Producto,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    SubTotal = d.SubTotal
                }).ToList() ?? []
            }).ToList();

            if (vm.Pedidos.Count == 0)
                vm.Message = "Aún no tienes pedidos registrados.";
        }
        catch
        {
            vm.Message = "No se pudo cargar el historial de pedidos.";
            vm.IsError = true;
        }

        return View(vm);
    }

    private sealed class PedidoDto
    {
        public int IdPedido { get; set; }
        public DateTime FechaPedido { get; set; }
        public decimal Total { get; set; }
        public string? EstadoPedido { get; set; }
        public string? DireccionEntrega { get; set; }
        public string? MetodoPago { get; set; }
        public List<PedidoDetalleDto>? Detalles { get; set; }
    }

    private sealed class PedidoDetalleDto
    {
        public int IdProducto { get; set; }
        public string? Producto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal SubTotal { get; set; }
    }
}
