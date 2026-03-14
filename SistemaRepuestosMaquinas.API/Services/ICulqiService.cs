namespace SistemaRepuestosMaquinas.API.Services;

public interface ICulqiService
{
    Task<CulqiChargeResult> CreateChargeAsync(CulqiChargeRequest request, CancellationToken cancellationToken = default);
}

public record CulqiChargeRequest(decimal Total, string SourceId, string Email, int IdCliente);
public record CulqiChargeResult(bool IsSuccess, string Message, string? ChargeId);
