using System.Globalization;
using Identity.Api;
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
