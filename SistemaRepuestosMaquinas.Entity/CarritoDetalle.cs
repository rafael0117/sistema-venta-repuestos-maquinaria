namespace SistemaRepuestosMaquinas.Entity;

public class CarritoDetalle
{
    public int IdCarritoDetalle { get; set; }
    public int IdCarrito { get; set; }
    public int IdProducto { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal SubTotal { get; set; }

    public Carrito? Carrito { get; set; }
    public Producto? Producto { get; set; }
}
