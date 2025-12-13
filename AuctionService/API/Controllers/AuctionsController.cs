#nullable enable
using Asp.Versioning;
using AuctionService.Application.Services;
using AuctionService.Application.Commands.ActivateAuction;
using AuctionService.Application.Commands.CreateAuction;
using AuctionService.Application.Commands.DeactivateAuction;
using AuctionService.Application.Commands.DeleteAuction;
using AuctionService.Application.Commands.ImportAuctions;
using AuctionService.Application.Commands.UpdateAuction;
using AuctionService.Application.DTOs;
using AuctionService.Application.DTOs.Requests;
using AuctionService.Application.Queries.ExportAuctions;
using AuctionService.Application.Queries.GetAuctionById;
using AuctionService.Application.Queries.GetAuctions;
using AuctionService.Application.Queries.GetMyAuctions;
using AuctionService.Application.Queries.GetSellerAnalytics;
using AuctionService.Application.Queries.GetUserDashboardStats;
using Common.Core.Helpers;
using Common.Utilities.Constants;
using Common.Utilities.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAuctionExcelService _excelService;

    public AuctionsController(IMediator mediator, IAuctionExcelService excelService)
    {
        _mediator = mediator;
        _excelService = excelService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AuctionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AuctionDto>>> GetAuctions(
        [FromQuery] GetAuctionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAuctionsQuery(
            request.Status, request.Seller, request.Winner, request.SearchTerm,
            request.Category, request.IsFeatured,
            request.PageNumber, request.PageSize, request.OrderBy, request.Descending);

        var result = await _mediator.Send(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    [HttpGet("featured")]
    [ProducesResponseType(typeof(PagedResult<AuctionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AuctionDto>>> GetFeaturedAuctions(
        [FromQuery] int pageSize = 8,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAuctionsQuery(
            null, null, null, null,
            null, true,
            1, pageSize, "auctionEnd", false);

        var result = await _mediator.Send(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    [HttpGet("my")]
    [Authorize(Policy = "AuctionScope")]
    [ProducesResponseType(typeof(PagedResult<AuctionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<AuctionDto>>> GetMyAuctions(
        [FromQuery] GetMyAuctionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var username = UserHelper.GetUsername(User);
        
        var query = new GetMyAuctionsQuery(
            username,
            request.Status,
            request.SearchTerm,
            request.PageNumber,
            request.PageSize,
            request.OrderBy,
            request.Descending);

        var result = await _mediator.Send(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    [HttpGet("dashboard/stats")]
    [Authorize(Policy = "AuctionScope")]
    [ProducesResponseType(typeof(UserDashboardStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDashboardStatsDto>> GetDashboardStats(
        CancellationToken cancellationToken = default)
    {
        var username = UserHelper.GetUsername(User);
        var query = new GetUserDashboardStatsQuery(username);
        var result = await _mediator.Send(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    [HttpGet("analytics/seller")]
    [Authorize(Policy = "AuctionScope")]
    [ProducesResponseType(typeof(SellerAnalyticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SellerAnalyticsDto>> GetSellerAnalytics(
        [FromQuery] string timeRange = "30d",
        CancellationToken cancellationToken = default)
    {
        var username = UserHelper.GetUsername(User);
        var query = new GetSellerAnalyticsQuery(username, timeRange);
        var result = await _mediator.Send(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AuctionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetAuctionByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(ProblemDetailsHelper.FromError(result.Error!));
    }

    [HttpPost]
    [Authorize(Policy = "AuctionWrite")]
    [ProducesResponseType(typeof(AuctionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuctionDto>> CreateAuction(
        [FromBody] CreateAuctionWithFileIdsDto dto,
        CancellationToken cancellationToken)
    {
        var seller = UserHelper.GetUsername(User);

        var command = new CreateAuctionCommand(
            dto.Title,
            dto.Description,
            dto.Make,
            dto.Model,
            dto.Year,
            dto.Color,
            dto.Mileage,
            dto.ReservePrice,
            dto.BuyNowPrice,
            dto.AuctionEnd,
            seller,
            dto.FileIds);

        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetAuctionById), new { id = result.Value!.Id }, new { id = result.Value.Id })
            : BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }


    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AuctionOwner")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateAuction(
        Guid id,
        [FromBody] UpdateAuctionDto updateAuctionDto,
        CancellationToken cancellationToken)
    {
        var command = new UpdateAuctionCommand(
            id,
            updateAuctionDto.Title,
            updateAuctionDto.Description,
            updateAuctionDto.Make,
            updateAuctionDto.Model,
            updateAuctionDto.Year,
            updateAuctionDto.Color,
            updateAuctionDto.Mileage);

        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : NotFound(ProblemDetailsHelper.FromError(result.Error!));
    }


    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AuctionOwner")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAuction(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteAuctionCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : NotFound(ProblemDetailsHelper.FromError(result.Error!));
    }


    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = "AuctionOwner")]
    [ProducesResponseType(typeof(AuctionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuctionDto>> ActivateAuction(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ActivateAuctionCommand(id);
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return result.Error!.Code.Contains("NotFound")
            ? NotFound(ProblemDetailsHelper.FromError(result.Error!))
            : BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = "AuctionOwner")]
    [ProducesResponseType(typeof(AuctionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuctionDto>> DeactivateAuction(
        Guid id,
        [FromQuery] string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var command = new DeactivateAuctionCommand(id, reason);
        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return result.Error!.Code.Contains("NotFound")
            ? NotFound(ProblemDetailsHelper.FromError(result.Error!))
            : BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    [HttpPost("{id:guid}/buy-now")]
    [Authorize(Policy = "AuctionScope")]
    [ProducesResponseType(typeof(BuyNowResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BuyNowResultDto>> BuyNow(
        Guid id,
        CancellationToken cancellationToken)
    {
        var buyer = UserHelper.GetUsername(User);
        var command = new Application.Commands.BuyNow.BuyNowCommand(id, buyer);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return result.Error!.Code.Contains("NotFound")
            ? NotFound(ProblemDetailsHelper.FromError(result.Error!))
            : BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }


    [HttpPost("import")]
    [Authorize(Policy = "AuctionWrite")]
    [ProducesResponseType(typeof(ImportAuctionsResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ImportAuctionsResultDto>> ImportAuctions(
        [FromBody] List<ImportAuctionDto> auctions,
        CancellationToken cancellationToken)
    {
        if (auctions == null || auctions.Count == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Validation.EmptyImport",
                Detail = "No auctions provided for import",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var seller = UserHelper.GetUsername(User);
        var command = new ImportAuctionsCommand(auctions, seller);
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    [HttpGet("export")]
    [Authorize(Policy = "AuctionScope")]
    [ProducesResponseType(typeof(List<ExportAuctionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportAuctions(
        [FromQuery] ExportAuctionsRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = new ExportAuctionsQuery(request.Status, request.Seller, request.StartDate, request.EndDate);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        var data = result.Value!;

        return request.Format.ToLower() switch
        {
            "csv" => ExportAsCsv(data),
            "excel" or "xlsx" => ExportAsExcel(data),
            _ => Ok(data)
        };
    }

    [HttpPost("import/excel")]
    [Authorize(Policy = "AuctionWrite")]
    [ProducesResponseType(typeof(ImportAuctionsResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ImportAuctionsResultDto>> ImportFromExcel(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Validation.NoFile",
                Detail = "No file was uploaded",
                Status = StatusCodes.Status400BadRequest
            });
        }

        if (!FileHelper.IsValidExcelExtension(file.FileName))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Validation.InvalidFileType",
                Detail = "Only Excel files (.xlsx, .xls) are allowed",
                Status = StatusCodes.Status400BadRequest
            });
        }

        try
        {
            using var stream = file.OpenReadStream();
            var auctions = _excelService.ParseImportFile(stream);

            if (auctions.Count == 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation.EmptyFile",
                    Detail = "No valid auction data found in the Excel file",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var seller = UserHelper.GetUsername(User);
            var command = new ImportAuctionsCommand(auctions, seller);
            var result = await _mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }
        catch (Exception ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Import.ParseError",
                Detail = $"Failed to parse Excel file: {ex.Message}",
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    [HttpGet("import/template")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public IActionResult DownloadImportTemplate()
    {
        var templateBytes = _excelService.GetImportTemplate();
        return File(templateBytes, 
            FileConstants.ContentTypes.Excel, 
            $"auction_import_template{FileConstants.Extensions.Excel}");
    }

    private FileContentResult ExportAsExcel(List<ExportAuctionDto> auctions)
    {
        var excelBytes = _excelService.GenerateExportFile(auctions);
        return File(excelBytes, 
            FileConstants.ContentTypes.Excel, 
            FileHelper.GenerateExportFileName("auctions_export", FileConstants.Extensions.Excel));
    }

    private FileContentResult ExportAsCsv(List<ExportAuctionDto> auctions)
    {
        var headers = new[] { "Id", "Title", "Description", "Make", "Model", "Year", "Color", "Mileage", "ReservePrice", "Seller", "Winner", "SoldAmount", "CurrentHighBid", "CreatedAt", "AuctionEnd", "Status" };
        var rows = auctions.Select(a => new[]
        {
            a.Id.ToString(),
            a.Title,
            a.Description,
            a.Make,
            a.Model,
            a.Year.ToString(),
            a.Color,
            a.Mileage.ToString(),
            a.ReservePrice.ToString(),
            a.Seller,
            a.Winner ?? "",
            a.SoldAmount?.ToString() ?? "",
            a.CurrentHighBid?.ToString() ?? "",
            a.CreatedAt.ToString("O"),
            a.AuctionEnd.ToString("O"),
            a.Status
        });

        var bytes = CsvHelper.GenerateCsv(headers, rows);
        return File(bytes, FileConstants.ContentTypes.Csv, FileHelper.GenerateExportFileName("auctions_export", FileConstants.Extensions.Csv));
    }
}
