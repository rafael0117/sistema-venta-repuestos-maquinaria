namespace SistemaRepuestosMaquinas.Business.Interfaces;

public interface ICheckoutService
{
    Task<CheckoutConfirmationResult> ConfirmCheckoutProAsync(
        int idCliente,
        string status,
        string? paymentId,
        string? preferenceId,
        string direccionEntrega,
        CancellationToken cancellationToken = default);
}

public record CheckoutConfirmationResult(
    bool IsSuccess,
    string Message,
    int? IdPedido = null,
    decimal TotalPedido = 0m);
