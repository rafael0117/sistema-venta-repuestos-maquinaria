using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SistemaRepuestosMaquinas.Common.Constants;
using SistemaRepuestosMaquinas.Data.Context;

namespace SistemaRepuestosMaquinas.API.Infrastructure;

public static class DatabaseInitializer
{
    private const string AdminEmail = "admin@repuestos.com";
    private const string AdminPassword = "Admin123*";

    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var hasMigrations = (await context.Database.GetAppliedMigrationsAsync(cancellationToken)).Any();
        if (hasMigrations)
        {
            await context.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await context.Database.EnsureCreatedAsync(cancellationToken);
        }

        await SeedRolesAndAdminAsync(context, cancellationToken);
    }

    private static async Task SeedRolesAndAdminAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        var requiredRoles = new[] { Roles.Administrador, Roles.Vendedor, Roles.Cliente };

        var existingRoleNames = await context.Roles
            .Select(x => x.Nombre)
            .ToListAsync(cancellationToken);

        var missingRoles = requiredRoles
            .Where(role => !existingRoleNames.Any(x => string.Equals(x, role, StringComparison.OrdinalIgnoreCase)))
            .Select(role => new Rol { Nombre = role })
            .ToList();

        if (missingRoles.Count > 0)
        {
            context.Roles.AddRange(missingRoles);
            await context.SaveChangesAsync(cancellationToken);
        }

        var adminRole = await context.Roles
            .FirstAsync(x => x.Nombre == Roles.Administrador, cancellationToken);

        var adminExists = await context.Usuarios
            .AnyAsync(x => x.Correo == AdminEmail, cancellationToken);

        if (adminExists)
        {
            return;
        }

        var adminUser = new Usuario
        {
            IdRol = adminRole.IdRol,
            Nombres = "Administrador",
            Apellidos = "Sistema",
            Correo = AdminEmail,
            PasswordHash = ComputeHash(AdminPassword),
            Estado = true
        };

        context.Usuarios.Add(adminUser);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static string ComputeHash(string raw)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes);
    }
}
