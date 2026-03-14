namespace SistemaRepuestosMaquinas.Entity;

public class Venta
{
    public int IdVenta { get; set; }
    public int IdCliente { get; set; }
    public DateTime FechaVenta { get; set; } = DateTime.UtcNow;
    public decimal Total { get; set; }
    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
}
