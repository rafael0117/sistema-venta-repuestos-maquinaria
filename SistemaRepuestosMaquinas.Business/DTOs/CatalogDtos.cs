using System.ComponentModel.DataAnnotations;

namespace SistemaRepuestosMaquinas.Business.DTOs;

public class ProductoFiltroRequest
{
    public string? Texto { get; set; }
    public int? IdCategoria { get; set; }
    public int? IdMarca { get; set; }
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;
    [Range(1, 100)]
    public int PageSize { get; set; } = 12;
}

public class CheckoutRequest
{
    [Range(1, int.MaxValue)]
    public int IdCliente { get; set; }
    [Required]
    [MaxLength(300)]
    public string DireccionEntrega { get; set; } = string.Empty;
    [Required]
    [MaxLength(50)]
    public string MetodoPago { get; set; } = string.Empty;
    [EmailAddress]
    [MaxLength(150)]
    public string? EmailPago { get; set; }
}

public class ConfirmCheckoutProRequest
{
    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "pending";
    [MaxLength(80)]
    public string? PaymentId { get; set; }
    [MaxLength(120)]
    public string? PreferenceId { get; set; }
    [Required]
    [MaxLength(300)]
    public string DireccionEntrega { get; set; } = "Sin dirección";
}
