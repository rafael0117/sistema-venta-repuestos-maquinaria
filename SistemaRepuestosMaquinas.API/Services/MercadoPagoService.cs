using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SistemaRepuestosMaquinas.API.Configuration;

namespace SistemaRepuestosMaquinas.API.Services;

public class MercadoPagoService(IOptions<MercadoPagoOptions> options) : IMercadoPagoService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<MercadoPagoPreferenceResult> CreateCheckoutProPreferenceAsync(MercadoPagoPreferenceRequest request, CancellationToken cancellationToken = default)
    {
        var config = options.Value;
        if (string.IsNullOrWhiteSpace(config.AccessToken))
            return new MercadoPagoPreferenceResult(false, "Mercado Pago no está configurado (AccessToken).", null, null);

        if (request.Items.Count == 0)
            return new MercadoPagoPreferenceResult(false, "No hay ítems para generar la preferencia de pago.", null, null);

        var payload = new Dictionary<string, object?>
        {
            ["items"] = request.Items.Select(x => new
            {
                title = x.Title,
                quantity = x.Quantity,
                currency_id = "PEN",
                unit_price = Math.Round(x.UnitPrice, 2, MidpointRounding.AwayFromZero)
            }),
            ["payer"] = new { email = request.Email },
            ["back_urls"] = new
            {
                success = request.SuccessUrl,
                failure = request.FailureUrl,
                pending = request.PendingUrl
            },
            ["auto_return"] = "approved",
            ["external_reference"] = $"cliente-{request.IdCliente}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
            ["metadata"] = new
            {
                cliente_id = request.IdCliente,
                integracion = "checkout_pro"
            }
        };

        if (Uri.TryCreate(config.WebhookNotificationUrl, UriKind.Absolute, out var webhookUri) &&
            (webhookUri.Scheme == Uri.UriSchemeHttps || webhookUri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)) &&
            !webhookUri.Host.Contains("TU-DOMINIO", StringComparison.OrdinalIgnoreCase))
        {
            payload["notification_url"] = webhookUri.ToString();
        }

        using var client = BuildHttpClient(config.AccessToken);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "checkout/preferences")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.Add("x-idempotency-key", Guid.NewGuid().ToString());

        using var response = await client.SendAsync(httpRequest, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            return new MercadoPagoPreferenceResult(false, $"Mercado Pago rechazó la preferencia: {content}", null, null);

        try
        {
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;
            var preferenceId = root.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
            var initPoint = root.TryGetProperty("init_point", out var initProp) ? initProp.GetString() : null;
            var sandboxPoint = root.TryGetProperty("sandbox_init_point", out var sandProp) ? sandProp.GetString() : null;

            var redirect = string.IsNullOrWhiteSpace(sandboxPoint) ? initPoint : sandboxPoint;
            if (string.IsNullOrWhiteSpace(redirect))
                return new MercadoPagoPreferenceResult(false, "Mercado Pago no devolvió una URL de redirección.", preferenceId, null);

            return new MercadoPagoPreferenceResult(true, "Preferencia Checkout Pro creada correctamente.", preferenceId, redirect);
        }
        catch (JsonException)
        {
            return new MercadoPagoPreferenceResult(false, "Respuesta inválida al crear la preferencia de Mercado Pago.", null, null);
        }
    }

    public async Task<MercadoPagoPaymentLookupResult> GetPaymentByIdAsync(string paymentId, CancellationToken cancellationToken = default)
    {
        var config = options.Value;
        if (string.IsNullOrWhiteSpace(config.AccessToken))
            return new MercadoPagoPaymentLookupResult(false, "Mercado Pago no está configurado (AccessToken).", null, null, null, null, 0m);

        if (string.IsNullOrWhiteSpace(paymentId))
            return new MercadoPagoPaymentLookupResult(false, "PaymentId inválido.", null, null, null, null, 0m);

        using var client = BuildHttpClient(config.AccessToken);
        using var response = await client.GetAsync($"v1/payments/{paymentId}", cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            return new MercadoPagoPaymentLookupResult(false, $"No se pudo consultar pago en Mercado Pago: {content}", paymentId, null, null, null, 0m);

        try
        {
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            var status = root.TryGetProperty("status", out var statusProp) ? statusProp.GetString() : null;
            var externalReference = root.TryGetProperty("external_reference", out var externalProp) ? externalProp.GetString() : null;
            var amount = root.TryGetProperty("transaction_amount", out var amountProp) && amountProp.TryGetDecimal(out var amountValue)
                ? amountValue
                : 0m;

            string? payerEmail = null;
            if (root.TryGetProperty("payer", out var payerProp) && payerProp.TryGetProperty("email", out var emailProp))
                payerEmail = emailProp.GetString();

            return new MercadoPagoPaymentLookupResult(true, "Pago consultado correctamente.", paymentId, status, externalReference, payerEmail, amount);
        }
        catch (JsonException)
        {
            return new MercadoPagoPaymentLookupResult(false, "Respuesta inválida consultando pago en Mercado Pago.", paymentId, null, null, null, 0m);
        }
    }

    private static HttpClient BuildHttpClient(string accessToken)
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri("https://api.mercadopago.com/")
        };

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }
}
