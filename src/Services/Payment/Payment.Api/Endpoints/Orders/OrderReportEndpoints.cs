using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Web.Authorization;
using BuildingBlocks.Web.Extensions;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Features.Orders.QueueOrderReportGeneration;
using Payment.Domain.Enums;

namespace Payment.Api.Endpoints.Orders;

public class OrderReportEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/orders/reports")
            .WithTags("Order Reports")
            .RequireAuthorization();

        group.MapPost("/generate", QueueReportGeneration)
            .WithName("QueueOrderReportGeneration")
            .WithSummary("Queue order report generation as background job")
            .Produces<BackgroundJobResult>(StatusCodes.Status202Accepted)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .RequireAuthorization(new RequirePermissionAttribute(Permissions.Reports.Create));
    }

    private static async Task<IResult> QueueReportGeneration(
        QueueOrderReportRequest request,
        IMediator mediator,
        ILogger<OrderReportEndpoints> logger,
        CancellationToken cancellationToken)
    {
        var command = new QueueOrderReportCommand(
            RequestedBy: request.RequestedBy,
            ReportType: request.ReportType,
            Format: request.Format,
            StatusFilter: request.StatusFilter,
            BuyerIdFilter: request.BuyerIdFilter,
            SellerIdFilter: request.SellerIdFilter,
            StartDate: request.StartDate,
            EndDate: request.EndDate);

        var result = await mediator.Send(command, cancellationToken);

        return result.ToApiResult(value =>
        {
            logger.LogInformation(
                "Report generation queued: JobId={JobId}, Type={ReportType}, Format={Format}",
                value.JobId, request.ReportType, request.Format);

            return Results.Accepted($"/api/v1/orders/reports/status/{value.JobId}", value);
        });
    }
}

public record QueueOrderReportRequest(
    Guid RequestedBy,
    ReportType ReportType,
    ReportFormat Format,
    OrderStatus? StatusFilter = null,
    Guid? BuyerIdFilter = null,
    Guid? SellerIdFilter = null,
    DateTimeOffset? StartDate = null,
    DateTimeOffset? EndDate = null);
