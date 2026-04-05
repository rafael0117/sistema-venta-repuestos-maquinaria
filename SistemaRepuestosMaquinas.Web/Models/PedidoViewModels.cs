namespace SistemaRepuestosMaquinas.Web.Models;

public class PedidoPageViewModel
{
    public string? Message { get; set; }
    public bool IsError { get; set; }
    public List<PedidoTimelineItemViewModel> Pedidos { get; set; } = [];
}

public class PedidoTimelineItemViewModel
{
    public int IdPedido { get; set; }
    public DateTime FechaPedido { get; set; }
    public decimal Total { get; set; }
    public string EstadoPedido { get; set; } = "Pendiente";
    public string DireccionEntrega { get; set; } = string.Empty;
    public string MetodoPago { get; set; } = string.Empty;
    public List<PedidoDetalleItemViewModel> Detalles { get; set; } = [];
}

public class PedidoDetalleItemViewModel
{
    public int IdProducto { get; set; }
    public string Producto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal SubTotal { get; set; }
}
