using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UtilityService.DTOs;
using UtilityService.Interfaces;

namespace UtilityService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    private string GetUsername()
    {
        return User.Identity?.Name
            ?? User.FindFirst("username")?.Value
            ?? "Anonymous";
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(PagedReportsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedReportsDto>> GetReports(
        [FromQuery] ReportQueryParams queryParams,
        CancellationToken cancellationToken)
    {
        var result = await _reportService.GetReportsAsync(queryParams, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(ReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReportDto>> GetReport(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var report = await _reportService.GetReportByIdAsync(id, cancellationToken);
            return Ok(report);
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Report not found");
        }
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ReportDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ReportDto>> CreateReport(
        [FromBody] CreateReportDto dto,
        CancellationToken cancellationToken)
    {
        var username = GetUsername();

        var report = await _reportService.CreateReportAsync(username, dto, cancellationToken);
        return CreatedAtAction(nameof(GetReport), new { id = report.Id }, report);
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateReportStatus(
        Guid id,
        [FromBody] UpdateReportStatusDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var adminUsername = GetUsername();
            await _reportService.UpdateReportStatusAsync(id, dto, adminUsername, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Report not found");
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteReport(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _reportService.DeleteReportAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound("Report not found");
        }
    }

    [HttpGet("stats")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(ReportStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReportStatsDto>> GetStats(CancellationToken cancellationToken)
    {
        var stats = await _reportService.GetReportStatsAsync(cancellationToken);
        return Ok(stats);
    }
}
