namespace SistemaRepuestosMaquinas.Business.DTOs;

public record ProductoFiltroRequest(string? Texto, int? IdCategoria, int? IdMarca, int Page = 1, int PageSize = 12);
public record CheckoutRequest(
    int IdCliente,
    string DireccionEntrega,
    string MetodoPago,
    string? CulqiToken = null,
    string? EmailPago = null);
