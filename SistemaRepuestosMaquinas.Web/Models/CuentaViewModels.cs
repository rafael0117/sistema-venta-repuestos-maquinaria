using System.ComponentModel.DataAnnotations;

namespace SistemaRepuestosMaquinas.Web.Models;

public class CuentaPageViewModel
{
    public LoginViewModel Login { get; set; } = new();
    public RegisterViewModel Register { get; set; } = new();
    public string? Message { get; set; }
    public bool IsError { get; set; }
}

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string Correo { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

public class RegisterViewModel
{
    [Required]
    public string Nombres { get; set; } = string.Empty;

    [Required]
    public string Apellidos { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Correo { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
