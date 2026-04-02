using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using SistemaRepuestosMaquinas.Web.Models;
using SistemaRepuestosMaquinas.Web.Services;

namespace SistemaRepuestosMaquinas.Web.Controllers;

public class CarritoController(ApiClient apiClient) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(int? idCliente, CancellationToken cancellationToken)
    {
        var vm = new CarritoPageViewModel
        {
            IdCliente = idCliente ?? TryGetSavedClienteId(),
            Message = TempData["Message"] as string,
            IsError = (TempData["IsError"] as string) == "1"
        };

        if (vm.IdCliente.HasValue)
        {
            await LoadCarritoAsync(vm, vm.IdCliente.Value, cancellationToken);
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SeleccionarCliente(int idCliente)
    {
        SaveClienteId(idCliente);
        return RedirectToAction(nameof(Index), new { idCliente });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Agregar(CarritoPageViewModel model, CancellationToken cancellationToken)
    {
        if (!model.IdCliente.HasValue)
            return RedirectWithMessage("Debes indicar el IdCliente.", true);

        if (!model.IdProducto.HasValue || model.Cantidad <= 0)
            return RedirectWithMessage("Datos inválidos para agregar producto.", true, model.IdCliente);

        var token = HttpContext.Session.GetString("jwt");
        apiClient.AttachJwt(token);

        var response = await apiClient.PostAsync($"api/carrito/cliente/{model.IdCliente.Value}/items", new { model.IdProducto, model.Cantidad }, cancellationToken);
        return response.IsSuccessStatusCode
            ? RedirectWithMessage("Producto agregado al carrito.", false, model.IdCliente)
            : RedirectWithMessage("No se pudo agregar al carrito (verifica sesión, producto y stock).", true, model.IdCliente);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int idCarritoDetalle, int idCliente, CancellationToken cancellationToken)
    {
        var token = HttpContext.Session.GetString("jwt");
        apiClient.AttachJwt(token);

        var response = await apiClient.DeleteAsync($"api/carrito/items/{idCarritoDetalle}", cancellationToken);
        return response.IsSuccessStatusCode
            ? RedirectWithMessage("Ítem eliminado.", false, idCliente)
            : RedirectWithMessage("No se pudo eliminar el ítem.", true, idCliente);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CarritoPageViewModel model, CancellationToken cancellationToken)
    {
        if (!model.IdCliente.HasValue || string.IsNullOrWhiteSpace(model.DireccionEntrega) || string.IsNullOrWhiteSpace(model.MetodoPago))
            return RedirectWithMessage("Completa IdCliente, dirección y método de pago.", true, model.IdCliente);

        if (model.MetodoPago.Equals("MERCADO_PAGO", StringComparison.OrdinalIgnoreCase) &&
            (string.IsNullOrWhiteSpace(model.MercadoPagoToken) ||
             string.IsNullOrWhiteSpace(model.PaymentMethodId) ||
             !model.Installments.HasValue || model.Installments.Value <= 0 ||
             string.IsNullOrWhiteSpace(model.EmailPago)))
        {
            return RedirectWithMessage("Para Mercado Pago debes ingresar EmailPago, MercadoPagoToken, PaymentMethodId e Installments válidos.", true, model.IdCliente);
        }

        var token = HttpContext.Session.GetString("jwt");
        apiClient.AttachJwt(token);

        var response = await apiClient.PostAsync($"api/carrito/cliente/{model.IdCliente.Value}/checkout", new
        {
            IdCliente = model.IdCliente.Value,
            model.DireccionEntrega,
            model.MetodoPago,
            model.MercadoPagoToken,
            model.PaymentMethodId,
            model.Installments,
            model.IssuerId,
            model.EmailPago,
            model.IdentificationType,
            model.IdentificationNumber
        }, cancellationToken);

        return response.IsSuccessStatusCode
            ? RedirectWithMessage("Checkout realizado correctamente.", false, model.IdCliente)
            : RedirectWithMessage("No se pudo realizar checkout.", true, model.IdCliente);
    }

    private async Task LoadCarritoAsync(CarritoPageViewModel vm, int idCliente, CancellationToken cancellationToken)
    {
        var token = HttpContext.Session.GetString("jwt");
        apiClient.AttachJwt(token);

        var data = await apiClient.GetAsync<CarritoResponseDto>($"api/carrito/cliente/{idCliente}", cancellationToken);
        if (data is null)
        {
            vm.Message ??= "No se pudo cargar el carrito.";
            vm.IsError = true;
            return;
        }

        vm.Total = data.Total;
        vm.Detalles = data.Detalles?.Select(x => new CarritoItemViewModel
        {
            IdCarritoDetalle = x.IdCarritoDetalle,
            IdProducto = x.IdProducto,
            Producto = x.Producto,
            Cantidad = x.Cantidad,
            PrecioUnitario = x.PrecioUnitario,
            SubTotal = x.SubTotal
        }).ToList() ?? [];
    }

    private IActionResult RedirectWithMessage(string message, bool isError, int? idCliente = null)
    {
        TempData["Message"] = message;
        TempData["IsError"] = isError ? "1" : "0";
        return RedirectToAction(nameof(Index), new { idCliente });
    }

    private int? TryGetSavedClienteId()
    {
        var raw = HttpContext.Session.GetString("idCliente");
        return int.TryParse(raw, out var value) ? value : null;
    }

    private void SaveClienteId(int idCliente) => HttpContext.Session.SetString("idCliente", idCliente.ToString());

    private sealed class CarritoResponseDto
    {
        public decimal Total { get; set; }
        public List<CarritoItemDto>? Detalles { get; set; }
    }

    private sealed class CarritoItemDto
    {
        public int IdCarritoDetalle { get; set; }
        public int IdProducto { get; set; }
        public string? Producto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal SubTotal { get; set; }
    }
}
