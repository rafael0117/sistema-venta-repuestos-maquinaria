namespace SistemaRepuestosMaquinas.API.Configuration;

public class MercadoPagoOptions
{
    public const string SectionName = "MercadoPago";
    public string AccessToken { get; set; } = string.Empty;
}
