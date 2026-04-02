using MercadoPago.Client.Common;
using MercadoPago.Client.Payment;
using MercadoPago.Config;
using MercadoPago.Resource.Payment;
using Microsoft.Extensions.Options;
using SistemaRepuestosMaquinas.API.Configuration;

namespace SistemaRepuestosMaquinas.API.Services;

public class MercadoPagoService(IOptions<MercadoPagoOptions> options) : IMercadoPagoService
{
    public async Task<MercadoPagoChargeResult> CreatePaymentAsync(MercadoPagoChargeRequest request, CancellationToken cancellationToken = default)
    {
        var config = options.Value;
        if (string.IsNullOrWhiteSpace(config.AccessToken))
        {
            return new MercadoPagoChargeResult(false, "Mercado Pago no está configurado (AccessToken).", null, null, null);
        }

        var amount = Math.Round(request.Total, 2, MidpointRounding.AwayFromZero);
        if (amount <= 0)
        {
            return new MercadoPagoChargeResult(false, "El monto del pago es inválido.", null, null, null);
        }

        MercadoPagoConfig.AccessToken = config.AccessToken;

        var paymentRequest = new PaymentCreateRequest
        {
            TransactionAmount = amount,
            Token = request.Token,
            Description = $"Pedido cliente #{request.IdCliente}",
            Installments = request.Installments,
            PaymentMethodId = request.PaymentMethodId,
            IssuerId = request.IssuerId,
            Payer = new PaymentPayerRequest
            {
                Email = request.Email,
                Identification = string.IsNullOrWhiteSpace(request.IdentificationType) || string.IsNullOrWhiteSpace(request.IdentificationNumber)
                    ? null
                    : new IdentificationRequest
                    {
                        Type = request.IdentificationType,
                        Number = request.IdentificationNumber
                    }
            },
            Metadata = new Dictionary<string, object>
            {
                ["cliente_id"] = request.IdCliente,
                ["integracion"] = "mercado_pago"
            }
        };

        if (request.Metadata is not null)
        {
            foreach (var entry in request.Metadata)
            {
                paymentRequest.Metadata[entry.Key] = entry.Value;
            }
        }

        try
        {
            var client = new PaymentClient();
            Payment payment = await client.CreateAsync(paymentRequest, cancellationToken: cancellationToken);

            var isApproved = string.Equals(payment.Status, "approved", StringComparison.OrdinalIgnoreCase);
            var message = isApproved
                ? "Pago aprobado con Mercado Pago."
                : $"Mercado Pago devolvió el estado '{payment.Status}' ({payment.StatusDetail}).";

            return new MercadoPagoChargeResult(
                isApproved,
                message,
                payment.Id.ToString(),
                payment.Status,
                payment.StatusDetail);
        }
        catch (Exception ex)
        {
            return new MercadoPagoChargeResult(false, $"Error al procesar el pago con Mercado Pago: {ex.Message}", null, null, null);
        }
    }
}
