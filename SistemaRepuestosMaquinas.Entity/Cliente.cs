namespace SistemaRepuestosMaquinas.Entity;

public class Cliente : BaseEntity
{
    public int IdCliente { get; set; }
    public int IdUsuario { get; set; }
    public string TipoDocumento { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;

    public Usuario? Usuario { get; set; }
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
