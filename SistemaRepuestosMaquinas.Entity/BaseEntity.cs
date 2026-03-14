namespace SistemaRepuestosMaquinas.Entity;

public abstract class BaseEntity
{
    public bool Estado { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}
