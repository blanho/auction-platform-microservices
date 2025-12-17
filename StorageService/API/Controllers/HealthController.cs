using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StorageService.Infrastructure.Data;

namespace StorageService.API.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly StorageDbContext _dbContext;

    public HealthController(StorageDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            await _dbContext.Database.CanConnectAsync();
            return Ok(new { status = "healthy", service = "StorageService" });
        }
        catch
        {
            return StatusCode(503, new { status = "unhealthy", service = "StorageService" });
        }
    }
}
