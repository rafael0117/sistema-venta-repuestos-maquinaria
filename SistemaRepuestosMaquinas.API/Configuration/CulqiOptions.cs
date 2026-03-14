namespace SistemaRepuestosMaquinas.API.Configuration;

public class CulqiOptions
{
    public const string SectionName = "Culqi";
    public string BaseUrl { get; set; } = "https://api.culqi.com/v2/";
    public string PublicKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
}
