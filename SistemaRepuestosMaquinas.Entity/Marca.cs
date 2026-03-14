namespace SistemaRepuestosMaquinas.Entity;

public class Marca : BaseEntity
{
    public int IdMarca { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
