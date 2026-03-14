namespace SistemaRepuestosMaquinas.Entity;

public class DetalleCompra
{
    public int IdDetalleCompra { get; set; }
    public int IdCompra { get; set; }
    public int IdProducto { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioCompra { get; set; }
    public decimal SubTotal { get; set; }

    public Compra? Compra { get; set; }
    public Producto? Producto { get; set; }
}
