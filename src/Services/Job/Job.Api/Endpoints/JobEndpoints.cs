using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Web.Helpers;
using Carter;
using Jobs.Application.DTOs;
using Jobs.Application.Features.Jobs.CancelJob;
using Jobs.Application.Features.Jobs.CreateJob;
using Jobs.Application.Features.Jobs.GetJob;
using Jobs.Application.Features.Jobs.GetJobItems;
using Jobs.Application.Features.Jobs.GetJobs;
using Jobs.Application.Features.Jobs.RetryJob;
using Jobs.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Jobs.Api.Endpoints;

public class JobEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/jobs")
            .WithTags("Jobs")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapPost("/", CreateJob)
            .WithName("CreateJob")
            .Produces<JobDto>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict);

        group.MapGet("/", GetJobs)
            .WithName("GetJobs")
            .Produces<PaginatedResult<JobSummaryDto>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:guid}", GetJobById)
            .WithName("GetJobById")
            .Produces<JobDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/items", GetJobItems)
            .WithName("GetJobItems")
            .Produces<PaginatedResult<JobItemDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/cancel", CancelJob)
            .WithName("CancelJob")
            .Produces<JobDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/retry", RetryJob)
            .WithName("RetryJob")
            .Produces<JobDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateJob(
        CreateJobRequestDto request,
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken ct)
    {
        var userId = UserHelper.GetRequiredUserId(httpContext.User);

        if (!Enum.TryParse<JobType>(request.Type, true, out var jobType))
            return Results.BadRequest(ProblemDetailsHelper.FromError(
                Error.Create("Job.InvalidType", $"'{request.Type}' is not a valid job type")));

        var priority = Enum.IsDefined(typeof(JobPriority), request.Priority)
            ? (JobPriority)request.Priority
            : JobPriority.Normal;

        var command = new CreateJobCommand(
            jobType,
            request.CorrelationId,
            request.PayloadJson,
            userId,
            request.TotalItems,
            request.MaxRetryCount,
            priority,
            request.Items);

        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            if (result.Error!.Code.Contains("Duplicate"))
                return Results.Conflict(ProblemDetailsHelper.FromError(result.Error!));

            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Created($"/api/v1/jobs/{result.Value!.Id}", result.Value);
    }

    private static async Task<IResult> GetJobs(
        [FromQuery] string? type,
        [FromQuery] string? status,
        [FromQuery] Guid? requestedBy,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [AsParameters] JobsQueryServices services = default!)
    {
        JobType? jobType = null;
        if (!string.IsNullOrEmpty(type) && Enum.TryParse<JobType>(type, true, out var parsedType))
            jobType = parsedType;

        JobStatus? jobStatus = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<JobStatus>(status, true, out var parsedStatus))
            jobStatus = parsedStatus;

        var query = new GetJobsQuery(jobType, jobStatus, requestedBy, page, pageSize);
        var result = await services.Mediator.Send(query, services.CancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> GetJobById(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var query = new GetJobQuery(id);
        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(ProblemDetailsHelper.FromError(result.Error!));
    }

    private static async Task<IResult> GetJobItems(
        Guid id,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [AsParameters] JobsQueryServices services = default!)
    {
        JobItemStatus? itemStatus = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<JobItemStatus>(status, true, out var parsedStatus))
            itemStatus = parsedStatus;

        var query = new GetJobItemsQuery(id, itemStatus, page, pageSize);
        var result = await services.Mediator.Send(query, services.CancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error!.Code.Contains("NotFound"))
                return Results.NotFound(ProblemDetailsHelper.FromError(result.Error!));

            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> CancelJob(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new CancelJobCommand(id);
        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            if (result.Error!.Code.Contains("NotFound"))
                return Results.NotFound(ProblemDetailsHelper.FromError(result.Error!));

            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> RetryJob(
        Guid id,
        IMediator mediator,
        CancellationToken ct)
    {
        var command = new RetryJobCommand(id);
        var result = await mediator.Send(command, ct);

        if (!result.IsSuccess)
        {
            if (result.Error!.Code.Contains("NotFound"))
                return Results.NotFound(ProblemDetailsHelper.FromError(result.Error!));

            return Results.BadRequest(ProblemDetailsHelper.FromError(result.Error!));
        }

        return Results.Ok(result.Value);
    }
}

public struct JobsQueryServices
{
    public required IMediator Mediator { get; init; }
    public required CancellationToken CancellationToken { get; init; }
}
