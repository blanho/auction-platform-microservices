using Analytics.Api.Models;
using BuildingBlocks.Application.BackgroundJobs.Core;
using BuildingBlocks.Application.BackgroundJobs.Services;
using BuildingBlocks.Application.Helpers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Analytics.Api.Endpoints;

public static class JobsEndpoints
{
    public static IEndpointRouteBuilder MapJobsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/jobs")
            .WithTags("Background Jobs")
            .RequireAuthorization();

        group.MapGet("/{jobId:guid}", GetJobStatus)
            .WithName("GetJobStatus")
            .WithSummary("Get the status of a background job");

        group.MapGet("/", GetUserJobs)
            .WithName("GetUserJobs")
            .WithSummary("Get all background jobs for the current user");

        group.MapPost("/{jobId:guid}/cancel", CancelJob)
            .WithName("CancelJob")
            .WithSummary("Cancel a running background job");

        group.MapGet("/{jobId:guid}/download", DownloadJobResult)
            .WithName("DownloadJobResult")
            .WithSummary("Download the result of a completed job");

        return endpoints;
    }

    private static async Task<Results<Ok<BackgroundJobResponse>, NotFound>> GetJobStatus(
        Guid jobId,
        IBackgroundJobService jobService,
        CancellationToken ct)
    {
        var job = await jobService.GetJobAsync(jobId, ct);

        if (job is null)
            return TypedResults.NotFound();

        return TypedResults.Ok(MapToResponse(job));
    }

    private static async Task<Ok<IEnumerable<BackgroundJobResponse>>> GetUserJobs(
        IBackgroundJobService jobService,
        HttpContext httpContext,
        [FromQuery] string? jobType,
        CancellationToken ct)
    {
        var userId = GetUserId(httpContext);
        var jobs = await jobService.GetUserJobsAsync(userId, jobType, ct);

        var response = jobs.Select(MapToResponse);
        return TypedResults.Ok(response);
    }

    private static async Task<Results<Ok, NotFound>> CancelJob(
        Guid jobId,
        IBackgroundJobService jobService,
        CancellationToken ct)
    {
        var job = await jobService.GetJobAsync(jobId, ct);

        if (job is null)
            return TypedResults.NotFound();

        await jobService.CancelJobAsync(jobId, ct);
        return TypedResults.Ok();
    }

    private static async Task<Results<FileStreamHttpResult, NotFound, BadRequest<string>>> DownloadJobResult(
        Guid jobId,
        IBackgroundJobService jobService,
        CancellationToken ct)
    {
        var job = await jobService.GetJobAsync(jobId, ct);

        if (job is null)
            return TypedResults.NotFound();

        if (job.Status != BackgroundJobStatus.Completed)
            return TypedResults.BadRequest("Job has not completed yet");

        if (string.IsNullOrEmpty(job.ResultFileName))
            return TypedResults.BadRequest("No result file available for this job");

        var outputPath = Path.Combine(Path.GetTempPath(), $"job_{job.Id}", job.ResultFileName);

        if (!File.Exists(outputPath))
            return TypedResults.NotFound();

        var stream = new FileStream(outputPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var contentType = ExportHelper.GetContentType(Path.GetExtension(job.ResultFileName).TrimStart('.'));

        return TypedResults.File(stream, contentType, job.ResultFileName);
    }

    private static Guid GetUserId(HttpContext httpContext)
    {
        var userIdClaim = httpContext.User.FindFirst("sub") ?? httpContext.User.FindFirst("id");
        return userIdClaim is not null && Guid.TryParse(userIdClaim.Value, out var userId)
            ? userId
            : Guid.Empty;
    }

    private static BackgroundJobResponse MapToResponse(BackgroundJobState job)
    {
        return new BackgroundJobResponse(
            job.Id,
            job.JobType,
            job.Status.ToString(),
            job.ProgressPercentage,
            job.ProcessedItems,
            job.TotalItems,
            job.EstimatedSecondsRemaining,
            job.ResultFileName,
            job.ErrorMessage,
            job.CreatedAt,
            job.StartedAt,
            job.CompletedAt);
    }
}

public sealed record BackgroundJobResponse(
    Guid Id,
    string JobType,
    string Status,
    double ProgressPercentage,
    int ProcessedItems,
    int TotalItems,
    int EstimatedSecondsRemaining,
    string? ResultFileName,
    string? ErrorMessage,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt);
