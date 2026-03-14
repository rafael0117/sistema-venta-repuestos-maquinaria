using Microsoft.Extensions.Options;
using SistemaRepuestosMaquinas.Business.DTOs;
using SistemaRepuestosMaquinas.Business.Interfaces;
using SistemaRepuestosMaquinas.Business.Security;

namespace SistemaRepuestosMaquinas.Business.Services;

public class AuthService(IOptions<JwtOptions> jwtOptions) : IAuthService
{
    public Task<AuthResponse> RegisterClienteAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        // TODO: persistir usuario/cliente y hashear password con ASP.NET Identity o BCrypt.
        return Task.FromResult(BuildMockToken("Cliente"));
    }

    public Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        // TODO: validar credenciales reales contra base de datos.
        return Task.FromResult(BuildMockToken("Cliente"));
    }

    private AuthResponse BuildMockToken(string role)
    {
        var expiration = DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpirationMinutes);
        return new AuthResponse("jwt-demo-reemplazar-por-token-real", expiration, role);
    }
}
