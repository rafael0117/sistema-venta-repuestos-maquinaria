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

        var payload = new
        {
            items = request.Items.Select(x => new
            {
                title = x.Title,
                quantity = x.Quantity,
                currency_id = "PEN",
                unit_price = Math.Round(x.UnitPrice, 2, MidpointRounding.AwayFromZero)
            }),
            payer = new { email = request.Email },
            back_urls = new
            {
                success = request.SuccessUrl,
                failure = request.FailureUrl,
                pending = request.PendingUrl
            },
            auto_return = "approved",
            external_reference = $"cliente-{request.IdCliente}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
            metadata = new
            {
                cliente_id = request.IdCliente,
                integracion = "checkout_pro"
            }
        };

        using var client = new HttpClient
        {
            BaseAddress = new Uri("https://api.mercadopago.com/")
        };

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.AccessToken);

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
}
