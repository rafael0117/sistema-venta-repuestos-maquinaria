# Arquitectura completa de tesis

## Título propuesto

**Desarrollo de un sistema web de comercio electrónico y gestión administrativa para la venta de repuestos de maquinaria en el Perú, utilizando ASP.NET Core, SQL Server y autenticación JWT.**

## Capas del sistema

```text
SistemaRepuestosMaquinas.sln
│
├── SistemaRepuestosMaquinas.API
├── SistemaRepuestosMaquinas.Web
├── SistemaRepuestosMaquinas.Business
├── SistemaRepuestosMaquinas.Data
├── SistemaRepuestosMaquinas.Entity
└── SistemaRepuestosMaquinas.Common
```

## Módulos funcionales

### Seguridad

- Registro de cliente.
- Login.
- Emisión/validación JWT.
- Roles: Administrador, Vendedor, Cliente.
- Autorización por perfil.

### E-commerce

- Catálogo de productos.
- Búsqueda y filtros.
- Carrito.
- Checkout.
- Pedidos e historial.

### Administración

- Productos, categorías y marcas.
- Clientes y proveedores.
- Compras y ventas.
- Control de stock.
- Reportes gerenciales.

## Modelo de datos base

### Seguridad

- Rol
- Usuario

### Negocio

- Cliente
- Categoria
- Marca
- Producto
- Proveedor
- Carrito
- CarritoDetalle
- Pedido
- PedidoDetalle
- Compra
- DetalleCompra
- Venta
- DetalleVenta

## Flujo de compra con JWT

1. Cliente crea cuenta.
2. Cliente inicia sesión y recibe token.
3. Cliente agrega productos al carrito.
4. Cliente confirma compra.
5. API valida stock y registra pedido.
6. API descuenta stock.
7. Cliente revisa historial.

## Plan de desarrollo sugerido

1. Análisis (requisitos, casos de uso, modelo relacional).
2. Estructura de solución y configuración JWT.
3. Implementación catálogo y autenticación.
4. Implementación carrito y pedidos.
5. Implementación panel administrativo.
6. Pruebas, documentación y evidencias.
