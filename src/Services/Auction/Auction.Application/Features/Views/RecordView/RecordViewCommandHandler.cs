using Auctions.Application.Interfaces;

namespace Auctions.Application.Features.Views.RecordView;

public class RecordViewCommandHandler : ICommandHandler<RecordViewCommand, bool>
{
    private readonly IAuctionViewRepository _viewRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RecordViewCommandHandler> _logger;

    public RecordViewCommandHandler(
        IAuctionViewRepository viewRepository,
        IUnitOfWork unitOfWork,
        ILogger<RecordViewCommandHandler> logger)
    {
        _viewRepository = viewRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(RecordViewCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recording view for auction {AuctionId}", request.AuctionId);

        await _viewRepository.RecordViewAsync(request.AuctionId, request.UserId, request.IpAddress, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
