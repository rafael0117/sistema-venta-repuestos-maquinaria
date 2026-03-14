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
        modelBuilder.Entity<Rol>().HasKey(x => x.IdRol);
        modelBuilder.Entity<Usuario>().HasKey(x => x.IdUsuario);
        modelBuilder.Entity<Cliente>().HasKey(x => x.IdCliente);
        modelBuilder.Entity<Categoria>().HasKey(x => x.IdCategoria);
        modelBuilder.Entity<Marca>().HasKey(x => x.IdMarca);
        modelBuilder.Entity<Producto>().HasKey(x => x.IdProducto);
        modelBuilder.Entity<Proveedor>().HasKey(x => x.IdProveedor);
        modelBuilder.Entity<Carrito>().HasKey(x => x.IdCarrito);
        modelBuilder.Entity<CarritoDetalle>().HasKey(x => x.IdCarritoDetalle);
        modelBuilder.Entity<Pedido>().HasKey(x => x.IdPedido);
        modelBuilder.Entity<PedidoDetalle>().HasKey(x => x.IdPedidoDetalle);
        modelBuilder.Entity<Compra>().HasKey(x => x.IdCompra);
        modelBuilder.Entity<DetalleCompra>().HasKey(x => x.IdDetalleCompra);
        modelBuilder.Entity<Venta>().HasKey(x => x.IdVenta);
        modelBuilder.Entity<DetalleVenta>().HasKey(x => x.IdDetalleVenta);

        modelBuilder.Entity<Usuario>().HasIndex(x => x.Correo).IsUnique();
        modelBuilder.Entity<Producto>().HasIndex(x => x.Codigo).IsUnique();
        modelBuilder.Entity<Proveedor>().HasIndex(x => x.Ruc).IsUnique();
        modelBuilder.Entity<Cliente>().HasIndex(x => x.IdUsuario).IsUnique();

        modelBuilder.Entity<Usuario>()
            .HasOne(x => x.Rol)
            .WithMany(x => x.Usuarios)
            .HasForeignKey(x => x.IdRol)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Usuario>()
            .HasOne(x => x.Cliente)
            .WithOne(x => x.Usuario)
            .HasForeignKey<Cliente>(x => x.IdUsuario)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Producto>()
            .HasOne(x => x.Categoria)
            .WithMany(x => x.Productos)
            .HasForeignKey(x => x.IdCategoria)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Producto>()
            .HasOne(x => x.Marca)
            .WithMany(x => x.Productos)
            .HasForeignKey(x => x.IdMarca)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Carrito>()
            .HasOne(x => x.Cliente)
            .WithMany()
            .HasForeignKey(x => x.IdCliente)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CarritoDetalle>()
            .HasOne(x => x.Carrito)
            .WithMany(x => x.Detalles)
            .HasForeignKey(x => x.IdCarrito)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CarritoDetalle>()
            .HasOne(x => x.Producto)
            .WithMany()
            .HasForeignKey(x => x.IdProducto)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Pedido>()
            .HasOne(x => x.Cliente)
            .WithMany(x => x.Pedidos)
            .HasForeignKey(x => x.IdCliente)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PedidoDetalle>()
            .HasOne(x => x.Pedido)
            .WithMany(x => x.Detalles)
            .HasForeignKey(x => x.IdPedido)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PedidoDetalle>()
            .HasOne(x => x.Producto)
            .WithMany()
            .HasForeignKey(x => x.IdProducto)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Compra>()
            .HasOne(x => x.Proveedor)
            .WithMany()
            .HasForeignKey(x => x.IdProveedor)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DetalleCompra>()
            .HasOne(x => x.Compra)
            .WithMany(x => x.Detalles)
            .HasForeignKey(x => x.IdCompra)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DetalleCompra>()
            .HasOne(x => x.Producto)
            .WithMany()
            .HasForeignKey(x => x.IdProducto)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Venta>()
            .HasOne<Cliente>()
            .WithMany()
            .HasForeignKey(x => x.IdCliente)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DetalleVenta>()
            .HasOne(x => x.Venta)
            .WithMany(x => x.Detalles)
            .HasForeignKey(x => x.IdVenta)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DetalleVenta>()
            .HasOne(x => x.Producto)
            .WithMany()
            .HasForeignKey(x => x.IdProducto)
            .OnDelete(DeleteBehavior.Restrict);

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