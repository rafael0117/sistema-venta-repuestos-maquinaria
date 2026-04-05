namespace SistemaRepuestosMaquinas.Web.Models;

public class CarritoPageViewModel
{
    public int? IdCliente { get; set; }
    public string? DireccionEntrega { get; set; }
    public string? EmailPago { get; set; }
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

public class ConfirmacionPagoViewModel
{
    public string EstadoPedido { get; set; } = "pending";
    public string? PaymentId { get; set; }
    public string? PreferenceId { get; set; }
    public bool IsApproved { get; set; }
    public bool PedidoRegistrado { get; set; }
    public int? IdPedido { get; set; }
    public string? Message { get; set; }
}
