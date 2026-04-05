namespace SistemaRepuestosMaquinas.API.Services;

public interface IMercadoPagoService
{
    Task<MercadoPagoPreferenceResult> CreateCheckoutProPreferenceAsync(MercadoPagoPreferenceRequest request, CancellationToken cancellationToken = default);
    Task<MercadoPagoPaymentLookupResult> GetPaymentByIdAsync(string paymentId, CancellationToken cancellationToken = default);
}

public record MercadoPagoPreferenceRequest(
    int IdCliente,
    string Email,
    IReadOnlyCollection<MercadoPagoPreferenceItem> Items,
    string SuccessUrl,
    string FailureUrl,
    string PendingUrl);

public record MercadoPagoPreferenceItem(string Title, int Quantity, decimal UnitPrice);

public record MercadoPagoPreferenceResult(bool IsSuccess, string Message, string? PreferenceId, string? RedirectUrl);

public record MercadoPagoPaymentLookupResult(
    bool IsSuccess,
    string Message,
    string? PaymentId,
    string? Status,
    string? ExternalReference,
    string? PayerEmail,
    decimal Amount);
