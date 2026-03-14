namespace SistemaRepuestosMaquinas.Entity;

public class Usuario : BaseEntity
{
    public int IdUsuario { get; set; }
    public int IdRol { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public Rol? Rol { get; set; }
    public Cliente? Cliente { get; set; }
}
