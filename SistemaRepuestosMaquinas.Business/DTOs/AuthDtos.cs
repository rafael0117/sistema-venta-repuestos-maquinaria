namespace SistemaRepuestosMaquinas.Business.DTOs;

public record RegisterRequest(
    string Nombres,
    string Apellidos,
    string Correo,
    string Password,
    string ConfirmPassword,
    string Telefono,
    string TipoDocumento,
    string NumeroDocumento,
    string Direccion);

public record LoginRequest(string Correo, string Password);

public record ForgotPasswordRequest(string Correo);

public record ResetPasswordRequest(string Token, string NewPassword, string ConfirmPassword);

public record PasswordResetNotificationData(string Correo, string NombreCompleto, string Token);

public record AuthResponse(string Token, DateTime Expiration, string Rol, int? IdCliente = null);
