using Auctions.Application.DTOs;
namespace Auctions.Application.Queries.GetReviewsByUser;

public record GetReviewsByUserQuery(string Username) : IQuery<List<ReviewDto>>;

