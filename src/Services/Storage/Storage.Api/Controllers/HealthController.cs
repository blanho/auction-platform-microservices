using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Storage.Infrastructure.Persistence;

namespace Storage.Api.Controllers;

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
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.Database.CanConnectAsync(cancellationToken);
            return Ok(new { status = "healthy", service = "Storage.Api" });
        }
        catch
        {
            return StatusCode(503, new { status = "unhealthy", service = "Storage.Api" });
        }
    }
}
