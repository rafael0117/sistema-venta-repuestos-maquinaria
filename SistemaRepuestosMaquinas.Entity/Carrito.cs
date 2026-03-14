namespace SistemaRepuestosMaquinas.Entity;

public class Carrito
{
    public int IdCarrito { get; set; }
    public int IdCliente { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public string Estado { get; set; } = "Abierto";

    public Cliente? Cliente { get; set; }
    public ICollection<CarritoDetalle> Detalles { get; set; } = new List<CarritoDetalle>();
}
