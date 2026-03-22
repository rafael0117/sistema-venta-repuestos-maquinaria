namespace SistemaRepuestosMaquinas.Web.Models;

public class CarritoPageViewModel
{
    public int? IdCliente { get; set; }
    public int? IdProducto { get; set; }
    public int Cantidad { get; set; } = 1;
    public string? DireccionEntrega { get; set; }
    public string? MetodoPago { get; set; } = "MERCADO_PAGO";
    public string? MercadoPagoToken { get; set; }
    public string? PaymentMethodId { get; set; }
    public int? Installments { get; set; } = 1;
    public string? IssuerId { get; set; }
    public string? EmailPago { get; set; }
    public string? IdentificationType { get; set; } = "DNI";
    public string? IdentificationNumber { get; set; }
    public decimal Total { get; set; }
    public string? Message { get; set; }
    public bool IsError { get; set; }
    public List<CarritoItemViewModel> Detalles { get; set; } = [];
}

public class CarritoItemViewModel
{
    public int IdCarritoDetalle { get; set; }
    public int IdProducto { get; set; }
    public string? Producto { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal SubTotal { get; set; }
}
