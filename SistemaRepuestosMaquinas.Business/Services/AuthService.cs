using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SistemaRepuestosMaquinas.Business.DTOs;
using SistemaRepuestosMaquinas.Business.Interfaces;
using SistemaRepuestosMaquinas.Business.Security;
using SistemaRepuestosMaquinas.Common.Constants;
using SistemaRepuestosMaquinas.Data.Context;
using SistemaRepuestosMaquinas.Entity;

namespace SistemaRepuestosMaquinas.Business.Services;

public class AuthService(
    IOptions<JwtOptions> jwtOptions,
    ApplicationDbContext context) : IAuthService
{
    public async Task<AuthResponse> RegisterClienteAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var correo = request.Correo.Trim().ToLowerInvariant();
        var existe = await context.Usuarios.AnyAsync(x => x.Correo == correo, cancellationToken);
        if (existe)
        {
            throw new InvalidOperationException("El correo ya está registrado.");
        }

        var rolCliente = await context.Roles
            .FirstOrDefaultAsync(x => x.Nombre == Roles.Cliente, cancellationToken);

        if (rolCliente is null)
        {
            rolCliente = new Rol { Nombre = Roles.Cliente };
            context.Roles.Add(rolCliente);
            await context.SaveChangesAsync(cancellationToken);
        }

        var usuario = new Usuario
        {
            IdRol = rolCliente.IdRol,
            Nombres = request.Nombres.Trim(),
            Apellidos = request.Apellidos.Trim(),
            Correo = correo,
            PasswordHash = ComputeHash(request.Password)
        };

        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync(cancellationToken);

        var cliente = new Cliente
        {
            IdUsuario = usuario.IdUsuario,
            TipoDocumento = "DNI",
            NumeroDocumento = string.Empty,
            Telefono = string.Empty,
            Direccion = string.Empty
        };

        context.Clientes.Add(cliente);
        await context.SaveChangesAsync(cancellationToken);

        return BuildToken(usuario, rolCliente.Nombre, cliente.IdCliente);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var correo = request.Correo.Trim().ToLowerInvariant();
        var hash = ComputeHash(request.Password);

        var usuario = await context.Usuarios
            .Include(x => x.Rol)
            .FirstOrDefaultAsync(x => x.Correo == correo && x.PasswordHash == hash, cancellationToken);

        if (usuario is null || usuario.Rol is null)
        {
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }

        var idCliente = await context.Clientes
            .Where(x => x.IdUsuario == usuario.IdUsuario)
            .Select(x => (int?)x.IdCliente)
            .FirstOrDefaultAsync(cancellationToken);

        return BuildToken(usuario, usuario.Rol.Nombre, idCliente);
    }

    private AuthResponse BuildToken(Usuario usuario, string role, int? idCliente = null)
    {
        var options = jwtOptions.Value;
        var expiration = DateTime.UtcNow.AddMinutes(options.ExpirationMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.IdUsuario.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Correo),
            new(ClaimTypes.Name, $"{usuario.Nombres} {usuario.Apellidos}".Trim()),
            new(ClaimTypes.Role, role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            expires: expiration,
            signingCredentials: creds);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return new AuthResponse(jwt, expiration, role, idCliente);
    }

    private static string ComputeHash(string raw)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes);
    }
}
