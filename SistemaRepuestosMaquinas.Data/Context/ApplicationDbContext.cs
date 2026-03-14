using Microsoft.EntityFrameworkCore;
using SistemaRepuestosMaquinas.Entity;

namespace SistemaRepuestosMaquinas.Data.Context;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Marca> Marcas => Set<Marca>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Carrito> Carritos => Set<Carrito>();
    public DbSet<CarritoDetalle> CarritoDetalles => Set<CarritoDetalle>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoDetalle> PedidoDetalles => Set<PedidoDetalle>();
    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<DetalleCompra> DetalleCompras => Set<DetalleCompra>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetalleVentas => Set<DetalleVenta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>().HasIndex(x => x.Correo).IsUnique();
        modelBuilder.Entity<Producto>().HasIndex(x => x.Codigo).IsUnique();
        modelBuilder.Entity<Proveedor>().HasIndex(x => x.Ruc).IsUnique();

        modelBuilder.Entity<CarritoDetalle>()
            .Property(x => x.SubTotal)
            .HasComputedColumnSql("[Cantidad] * [PrecioUnitario]");

        modelBuilder.Entity<PedidoDetalle>()
            .Property(x => x.SubTotal)
            .HasComputedColumnSql("[Cantidad] * [PrecioUnitario]");

        modelBuilder.Entity<DetalleCompra>()
            .Property(x => x.SubTotal)
            .HasComputedColumnSql("[Cantidad] * [PrecioCompra]");

        modelBuilder.Entity<DetalleVenta>()
            .Property(x => x.SubTotal)
            .HasComputedColumnSql("[Cantidad] * [PrecioUnitario]");

        base.OnModelCreating(modelBuilder);
    }
}
