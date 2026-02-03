#nullable enable
using Auctions.Application.Commands.ImportAuctions;
using Auctions.Application.DTOs.Auctions;
using Auctions.Application.Features.Auctions.BulkImport;
using Auctions.Application.Queries.ExportAuctions;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Web.Authorization;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Auctions.Api.Endpoints.Auctions;

public class AuctionImportExportEndpoints : ICarterModule
{
    private static readonly string[] ExportHeaders =
    [
        "Id", "Title", "Description", "Condition", "YearManufactured",
        "ReservePrice", "Currency", "Seller", "Winner", "SoldAmount",
        "CurrentHighBid", "CreatedAt", "AuctionEnd", "Status"
    ];

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auctions")
            .WithTags("Auctions")
            .WithOpenApi();

        group.MapGet("/export", ExportAuctions)
            .WithName("ExportAuctions")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Auctions.Export))
            .Produces<List<ExportAuctionDto>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("/import", ImportAuctions)
            .WithName("ImportAuctions")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Auctions.Import))
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<ImportAuctionsResultDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("/import/json", ImportAuctionsJson)
            .WithName("ImportAuctionsJson")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Auctions.Import))
            .Produces<ImportAuctionsResultDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapPost("/import/bulk", StartBulkImport)
            .WithName("StartBulkImport")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Auctions.Import))
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<BulkImportStartResponse>(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/import/bulk/{jobId:guid}", GetBulkImportProgress)
            .WithName("GetBulkImportProgress")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Auctions.Import))
            .Produces<BulkImportProgress>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/import/bulk", GetUserBulkImports)
            .WithName("GetUserBulkImports")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Auctions.Import))
            .Produces<List<BulkImportProgress>>(StatusCodes.Status200OK);

        group.MapDelete("/import/bulk/{jobId:guid}", CancelBulkImport)
            .WithName("CancelBulkImport")
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Auctions.Import))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> ExportAuctions(
        [AsParameters] ExportAuctionsRequest request,
        IMediator mediator,
        IExcelService excelService,
        CancellationToken ct)
    {
        var query = new ExportAuctionsQuery(
            request.Status,
            request.Seller,
            request.StartDate,
            request.EndDate);

        var result = await mediator.Send(query, ct);

        if (!result.IsSuccess)
            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));

        var auctions = result.Value!;
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

        return request.Format?.ToLowerInvariant() switch
        {
            "csv" => ExportAsCsv(auctions, timestamp),
            "excel" or "xlsx" => ExportAsExcel(auctions, timestamp, excelService),
            _ => Results.Ok(auctions)
        };
    }

    private static IResult ExportAsCsv(List<ExportAuctionDto> auctions, string timestamp)
    {
        var csv = ConvertToCsv(auctions);
        return Results.File(
            System.Text.Encoding.UTF8.GetBytes(csv),
            "text/csv",
            $"auctions-export-{timestamp}.csv");
    }

    private static IResult ExportAsExcel(List<ExportAuctionDto> auctions, string timestamp, IExcelService excelService)
    {
        var bytes = excelService.GenerateFile(
            auctions,
            ExportHeaders,
            a => new object?[]
            {
                a.Id.ToString(),
                a.Title,
                a.Description,
                a.Condition,
                a.YearManufactured,
                a.ReservePrice,
                a.Currency,
                a.Seller,
                a.Winner,
                a.SoldAmount,
                a.CurrentHighBid,
                a.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                a.AuctionEnd.ToString("yyyy-MM-dd HH:mm:ss"),
                a.Status
            },
            new ExcelExportConfig { SheetName = "Auctions" });

        return Results.File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"auctions-export-{timestamp}.xlsx");
    }

    private static async Task<IResult> ImportAuctions(
        IFormFile file,
        HttpContext httpContext,
        IMediator mediator,
        IExcelService excelService,
        CancellationToken ct)
    {
        if (file.Length == 0)
            return Results.BadRequest(ProblemDetailsHelper.FromError(
                Error.Create("Import.EmptyFile", "The uploaded file is empty")));

        var sellerId = UserHelper.GetRequiredUserId(httpContext.User);
        var sellerUsername = UserHelper.GetUsername(httpContext.User);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        List<ImportAuctionDto> auctions;

        await using var stream = file.OpenReadStream();

        if (extension is ".xlsx" or ".xls")
        {
            auctions = excelService.ParseFile(stream, row => new ImportAuctionDto
            {
                Title = row.GetString("Title") ?? "",
                Description = row.GetString("Description") ?? "",
                Condition = row.GetString("Condition"),
                YearManufactured = row.GetInt("YearManufactured") is var year and > 0 ? year : null,
                ReservePrice = row.GetDecimal("ReservePrice"),
                Currency = row.GetString("Currency") ?? "USD",
                AuctionEnd = row.GetDateTimeOffset("AuctionEnd") ?? DateTimeOffset.UtcNow.AddDays(7)
            });
        }
        else if (extension == ".csv")
        {
            auctions = ParseCsv(stream);
        }
        else
        {
            return Results.BadRequest(ProblemDetailsHelper.FromError(
                Error.Create("Import.UnsupportedFormat", "Supported formats: .xlsx, .xls, .csv")));
        }

        var command = new ImportAuctionsCommand(auctions, sellerId, sellerUsername);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> ImportAuctionsJson(
        [FromBody] List<ImportAuctionDto> auctions,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken ct)
    {
        var sellerId = UserHelper.GetRequiredUserId(httpContext.User);
        var sellerUsername = UserHelper.GetUsername(httpContext.User);

        var command = new ImportAuctionsCommand(auctions, sellerId, sellerUsername);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static List<ImportAuctionDto> ParseCsv(Stream stream)
    {
        using var reader = new StreamReader(stream);
        var auctions = new List<ImportAuctionDto>();

        var headerLine = reader.ReadLine();
        if (string.IsNullOrEmpty(headerLine))
            return auctions;

        var headers = ParseCsvLine(headerLine);
        var headerIndex = headers
            .Select((h, i) => (Header: h.Trim(), Index: i))
            .ToDictionary(x => x.Header, x => x.Index, StringComparer.OrdinalIgnoreCase);

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var values = ParseCsvLine(line);

            var auction = new ImportAuctionDto
            {
                Title = GetCsvValue(values, headerIndex, "Title") ?? "",
                Description = GetCsvValue(values, headerIndex, "Description") ?? "",
                Condition = GetCsvValue(values, headerIndex, "Condition"),
                YearManufactured = int.TryParse(GetCsvValue(values, headerIndex, "YearManufactured"), out var year) ? year : null,
                ReservePrice = decimal.TryParse(GetCsvValue(values, headerIndex, "ReservePrice"), out var price) ? price : 0,
                Currency = GetCsvValue(values, headerIndex, "Currency") ?? "USD",
                AuctionEnd = DateTimeOffset.TryParse(GetCsvValue(values, headerIndex, "AuctionEnd"), out var endDate)
                    ? endDate
                    : DateTimeOffset.UtcNow.AddDays(7)
            };

            auctions.Add(auction);
        }

        return auctions;
    }

    private static string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        var inQuotes = false;
        var current = new System.Text.StringBuilder();

        foreach (var c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }
        result.Add(current.ToString());

        return result.ToArray();
    }

    private static string? GetCsvValue(string[] values, Dictionary<string, int> headerIndex, string header)
    {
        if (!headerIndex.TryGetValue(header, out var index) || index >= values.Length)
            return null;
        var value = values[index].Trim();
        return string.IsNullOrEmpty(value) ? null : value;
    }

    private static string ConvertToCsv(List<ExportAuctionDto> auctions)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine(string.Join(",", ExportHeaders));

        foreach (var auction in auctions)
        {
            sb.AppendLine(string.Join(",",
                EscapeCsv(auction.Id.ToString()),
                EscapeCsv(auction.Title),
                EscapeCsv(auction.Description),
                EscapeCsv(auction.Condition ?? ""),
                auction.YearManufactured?.ToString() ?? "",
                auction.ReservePrice.ToString("F2"),
                EscapeCsv(auction.Currency),
                EscapeCsv(auction.Seller),
                EscapeCsv(auction.Winner ?? ""),
                auction.SoldAmount?.ToString("F2") ?? "",
                auction.CurrentHighBid?.ToString("F2") ?? "",
                auction.CreatedAt.ToString("o"),
                auction.AuctionEnd.ToString("o"),
                EscapeCsv(auction.Status)));
        }

        return sb.ToString();
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";

        return value;
    }

    private static async Task<IResult> StartBulkImport(
        IFormFile file,
        HttpContext httpContext,
        IBulkImportService bulkImportService)
    {
        if (file.Length == 0)
            return Results.BadRequest(ProblemDetailsHelper.FromError(
                Error.Create("Import.EmptyFile", "The uploaded file is empty")));

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension is not (".xlsx" or ".xls" or ".csv"))
            return Results.BadRequest(ProblemDetailsHelper.FromError(
                Error.Create("Import.UnsupportedFormat", "Supported formats: .xlsx, .xls, .csv")));

        var userId = UserHelper.GetRequiredUserId(httpContext.User);
        var username = UserHelper.GetUsername(httpContext.User);

        await using var stream = file.OpenReadStream();
        var jobId = await bulkImportService.StartImportAsync(stream, file.FileName, userId, username);

        return Results.Accepted($"/api/v1/auctions/import/bulk/{jobId}", new BulkImportStartResponse
        {
            JobId = jobId,
            Message = "Bulk import job started. Poll the progress endpoint to track status."
        });
    }

    private static IResult GetBulkImportProgress(
        Guid jobId,
        IBulkImportService bulkImportService)
    {
        var progress = bulkImportService.GetProgress(jobId);
        return progress != null
            ? Results.Ok(progress)
            : Results.NotFound();
    }

    private static IResult GetUserBulkImports(
        HttpContext httpContext,
        IBulkImportService bulkImportService)
    {
        var userId = UserHelper.GetRequiredUserId(httpContext.User);
        var jobs = bulkImportService.GetUserJobs(userId);
        return Results.Ok(jobs);
    }

    private static IResult CancelBulkImport(
        Guid jobId,
        IBulkImportService bulkImportService)
    {
        bulkImportService.CancelJob(jobId);
        return Results.NoContent();
    }

    public record BulkImportStartResponse
    {
        public Guid JobId { get; init; }
        public string Message { get; init; } = string.Empty;
    }
}
