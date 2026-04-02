namespace SistemaRepuestosMaquinas.API.Configuration;

public class MercadoPagoOptions
{
    public const string SectionName = "MercadoPago";
    public string PublicKey { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string WebAppBaseUrl { get; set; } = "https://localhost:7002";
}
