using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using SistemaRepuestosMaquinas.Web.Models;
using SistemaRepuestosMaquinas.Web.Services;

namespace SistemaRepuestosMaquinas.Web.Controllers;

public class CarritoController(ApiClient apiClient) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var vm = new CarritoPageViewModel
        {
            IdCliente = TryGetSavedClienteId(),
            Message = TempData["Message"] as string,
            IsError = (TempData["IsError"] as string) == "1"
        };

        if (!vm.IdCliente.HasValue)
        {
            vm.Message ??= "Inicia sesión para ver tu carrito.";
            vm.IsError = true;
            return View(vm);
        }

        await LoadCarritoAsync(vm, vm.IdCliente.Value, cancellationToken);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AgregarRapido(int idProducto, int cantidad = 1, string? returnUrl = null, CancellationToken cancellationToken = default)
    {
        var idCliente = TryGetSavedClienteId();
        if (!idCliente.HasValue)
            return RedirectWithMessage("Debes iniciar sesión para agregar productos al carrito.", true, returnUrl: returnUrl ?? Url.Action("Index", "Catalogo"));

        if (cantidad <= 0)
            return RedirectWithMessage("La cantidad debe ser mayor a cero.", true, idCliente, returnUrl);

        var token = HttpContext.Session.GetString("jwt");
        apiClient.AttachJwt(token);

        var response = await apiClient.PostAsync($"api/carrito/cliente/{idCliente.Value}/items", new { IdProducto = idProducto, Cantidad = cantidad }, cancellationToken);
        return response.IsSuccessStatusCode
            ? RedirectWithMessage("Producto agregado al carrito.", false, idCliente, returnUrl)
            : RedirectWithMessage("No se pudo agregar al carrito (verifica sesión, producto y stock).", true, idCliente, returnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActualizarCantidad(int idCarritoDetalle, int cantidad, CancellationToken cancellationToken)
    {
        var idCliente = TryGetSavedClienteId();
        if (!idCliente.HasValue)
            return RedirectWithMessage("Inicia sesión para actualizar tu carrito.", true);

        var token = HttpContext.Session.GetString("jwt");
        apiClient.AttachJwt(token);

        var response = await apiClient.PutAsync($"api/carrito/items/{idCarritoDetalle}", new { Cantidad = cantidad }, cancellationToken);
        return response.IsSuccessStatusCode
            ? RedirectWithMessage("Cantidad actualizada.", false, idCliente)
            : RedirectWithMessage("No se pudo actualizar la cantidad.", true, idCliente);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int idCarritoDetalle, CancellationToken cancellationToken)
    {
        var idCliente = TryGetSavedClienteId();
        if (!idCliente.HasValue)
            return RedirectWithMessage("Inicia sesión para modificar tu carrito.", true);

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
        var idCliente = TryGetSavedClienteId();
        if (!idCliente.HasValue || string.IsNullOrWhiteSpace(model.DireccionEntrega) || string.IsNullOrWhiteSpace(model.MetodoPago))
            return RedirectWithMessage("Completa dirección y método de pago, e inicia sesión.", true, idCliente);

        if (model.MetodoPago.Equals("MERCADO_PAGO", StringComparison.OrdinalIgnoreCase) &&
            (string.IsNullOrWhiteSpace(model.MercadoPagoToken) ||
             string.IsNullOrWhiteSpace(model.PaymentMethodId) ||
             !model.Installments.HasValue || model.Installments.Value <= 0 ||
             string.IsNullOrWhiteSpace(model.EmailPago)))
        {
            return RedirectWithMessage("Para Mercado Pago debes ingresar EmailPago, MercadoPagoToken, PaymentMethodId e Installments válidos.", true, idCliente);
        }

        var token = HttpContext.Session.GetString("jwt");
        apiClient.AttachJwt(token);

        var response = await apiClient.PostAsync($"api/carrito/cliente/{idCliente.Value}/checkout", new
        {
            IdCliente = idCliente.Value,
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

        if (!response.IsSuccessStatusCode)
            return RedirectWithMessage("No se pudo realizar checkout.", true, idCliente);

        var checkout = await response.Content.ReadFromJsonAsync<CheckoutResponseDto>(cancellationToken);
        return RedirectToAction(nameof(Confirmacion), new
        {
            idPedido = checkout?.IdPedido ?? 0,
            total = checkout?.Total ?? 0,
            estado = checkout?.EstadoPedido ?? "Procesado"
        });
    }

    [HttpGet]
    public IActionResult Confirmacion(int idPedido, decimal total, string estado)
    {
        var vm = new ConfirmacionPagoViewModel
        {
            IdPedido = idPedido,
            Total = total,
            EstadoPedido = estado
        };

        return View(vm);
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

    private IActionResult RedirectWithMessage(string message, bool isError, int? idCliente = null, string? returnUrl = null)
    {
        TempData["Message"] = message;
        TempData["IsError"] = isError ? "1" : "0";

        if (!string.IsNullOrWhiteSpace(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction(nameof(Index), new { idCliente });
    }

    private int? TryGetSavedClienteId()
    {
        var raw = HttpContext.Session.GetString("idCliente");
        return int.TryParse(raw, out var value) ? value : null;
    }

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

    private sealed class CheckoutResponseDto
    {
        public int IdPedido { get; set; }
        public decimal Total { get; set; }
        public string? EstadoPedido { get; set; }
    }
}
