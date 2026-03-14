namespace SistemaRepuestosMaquinas.Entity;

public class PedidoDetalle
{
    public int IdPedidoDetalle { get; set; }
    public int IdPedido { get; set; }
    public int IdProducto { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal SubTotal { get; set; }

    public Pedido? Pedido { get; set; }
    public Producto? Producto { get; set; }
}
