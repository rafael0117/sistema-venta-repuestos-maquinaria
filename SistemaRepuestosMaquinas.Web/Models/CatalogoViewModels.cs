namespace SistemaRepuestosMaquinas.Web.Models;

public class CatalogoPageViewModel
{
    public string? Texto { get; set; }
    public int? IdCategoria { get; set; }
    public int? IdMarca { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
    public int Total { get; set; }
    public int TotalPages => PageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(Total / (double)PageSize));
    public List<CatalogOptionItem> Categorias { get; set; } = [];
    public List<CatalogOptionItem> Marcas { get; set; } = [];
    public List<ProductoCatalogoItemViewModel> Productos { get; set; } = [];
}

public class CatalogOptionItem
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class ProductoCatalogoItemViewModel
{
    public int IdProducto { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal PrecioVenta { get; set; }
    public int Stock { get; set; }
    public string? Categoria { get; set; }
    public string? Marca { get; set; }
    public string? ImagenUrl { get; set; }
}

public class ProductoDetalleViewModel
{
    public int IdProducto { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal PrecioVenta { get; set; }
    public int Stock { get; set; }
    public string? Categoria { get; set; }
    public string? Marca { get; set; }
    public string? ImagenUrl { get; set; }
    public int Cantidad { get; set; } = 1;
}
