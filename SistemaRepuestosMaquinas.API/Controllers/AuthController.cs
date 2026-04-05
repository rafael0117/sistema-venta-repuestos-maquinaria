using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SistemaRepuestosMaquinas.API.Configuration;
using SistemaRepuestosMaquinas.API.Services;
using SistemaRepuestosMaquinas.Business.DTOs;
using SistemaRepuestosMaquinas.Business.Interfaces;

namespace SistemaRepuestosMaquinas.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IAuthService authService,
    IOptions<PasswordRecoveryOptions> passwordRecoveryOptions,
    ILogger<AuthController> logger) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await authService.RegisterClienteAsync(request, cancellationToken));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await authService.LoginAsync(request, cancellationToken));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var notification = await authService.RequestPasswordResetAsync(request, cancellationToken);
            if (notification is not null)
            {
                await SendResetEmailAsync(notification, cancellationToken);
            }

            return Ok(new { message = "Si el correo existe, enviaremos un enlace para recuperar la contraseña." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al solicitar recuperación de contraseña.");
            return Ok(new { message = "Si el correo existe, enviaremos un enlace para recuperar la contraseña." });
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await authService.ResetPasswordAsync(request, cancellationToken);
            return Ok(new { message = "Contraseña actualizada correctamente." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private async Task SendResetEmailAsync(PasswordResetNotificationData notification, CancellationToken cancellationToken)
    {
        var options = passwordRecoveryOptions.Value;
        if (!HasSmtpConfiguration(options))
        {
            logger.LogWarning("PasswordRecovery no está configurado. Token generado para {Correo}.", notification.Correo);
            return;
        }

        var resetUrl = $"{options.FrontendResetUrl}?token={notification.Token}";
        var htmlBody = EmailTemplateBuilder.BuildPasswordResetHtml(notification.NombreCompleto, resetUrl);

        using var client = new SmtpClient(options.SmtpHost, options.SmtpPort)
        {
            EnableSsl = options.UseSsl,
            Credentials = new NetworkCredential(options.SmtpUser, options.SmtpPassword)
        };

        using var message = new MailMessage(options.FromEmail, notification.Correo)
        {
            Subject = "Recuperación de contraseña - Sistema de Repuestos",
            Body = htmlBody,
            IsBodyHtml = true
        };

        await client.SendMailAsync(message, cancellationToken);
    }

    private static bool HasSmtpConfiguration(PasswordRecoveryOptions options)
        => !string.IsNullOrWhiteSpace(options.SmtpHost)
           && !string.IsNullOrWhiteSpace(options.SmtpUser)
           && !string.IsNullOrWhiteSpace(options.SmtpPassword)
           && !string.IsNullOrWhiteSpace(options.FromEmail)
           && !string.IsNullOrWhiteSpace(options.FrontendResetUrl);
}
