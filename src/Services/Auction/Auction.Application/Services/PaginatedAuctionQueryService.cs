using Auctions.Application.DTOs;
using AutoMapper;
using Auctions.Domain.Entities;

namespace Auctions.Application.Services;

public interface IPaginatedAuctionQueryService
{
    Task<PaginatedResult<AuctionDto>> GetPagedAuctionsAsync(
        AuctionFilterDto queryParams,
        CancellationToken cancellationToken);
}

public class PaginatedAuctionQueryService : IPaginatedAuctionQueryService
{
    private readonly IAuctionReadRepository _repository;
    private readonly IMapper _mapper;

    public PaginatedAuctionQueryService(
        IAuctionReadRepository repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedResult<AuctionDto>> GetPagedAuctionsAsync(
        AuctionFilterDto queryParams,
        CancellationToken cancellationToken)
    {
        var result = await _repository.GetPagedAsync(queryParams, cancellationToken);
        var dtos = result.Items.Select(auction => _mapper.Map<AuctionDto>(auction)).ToList();
        return new PaginatedResult<AuctionDto>(dtos, result.TotalCount, queryParams.Page, queryParams.PageSize);
    }
}
