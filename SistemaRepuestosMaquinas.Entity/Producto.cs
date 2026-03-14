namespace SistemaRepuestosMaquinas.Entity;

public class Producto : BaseEntity
{
    public int IdProducto { get; set; }
    public int IdCategoria { get; set; }
    public int IdMarca { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal PrecioVenta { get; set; }
    public int Stock { get; set; }
    public int StockMinimo { get; set; }
    public string? ImagenUrl { get; set; }

    public Categoria? Categoria { get; set; }
    public Marca? Marca { get; set; }
}
