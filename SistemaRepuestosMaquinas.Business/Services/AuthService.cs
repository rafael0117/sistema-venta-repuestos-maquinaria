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
    private const int MinPasswordLength = 8;

    public async Task<AuthResponse> RegisterClienteAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRegisterRequest(request);

        var correo = request.Correo.Trim().ToLowerInvariant();
        var existe = await context.Usuarios.AnyAsync(x => x.Correo == correo, cancellationToken);
        if (existe)
        {
            throw new InvalidOperationException("El correo ya está registrado.");
        }

        var rolCliente = await GetOrCreateClienteRoleAsync(cancellationToken);

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
            TipoDocumento = request.TipoDocumento.Trim().ToUpperInvariant(),
            NumeroDocumento = request.NumeroDocumento.Trim(),
            Telefono = request.Telefono.Trim(),
            Direccion = request.Direccion.Trim()
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

    public async Task<PasswordResetNotificationData?> RequestPasswordResetAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var correo = request.Correo.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(correo))
        {
            return null;
        }

        var usuario = await context.Usuarios
            .FirstOrDefaultAsync(x => x.Correo == correo, cancellationToken);

        if (usuario is null)
        {
            return null;
        }

        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        usuario.ResetPasswordTokenHash = ComputeHash(rawToken);
        usuario.ResetPasswordExpiresAtUtc = DateTime.UtcNow.AddMinutes(30);
        await context.SaveChangesAsync(cancellationToken);

        return new PasswordResetNotificationData(
            usuario.Correo,
            $"{usuario.Nombres} {usuario.Apellidos}".Trim(),
            Uri.EscapeDataString(rawToken));
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            throw new InvalidOperationException("El token de recuperación es obligatorio.");
        }

        ValidateNewPassword(request.NewPassword, request.ConfirmPassword);

        var token = Uri.UnescapeDataString(request.Token.Trim());
        var tokenHash = ComputeHash(token);

        var usuario = await context.Usuarios.FirstOrDefaultAsync(
            x => x.ResetPasswordTokenHash == tokenHash,
            cancellationToken);

        if (usuario is null || !usuario.ResetPasswordExpiresAtUtc.HasValue || usuario.ResetPasswordExpiresAtUtc.Value < DateTime.UtcNow)
        {
            throw new InvalidOperationException("El enlace de recuperación no es válido o ha expirado.");
        }

        usuario.PasswordHash = ComputeHash(request.NewPassword);
        usuario.ResetPasswordTokenHash = null;
        usuario.ResetPasswordExpiresAtUtc = null;
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task<Rol> GetOrCreateClienteRoleAsync(CancellationToken cancellationToken)
    {
        var rolCliente = await context.Roles
            .FirstOrDefaultAsync(x => x.Nombre == Roles.Cliente, cancellationToken);

        if (rolCliente is not null)
        {
            return rolCliente;
        }

        rolCliente = new Rol { Nombre = Roles.Cliente };
        context.Roles.Add(rolCliente);
        await context.SaveChangesAsync(cancellationToken);
        return rolCliente;
    }

    private static void ValidateRegisterRequest(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombres) || request.Nombres.Trim().Length < 2)
            throw new InvalidOperationException("Los nombres deben tener al menos 2 caracteres.");

        if (string.IsNullOrWhiteSpace(request.Apellidos) || request.Apellidos.Trim().Length < 2)
            throw new InvalidOperationException("Los apellidos deben tener al menos 2 caracteres.");

        if (string.IsNullOrWhiteSpace(request.Telefono) || request.Telefono.Trim().Length < 9)
            throw new InvalidOperationException("El teléfono debe tener al menos 9 dígitos.");

        if (string.IsNullOrWhiteSpace(request.TipoDocumento) || string.IsNullOrWhiteSpace(request.NumeroDocumento))
            throw new InvalidOperationException("Debes indicar tipo y número de documento.");

        if (string.IsNullOrWhiteSpace(request.Direccion) || request.Direccion.Trim().Length < 6)
            throw new InvalidOperationException("La dirección debe tener al menos 6 caracteres.");

        ValidateNewPassword(request.Password, request.ConfirmPassword);
    }

    private static void ValidateNewPassword(string password, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < MinPasswordLength)
            throw new InvalidOperationException("La contraseña debe tener mínimo 8 caracteres.");

        if (!password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit) || !password.Any(ch => !char.IsLetterOrDigit(ch)))
            throw new InvalidOperationException("La contraseña debe incluir mayúscula, minúscula, número y símbolo.");

        if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            throw new InvalidOperationException("La confirmación de contraseña no coincide.");
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
