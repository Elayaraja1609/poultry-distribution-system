using PoultryDistributionSystem.Infrastructure.Data;

namespace PoultryDistributionSystem.API.Extensions;

/// <summary>
/// Application builder extensions for seeding database
/// </summary>
public static class ApplicationBuilderExtensions
{
    public static async Task<IApplicationBuilder> SeedDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();

        try
        {
            await DatabaseSeeder.SeedAsync(context);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding the database.");
        }

        return app;
    }
}
