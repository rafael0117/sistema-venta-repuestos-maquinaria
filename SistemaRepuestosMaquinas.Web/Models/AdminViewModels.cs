namespace SistemaRepuestosMaquinas.Web.Models;

public class AdminDashboardViewModel
{
    public string? Message { get; set; }
    public bool IsError { get; set; }
    public int TotalProductos { get; set; }
    public int TotalClientes { get; set; }
    public int TotalPedidos { get; set; }
    public decimal VentasMes { get; set; }
    public decimal ComprasMes { get; set; }
    public decimal UtilidadBrutaMes { get; set; }
}

public class AdminProductoPageViewModel
{
    public string? Message { get; set; }
    public bool IsError { get; set; }
    public List<AdminProductoItem> Items { get; set; } = [];
    public AdminProductoItem Form { get; set; } = new();
}

public class AdminProductoItem
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
    public bool Estado { get; set; } = true;
}

public class AdminCategoriaPageViewModel
{
    public string? Message { get; set; }
    public bool IsError { get; set; }
    public List<AdminCategoriaItem> Items { get; set; } = [];
    public AdminCategoriaItem Form { get; set; } = new();
}

public class AdminCategoriaItem
{
    public int IdCategoria { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Estado { get; set; } = true;
}

public class AdminMarcaPageViewModel
{
    public string? Message { get; set; }
    public bool IsError { get; set; }
    public List<AdminMarcaItem> Items { get; set; } = [];
    public AdminMarcaItem Form { get; set; } = new();
}

public class AdminMarcaItem
{
    public int IdMarca { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Estado { get; set; } = true;
}

public class AdminProveedorPageViewModel
{
    public string? Message { get; set; }
    public bool IsError { get; set; }
    public List<AdminProveedorItem> Items { get; set; } = [];
    public AdminProveedorItem Form { get; set; } = new();
}

public class AdminProveedorItem
{
    public int IdProveedor { get; set; }
    public string Ruc { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public bool Estado { get; set; } = true;
}

public class AdminClientePageViewModel
{
    public string? Message { get; set; }
    public bool IsError { get; set; }
    public List<AdminClienteItem> Items { get; set; } = [];
}

public class AdminClienteItem
{
    public int IdCliente { get; set; }
    public int IdUsuario { get; set; }
    public string TipoDocumento { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public bool Estado { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
}

public class AdminPedidoPageViewModel
{
    public string? Message { get; set; }
    public bool IsError { get; set; }
    public List<AdminPedidoItem> Items { get; set; } = [];
}

public class AdminPedidoItem
{
    public int IdPedido { get; set; }
    public int IdCliente { get; set; }
    public DateTime FechaPedido { get; set; }
    public decimal Total { get; set; }
    public string EstadoPedido { get; set; } = string.Empty;
    public string DireccionEntrega { get; set; } = string.Empty;
    public string MetodoPago { get; set; } = string.Empty;
}

public class AdminCompraPageViewModel
{
    public string? Message { get; set; }
    public bool IsError { get; set; }
    public List<AdminCompraItem> Items { get; set; } = [];
}

public class AdminCompraItem
{
    public int IdCompra { get; set; }
    public int IdProveedor { get; set; }
    public DateTime FechaCompra { get; set; }
    public decimal Total { get; set; }
}

public class AdminVentaPageViewModel
{
    public string? Message { get; set; }
    public bool IsError { get; set; }
    public List<AdminVentaItem> Items { get; set; } = [];
}

public class AdminVentaItem
{
    public int IdVenta { get; set; }
    public int IdCliente { get; set; }
    public DateTime FechaVenta { get; set; }
    public decimal Total { get; set; }
}


public class AdminUsuarioPageViewModel
{
    public string? Message { get; set; }
    public bool IsError { get; set; }
    public List<AdminUsuarioItem> Items { get; set; } = [];
    public AdminUsuarioCreateItem Form { get; set; } = new();
    public List<AdminRolItem> Roles { get; set; } = [];
}

public class AdminUsuarioItem
{
    public int IdUsuario { get; set; }
    public int IdRol { get; set; }
    public string Rol { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public bool Estado { get; set; } = true;
    public string? NewPassword { get; set; }
}

public class AdminUsuarioCreateItem
{
    public int IdRol { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool Estado { get; set; } = true;
}

public class AdminRolItem
{
    public int IdRol { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
