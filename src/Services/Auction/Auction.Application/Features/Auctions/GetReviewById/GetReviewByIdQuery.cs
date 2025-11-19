using Auctions.Application.DTOs;
namespace Auctions.Application.Queries.GetReviewById;

public record GetReviewByIdQuery(Guid Id) : IQuery<ReviewDto>;

