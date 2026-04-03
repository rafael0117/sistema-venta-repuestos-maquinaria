using System.ComponentModel.DataAnnotations;

namespace SistemaRepuestosMaquinas.Business.DTOs;

public class RegisterRequest
{
    [Required]
    [MaxLength(100)]
    public string Nombres { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Apellidos { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Correo { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Correo { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    public string Password { get; set; } = string.Empty;
}

public record AuthResponse(string Token, DateTime Expiration, string Rol, int? IdCliente = null);
