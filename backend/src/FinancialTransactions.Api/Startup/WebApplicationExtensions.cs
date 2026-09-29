using FinancialTransactions.Infrastructure.Persistence;
using FinancialTransactions.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransactions.Api.Startup;

public static class WebApplicationExtensions
{
    public static async Task ApplyMigrationsAndSeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();

        await dbContext.Database.MigrateAsync();
        await DatabaseSeeder.SeedAsync(dbContext, timeProvider);
    }
}
