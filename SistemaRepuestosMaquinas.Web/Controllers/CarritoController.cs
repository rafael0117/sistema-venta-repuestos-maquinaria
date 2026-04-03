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
        if (!idCliente.HasValue || string.IsNullOrWhiteSpace(model.DireccionEntrega))
            return RedirectWithMessage("Completa dirección de entrega e inicia sesión.", true, idCliente);

        var token = HttpContext.Session.GetString("jwt");
        apiClient.AttachJwt(token);

        var response = await apiClient.PostAsync($"api/carrito/cliente/{idCliente.Value}/checkout", new
        {
            IdCliente = idCliente.Value,
            model.DireccionEntrega,
            MetodoPago = "CHECKOUT_PRO",
            model.EmailPago
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return RedirectWithMessage("No se pudo iniciar Checkout Pro.", true, idCliente);

        var checkout = await response.Content.ReadFromJsonAsync<CheckoutProResponseDto>(cancellationToken);
        if (string.IsNullOrWhiteSpace(checkout?.RedirectUrl))
            return RedirectWithMessage("No se recibió URL de pago de Mercado Pago.", true, idCliente);

        return Redirect(checkout.RedirectUrl);
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmacionCheckoutPro(
        string? status,
        string? payment_id,
        string? preference_id,
        string? collection_status,
        CancellationToken cancellationToken)
    {
        var estado = !string.IsNullOrWhiteSpace(status)
            ? status
            : (!string.IsNullOrWhiteSpace(collection_status) ? collection_status : "pending");

        var vm = new ConfirmacionPagoViewModel
        {
            EstadoPedido = estado,
            PaymentId = payment_id,
            PreferenceId = preference_id,
            IsApproved = string.Equals(estado, "approved", StringComparison.OrdinalIgnoreCase)
        };

        if (!vm.IsApproved)
        {
            vm.Message = "Mercado Pago aún no reporta aprobación del pago.";
            vm.IsError = true;
            return View("Confirmacion", vm);
        }

        var idCliente = TryGetSavedClienteId();
        if (!idCliente.HasValue)
        {
            vm.Message = "No se pudo validar tu sesión para confirmar el pedido.";
            vm.IsError = true;
            return View("Confirmacion", vm);
        }

        var token = HttpContext.Session.GetString("jwt");
        apiClient.AttachJwt(token);
        var response = await apiClient.PostAsync($"api/carrito/cliente/{idCliente.Value}/confirmar-checkout-pro", new
        {
            Status = estado,
            PaymentId = payment_id,
            PreferenceId = preference_id,
            DireccionEntrega = "Dirección confirmada en checkout"
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            vm.Message = "El pago fue aprobado, pero no se pudo generar el pedido automáticamente.";
            vm.IsError = true;
            return View("Confirmacion", vm);
        }

        var result = await response.Content.ReadFromJsonAsync<ConfirmCheckoutDto>(cancellationToken);
        vm.IdPedido = result?.IdPedido;
        vm.TotalPedido = result?.TotalPedido ?? 0m;
        vm.Message = result?.Message ?? "Pago confirmado y pedido generado.";
        vm.IsError = false;

        return View("Confirmacion", vm);
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

    private sealed class CheckoutProResponseDto
    {
        public string? RedirectUrl { get; set; }
        public string? PreferenceId { get; set; }
        public string? Message { get; set; }
    }

    private sealed class ConfirmCheckoutDto
    {
        public string? Message { get; set; }
        public int? IdPedido { get; set; }
        public decimal TotalPedido { get; set; }
    }
}
