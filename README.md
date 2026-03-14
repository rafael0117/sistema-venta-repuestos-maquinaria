# SistemaRepuestosMaquinas

Sistema web de comercio electrónico y gestión administrativa para venta de repuestos de maquinaria, con API JWT + Web MVC Razor.

## Arquitectura propuesta

- `SistemaRepuestosMaquinas.API`: ASP.NET Core Web API con JWT, autorización por roles y endpoints protegidos.
- `SistemaRepuestosMaquinas.Web`: ASP.NET Core MVC + Razor para tienda pública y panel administrativo.
- `SistemaRepuestosMaquinas.Business`: reglas de negocio, autenticación, carrito, pedidos y reportes.
- `SistemaRepuestosMaquinas.Data`: `ApplicationDbContext`, repositorios y configuración EF Core.
- `SistemaRepuestosMaquinas.Entity`: entidades del dominio.
- `SistemaRepuestosMaquinas.Common`: constantes y utilidades compartidas.

## Roles

- **Administrador**: acceso total.
- **Vendedor**: ventas, clientes, productos.
- **Cliente**: catálogo, carrito, pedidos e historial.

## Módulos

### Público (cliente web)

- Inicio
- Catálogo y búsqueda
- Filtros por categoría/marca
- Registro/login
- Carrito
- Checkout
- Mis pedidos

### Privado (admin/vendedor)

- Dashboard
- Productos
- Categorías
- Marcas
- Clientes
- Proveedores
- Compras
- Ventas
- Reportes
- Gestión de usuarios

## Flujo JWT

1. Registro de cliente en la web.
2. API guarda usuario + cliente con rol Cliente.
3. Login valida credenciales.
4. API emite JWT.
5. Web consume endpoints protegidos con `Bearer token`.

## Flujo de compra web

1. Cliente inicia sesión.
2. Agrega productos al carrito.
3. Confirma checkout.
4. API valida stock.
5. Se genera pedido y detalles.
6. Se descuenta stock.
7. Cliente visualiza historial.

## Requisitos técnicos

- .NET 8 (recomendado para estabilidad de tesis).
- ASP.NET Core Web API + JWT.
- ASP.NET Core MVC + Razor.
- Entity Framework Core.
- SQL Server.

## Próximos pasos de implementación

1. Implementar hash real de contraseñas y generación de JWT firmados.
2. Completar CRUD y casos de uso por módulo.
3. Crear migraciones y seed de roles/usuario admin.
4. Integrar checkout con validación transaccional de stock.
5. Implementar reportes y dashboard con KPIs.
