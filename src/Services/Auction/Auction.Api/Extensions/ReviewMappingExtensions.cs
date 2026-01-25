#nullable enable
using Auctions.Application.DTOs;
using Auctions.Application.DTOs.Reviews;
using Auctions.Domain.Entities;
using AutoMapper;
using BuildingBlocks.Application.Abstractions;

namespace Auctions.Api.Extensions;

public static class ReviewMappingExtensions
{
    public static ReviewDto ToDto(this Review review, IMapper mapper)
    {
        return mapper.Map<ReviewDto>(review);
    }

    public static List<ReviewDto> ToDtoList(this IEnumerable<Review> reviews, IMapper mapper)
    {
        return reviews.Select(r => mapper.Map<ReviewDto>(r)).ToList();
    }

    public static PaginatedResult<ReviewDto> ToPaginatedDto(
        this PaginatedResult<Review> result,
        IMapper mapper)
    {
        var dtos = result.Items.ToDtoList(mapper);
        return new PaginatedResult<ReviewDto>(dtos, result.TotalCount, result.Page, result.PageSize);
    }
}
