using Microsoft.EntityFrameworkCore;
using SistemaRepuestosMaquinas.Data.Context;
using SistemaRepuestosMaquinas.Entity;

namespace SistemaRepuestosMaquinas.API.Infrastructure;

public static class DatabaseInitializer
{
    private static readonly string[] DefaultRoles = ["Administrador", "Vendedor", "Cliente"];

    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var hasMigrations = (await context.Database.GetMigrationsAsync(cancellationToken)).Any();
        if (hasMigrations)
        {
            await context.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await context.Database.EnsureCreatedAsync(cancellationToken);
        }

        await SeedRolesAsync(context, cancellationToken);
    }

    private static async Task SeedRolesAsync(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        var existingRoles = await context.Roles
            .Select(x => x.Nombre)
            .ToListAsync(cancellationToken);

        var missingRoles = DefaultRoles
            .Except(existingRoles, StringComparer.OrdinalIgnoreCase)
            .Select(nombre => new Rol { Nombre = nombre })
            .ToList();

        if (missingRoles.Count == 0)
        {
            return;
        }

        await context.Roles.AddRangeAsync(missingRoles, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
