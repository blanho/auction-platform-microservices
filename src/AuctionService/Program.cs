using AuctionService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<AuctionDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Initialize database with seed data
try
{
    DbInitializer.InitDb(app);
}
catch (Exception ex)
{
    Console.WriteLine($"Error initializing database: {ex.Message}");
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
