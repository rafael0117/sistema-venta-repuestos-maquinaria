using System.Collections.Generic;

namespace SistemaRepuestosMaquinas.API.Services;

public interface IMercadoPagoService
{
    Task<MercadoPagoChargeResult> CreatePaymentAsync(MercadoPagoChargeRequest request, CancellationToken cancellationToken = default);
}

public record MercadoPagoChargeRequest(
    decimal Total,
    string Token,
    string PaymentMethodId,
    int Installments,
    string Email,
    int IdCliente,
    string? IssuerId = null,
    string? IdentificationType = null,
    string? IdentificationNumber = null,
    Dictionary<string, string>? Metadata = null);

public record MercadoPagoChargeResult(bool IsSuccess, string Message, string? PaymentId, string? Status, string? StatusDetail);
