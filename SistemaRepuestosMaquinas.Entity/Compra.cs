namespace SistemaRepuestosMaquinas.Entity;

public class Compra
{
    public int IdCompra { get; set; }
    public int IdProveedor { get; set; }
    public DateTime FechaCompra { get; set; } = DateTime.UtcNow;
    public decimal Total { get; set; }

    public Proveedor? Proveedor { get; set; }
    public ICollection<DetalleCompra> Detalles { get; set; } = new List<DetalleCompra>();
}
