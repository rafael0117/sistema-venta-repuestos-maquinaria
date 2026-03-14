namespace SistemaRepuestosMaquinas.Business.Security;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "SistemaRepuestos";
    public string Audience { get; set; } = "SistemaRepuestos.Web";
    public string SecretKey { get; set; } = "CAMBIAR_EN_PRODUCCION_MINIMO_32_CARACTERES";
    public int ExpirationMinutes { get; set; } = 120;
}
