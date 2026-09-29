using JobService.Contracts.Commands;
using AuctionService.Contracts.Commands;
using Auctions.Application.Errors;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Messaging;
using BuildingBlocks.Application.Constants;
using BuildingBlocks.Application.CQRS;
using MassTransit;

namespace Auctions.Application.Features.Auctions.QueueAuctionExport;

public class QueueAuctionExportCommandHandler : ICommandHandler<QueueAuctionExportCommand, BackgroundJobResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _publishEndpoint;
    private readonly ILogger<QueueAuctionExportCommandHandler> _logger;

    public QueueAuctionExportCommandHandler(
        IUnitOfWork unitOfWork,
        IEventPublisher publishEndpoint,
        ILogger<QueueAuctionExportCommandHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BackgroundJobResult>> Handle(
        QueueAuctionExportCommand request,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(request.Format))
        {
            return Result.Failure<BackgroundJobResult>(AuctionErrors.Export.UnsupportedFormat(request.Format.ToString()));
        }

        var correlationId = Guid.NewGuid();
        await _publishEndpoint.PublishAsync(new RequestJobCommand
        {
            CorrelationId = correlationId.ToString(),
            JobType = "DataExport",
            RequestedBy = request.RequestedBy,
            TotalItems = 1,
            MaxRetryCount = 0,
            PayloadJson = "{}"
        }, cancellationToken);

        var command = new ProcessAuctionExportCommand
        {
            CorrelationId = correlationId,
            RequestedBy = request.RequestedBy,
            Format = request.Format.ToString(),
            StatusFilter = request.StatusFilter?.ToString(),
            SellerFilter = request.SellerFilter,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            RequestedAt = DateTimeOffset.UtcNow
        };

        await _publishEndpoint.PublishAsync(command, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Queued auction export job {CorrelationId} in {Format} format for user {RequestedBy}",
            correlationId, request.Format, request.RequestedBy);

        return Result<BackgroundJobResult>.Success(new BackgroundJobResult(
            JobId: correlationId,
            CorrelationId: correlationId.ToString(),
            Status: BackgroundJobStatuses.Queued,
            Message: $"Export in {request.Format} format has been queued for background processing."));
    }
}
