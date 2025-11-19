using Auctions.Application.DTOs;
namespace Auctions.Application.Queries.GetReviewsForUser;

public record GetReviewsForUserQuery(string Username) : IQuery<List<ReviewDto>>;

