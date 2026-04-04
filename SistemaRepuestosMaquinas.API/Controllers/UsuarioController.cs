using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaRepuestosMaquinas.Data.Context;
using SistemaRepuestosMaquinas.Entity;

namespace SistemaRepuestosMaquinas.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class UsuarioController(ApplicationDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var data = await context.Usuarios
            .AsNoTracking()
            .Include(x => x.Rol)
            .OrderBy(x => x.Nombres)
            .Select(x => new
            {
                x.IdUsuario,
                x.IdRol,
                Rol = x.Rol != null ? x.Rol.Nombre : string.Empty,
                x.Nombres,
                x.Apellidos,
                x.Correo,
                x.Estado
            })
            .ToListAsync(cancellationToken);

        return Ok(data);
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        var data = await context.Roles.AsNoTracking().OrderBy(x => x.Nombre).ToListAsync(cancellationToken);
        return Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UsuarioCreateRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            return BadRequest(new { message = "La contraseña debe tener al menos 8 caracteres." });

        var correo = request.Correo.Trim().ToLowerInvariant();
        var exists = await context.Usuarios.AnyAsync(x => x.Correo == correo, cancellationToken);
        if (exists)
            return BadRequest(new { message = "El correo ya está registrado." });

        var usuario = new Usuario
        {
            IdRol = request.IdRol,
            Nombres = request.Nombres.Trim(),
            Apellidos = request.Apellidos.Trim(),
            Correo = correo,
            PasswordHash = ComputeHash(request.Password),
            Estado = request.Estado
        };

        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync(cancellationToken);
        return Ok(usuario);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UsuarioUpdateRequest request, CancellationToken cancellationToken)
    {
        var usuario = await context.Usuarios.FirstOrDefaultAsync(x => x.IdUsuario == id, cancellationToken);
        if (usuario is null) return NotFound();

        var correo = request.Correo.Trim().ToLowerInvariant();
        var exists = await context.Usuarios.AnyAsync(x => x.Correo == correo && x.IdUsuario != id, cancellationToken);
        if (exists)
            return BadRequest(new { message = "El correo ya está registrado." });

        usuario.IdRol = request.IdRol;
        usuario.Nombres = request.Nombres.Trim();
        usuario.Apellidos = request.Apellidos.Trim();
        usuario.Correo = correo;
        usuario.Estado = request.Estado;

        await context.SaveChangesAsync(cancellationToken);
        return Ok(usuario);
    }

    [HttpPost("{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] UsuarioResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var usuario = await context.Usuarios.FirstOrDefaultAsync(x => x.IdUsuario == id, cancellationToken);
        if (usuario is null) return NotFound();

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
            return BadRequest(new { message = "La nueva contraseña debe tener al menos 8 caracteres." });

        usuario.PasswordHash = ComputeHash(request.NewPassword);
        usuario.ResetPasswordTokenHash = null;
        usuario.ResetPasswordExpiresAtUtc = null;

        await context.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Contraseña restablecida correctamente." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var usuario = await context.Usuarios.FirstOrDefaultAsync(x => x.IdUsuario == id, cancellationToken);
        if (usuario is null) return NotFound();

        context.Usuarios.Remove(usuario);
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static string ComputeHash(string raw)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes);
    }

    public record UsuarioCreateRequest(int IdRol, string Nombres, string Apellidos, string Correo, string Password, bool Estado = true);
    public record UsuarioUpdateRequest(int IdRol, string Nombres, string Apellidos, string Correo, bool Estado = true);
    public record UsuarioResetPasswordRequest(string NewPassword);
}
