using Microsoft.EntityFrameworkCore;
using SistemaRepuestosMaquinas.Data.Context;

namespace SistemaRepuestosMaquinas.API.Infrastructure;

public static class DatabaseInitializer
{
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
    }
}
