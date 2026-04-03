using System.ComponentModel.DataAnnotations;

namespace SistemaRepuestosMaquinas.Web.Models;

public class CuentaPageViewModel
{
    public LoginViewModel Login { get; set; } = new();
    public string? Message { get; set; }
    public bool IsError { get; set; }
}

public class RegisterPageViewModel
{
    public RegisterViewModel Register { get; set; } = new();
    public string? Message { get; set; }
    public bool IsError { get; set; }
}

public class ForgotPasswordPageViewModel
{
    public ForgotPasswordViewModel ForgotPassword { get; set; } = new();
    public string? Message { get; set; }
    public bool IsError { get; set; }
}

public class ResetPasswordPageViewModel
{
    public ResetPasswordViewModel ResetPassword { get; set; } = new();
    public string? Message { get; set; }
    public bool IsError { get; set; }
}

public class LoginViewModel
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo válido.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

public class RegisterViewModel
{
    [Required, MinLength(2)]
    public string Nombres { get; set; } = string.Empty;

    [Required, MinLength(2)]
    public string Apellidos { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Correo { get; set; } = string.Empty;

    [Required, Phone, MinLength(9)]
    public string Telefono { get; set; } = string.Empty;

    [Required]
    public string TipoDocumento { get; set; } = "DNI";

    [Required, MinLength(8)]
    public string NumeroDocumento { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Direccion { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$", ErrorMessage = "Mínimo 8 caracteres, con mayúscula, minúscula, número y símbolo.")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "La confirmación no coincide con la contraseña.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ForgotPasswordViewModel
{
    [Required, EmailAddress]
    public string Correo { get; set; } = string.Empty;
}

public class ResetPasswordViewModel
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$", ErrorMessage = "Mínimo 8 caracteres, con mayúscula, minúscula, número y símbolo.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "La confirmación no coincide con la contraseña.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
