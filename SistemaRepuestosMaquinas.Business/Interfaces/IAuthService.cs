using SistemaRepuestosMaquinas.Business.DTOs;

namespace SistemaRepuestosMaquinas.Business.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterClienteAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
