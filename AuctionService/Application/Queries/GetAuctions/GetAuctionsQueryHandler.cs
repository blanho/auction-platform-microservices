using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using AutoMapper;
using Common.Core.Helpers;
using Common.Repository.Interfaces;
using Common.CQRS.Abstractions;
using Common.Domain.Enums;

namespace AuctionService.Application.Queries.GetAuctions;

public class GetAuctionsQueryHandler : IQueryHandler<GetAuctionsQuery, PagedResult<AuctionDto>>
{
    private readonly IAuctionRepository _repository;
    private readonly IMapper _mapper;
    private readonly IAppLogger<GetAuctionsQueryHandler> _logger;

    public GetAuctionsQueryHandler(
        IAuctionRepository repository,
        IMapper mapper,
        IAppLogger<GetAuctionsQueryHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PagedResult<AuctionDto>>> Handle(GetAuctionsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching auctions with filters - Status: {Status}, Seller: {Seller}, Page: {Page}",
            request.Status ?? "All", request.Seller ?? "All", request.PageNumber);

        try
        {
            var allAuctions = await _repository.GetAllAsync(cancellationToken);

            var query = allAuctions.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<Status>(request.Status, true, out var status))
            {
                query = query.Where(a => a.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(request.Seller))
            {
                query = query.Where(a => a.Seller.Contains(request.Seller, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(request.Winner))
            {
                query = query.Where(a => a.Winner != null && a.Winner.Contains(request.Winner, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(a =>
                    a.Item.Title.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    a.Item.Make.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                    a.Item.Model.Contains(request.SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            query = ApplyOrdering(query, request.OrderBy, request.Descending);

            var totalCount = query.Count();

            var items = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var dtos = _mapper.Map<List<AuctionDto>>(items);

            var result = PagedResult<AuctionDto>.Create(dtos, totalCount, request.PageNumber, request.PageSize);

            _logger.LogInformation("Retrieved {Count} auctions out of {Total}", dtos.Count, totalCount);

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to fetch auctions: {Error}", ex.Message);
            return Result.Failure<PagedResult<AuctionDto>>(Error.Create("Auction.FetchFailed", $"Failed to fetch auctions: {ex.Message}"));
        }
    }

    private static IQueryable<Domain.Entities.Auction> ApplyOrdering(
        IQueryable<Domain.Entities.Auction> query, 
        string? orderBy, 
        bool descending)
    {
        return orderBy?.ToLowerInvariant() switch
        {
            "title" => descending ? query.OrderByDescending(a => a.Item.Title) : query.OrderBy(a => a.Item.Title),
            "make" => descending ? query.OrderByDescending(a => a.Item.Make) : query.OrderBy(a => a.Item.Make),
            "year" => descending ? query.OrderByDescending(a => a.Item.Year) : query.OrderBy(a => a.Item.Year),
            "auctionend" => descending ? query.OrderByDescending(a => a.AuctionEnd) : query.OrderBy(a => a.AuctionEnd),
            "createdat" => descending ? query.OrderByDescending(a => a.CreatedAt) : query.OrderBy(a => a.CreatedAt),
            "currenthighbid" => descending ? query.OrderByDescending(a => a.CurrentHighBid) : query.OrderBy(a => a.CurrentHighBid),
            _ => descending ? query.OrderByDescending(a => a.AuctionEnd) : query.OrderBy(a => a.AuctionEnd)
        };
    }
}
