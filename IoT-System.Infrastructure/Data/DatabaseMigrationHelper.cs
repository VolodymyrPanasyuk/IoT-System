using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IoT_System.Infrastructure.Data;

/// <summary>
/// Helper class for applying migrations to multiple DbContexts
/// </summary>
public static class DatabaseMigrationHelper
{
    /// <summary>
    /// Apply migrations to all registered DbContext types
    /// </summary>
    public static async Task MigrateAllDatabasesAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        // Get all DbContext types to migrate
        var contextTypes = GetDbContextTypes();

        foreach (var contextType in contextTypes)
        {
            await MigrateDatabaseAsync(scope.ServiceProvider, contextType);
        }
    }

    /// <summary>
    /// Apply migrations to specific DbContext types
    /// </summary>
    public static async Task MigrateDatabaseAsync<T>(IServiceProvider serviceProvider) where T : DbContext
    {
        var context = serviceProvider.GetRequiredService<T>();
        await context.Database.MigrateAsync();
    }

    #region Private Methods

    private static async Task MigrateDatabaseAsync(IServiceProvider serviceProvider, Type contextType)
    {
        var context = serviceProvider.GetRequiredService(contextType) as DbContext;
        if (context == null)
        {
            throw new InvalidOperationException($"Could not resolve {contextType.Name} as DbContext");
        }

        await context.Database.MigrateAsync();
    }

    private static List<Type> GetDbContextTypes()
    {
        // Automatically discover all DbContext types in the application
        var assembly = typeof(DatabaseMigrationHelper).Assembly;

        return assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(DbContext).IsAssignableFrom(t))
            .ToList();
    }

    #endregion
}