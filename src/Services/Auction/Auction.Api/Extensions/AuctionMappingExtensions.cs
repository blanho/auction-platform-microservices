#nullable enable
using Auctions.Application.DTOs;
using Auctions.Application.DTOs.Auctions;
using Auctions.Domain.Entities;
using AutoMapper;
using BuildingBlocks.Application.Abstractions;

namespace Auctions.Api.Extensions;

public static class AuctionMappingExtensions
{
    public static AuctionDto ToDto(this Auction auction, IMapper mapper)
    {
        return mapper.Map<AuctionDto>(auction);
    }

    public static List<AuctionDto> ToDtoList(this IEnumerable<Auction> auctions, IMapper mapper)
    {
        return auctions.Select(a => mapper.Map<AuctionDto>(a)).ToList();
    }

    public static PaginatedResult<AuctionDto> ToPaginatedDto(
        this PaginatedResult<Auction> result,
        IMapper mapper)
    {
        var dtos = result.Items.ToDtoList(mapper);
        return new PaginatedResult<AuctionDto>(dtos, result.TotalCount, result.Page, result.PageSize);
    }

    public static PaginatedResult<AuctionDto> ToPaginatedDto(
        this (List<Auction> Items, int TotalCount) result,
        int page,
        int pageSize,
        IMapper mapper)
    {
        var dtos = result.Items.ToDtoList(mapper);
        return new PaginatedResult<AuctionDto>(dtos, result.TotalCount, page, pageSize);
    }
}
