namespace SistemaRepuestosMaquinas.API.Configuration;

public class PasswordRecoveryOptions
{
    public const string SectionName = "PasswordRecovery";

    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUser { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FrontendResetUrl { get; set; } = string.Empty;
    public bool UseSsl { get; set; } = true;
}
