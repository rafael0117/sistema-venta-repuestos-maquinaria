using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using SistemaRepuestosMaquinas.API.Configuration;

namespace SistemaRepuestosMaquinas.API.Services;

public class CulqiService(HttpClient httpClient, IOptions<CulqiOptions> options) : ICulqiService
{
    public async Task<CulqiChargeResult> CreateChargeAsync(CulqiChargeRequest request, CancellationToken cancellationToken = default)
    {
        var config = options.Value;
        if (string.IsNullOrWhiteSpace(config.SecretKey))
        {
            return new CulqiChargeResult(false, "Culqi no está configurado (SecretKey).", null);
        }

        httpClient.BaseAddress = new Uri(config.BaseUrl);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.SecretKey);

        var amount = (int)Math.Round(request.Total * 100m, MidpointRounding.AwayFromZero);
        if (amount <= 0)
        {
            return new CulqiChargeResult(false, "El monto del pago es inválido.", null);
        }

        var payload = new
        {
            amount,
            currency_code = "PEN",
            email = request.Email,
            source_id = request.SourceId,
            capture = true,
            description = $"Pedido cliente #{request.IdCliente}",
            metadata = new Dictionary<string, string>
            {
                ["cliente_id"] = request.IdCliente.ToString()
            }
        };

        try
        {
            var response = await httpClient.PostAsJsonAsync("charges", payload, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                return new CulqiChargeResult(false, $"Culqi rechazó el pago: {body}", null);
            }

            var data = await response.Content.ReadFromJsonAsync<CulqiChargeResponse>(cancellationToken: cancellationToken);
            return new CulqiChargeResult(true, "Pago aprobado con Culqi.", data?.Id);
        }
        catch (Exception ex)
        {
            return new CulqiChargeResult(false, $"Error conectando con Culqi: {ex.Message}", null);
        }
    }

    private sealed class CulqiChargeResponse
    {
        public string? Id { get; set; }
    }
}
