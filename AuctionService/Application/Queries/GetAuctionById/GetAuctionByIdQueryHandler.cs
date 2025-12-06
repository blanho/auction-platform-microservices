using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using AutoMapper;
using Common.Core.Helpers;
using Common.Repository.Interfaces;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Queries.GetAuctionById;

public class GetAuctionByIdQueryHandler : IQueryHandler<GetAuctionByIdQuery, AuctionDto>
{
    private readonly IAuctionRepository _repository;
    private readonly IMapper _mapper;
    private readonly IAppLogger<GetAuctionByIdQueryHandler> _logger;

    public GetAuctionByIdQueryHandler(
        IAuctionRepository repository,
        IMapper mapper,
        IAppLogger<GetAuctionByIdQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<AuctionDto>> Handle(GetAuctionByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching auction {AuctionId}", request.Id);

        try
        {
            var auction = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (auction == null)
            {
                _logger.LogWarning("Auction {AuctionId} not found", request.Id);
                return Result.Failure<AuctionDto>(Error.Create("Auction.NotFound", $"Auction with ID {request.Id} was not found"));
            }

            var dto = _mapper.Map<AuctionDto>(auction);
            return Result.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to fetch auction {AuctionId}: {Error}", request.Id, ex.Message);
            return Result.Failure<AuctionDto>(Error.Create("Auction.FetchFailed", $"Failed to fetch auction: {ex.Message}"));
        }
    }
}
