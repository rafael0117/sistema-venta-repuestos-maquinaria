namespace SistemaRepuestosMaquinas.Entity;

public class Rol : BaseEntity
{
    public int IdRol { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
