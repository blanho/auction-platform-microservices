using System.Globalization;
using Identity.Api;
using Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    var app = builder
        .ConfigureLogging()
        .ConfigureServices();

    var migrateOnly = args.Contains("--migrate-only", StringComparer.OrdinalIgnoreCase);
    var autoMigrate = builder.Configuration.GetValue("Database:AutoMigrate", !app.Environment.IsProduction());
    if (migrateOnly || autoMigrate)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
    }

    if (migrateOnly)
    {
        Log.Information("Database migration completed. Exiting.");
        return;
    }

    if (args.Contains("/seed"))
    {
        Log.Information("Seeding database...");
        await SeedData.EnsureSeedDataAsync(app);
        Log.Information("Done seeding database. Exiting.");
        return;
    }

    app.ConfigurePipeline();
    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}
