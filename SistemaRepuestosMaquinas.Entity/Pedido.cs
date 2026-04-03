namespace SistemaRepuestosMaquinas.Entity;

public class Pedido
{
    public int IdPedido { get; set; }
    public int IdCliente { get; set; }
    public DateTime FechaPedido { get; set; } = DateTime.UtcNow;
    public decimal Total { get; set; }
    public string EstadoPedido { get; set; } = "Pendiente";
    public string DireccionEntrega { get; set; } = string.Empty;
    public string MetodoPago { get; set; } = string.Empty;
    public string EstadoPago { get; set; } = "pending";
    public string? ExternalPaymentId { get; set; }
    public string? ExternalPreferenceId { get; set; }

    public Cliente? Cliente { get; set; }
    public ICollection<PedidoDetalle> Detalles { get; set; } = new List<PedidoDetalle>();
}
