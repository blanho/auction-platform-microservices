using AuctionService.Application.DTOs;
using AuctionService.Application.Interfaces;
using AuctionService.Domain.Entities;
using AutoMapper;
using Common.Core.Helpers;
using Common.CQRS.Abstractions;

namespace AuctionService.Application.Commands.CreateReview;

public class CreateReviewCommandHandler : ICommandHandler<CreateReviewCommand, ReviewDto>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IAuctionRepository _auctionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateReviewCommandHandler(
        IReviewRepository reviewRepository,
        IAuctionRepository auctionRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _auctionRepository = auctionRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ReviewDto>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var existingReview = await _reviewRepository.GetByAuctionAndReviewerAsync(
            request.AuctionId, request.ReviewerUsername, cancellationToken);
        
        if (existingReview != null)
            return Result.Failure<ReviewDto>(Error.Create("Review.AlreadyExists", "Review already exists for this auction"));

        Auction auction;
        try
        {
            auction = await _auctionRepository.GetByIdAsync(request.AuctionId, cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure<ReviewDto>(Error.Create("Auction.NotFound", "Auction not found"));
        }

        if (request.Rating < 1 || request.Rating > 5)
            return Result.Failure<ReviewDto>(Error.Create("Review.InvalidRating", "Rating must be between 1 and 5"));

        var review = new Review
        {
            Id = Guid.NewGuid(),
            AuctionId = request.AuctionId,
            Auction = auction,
            OrderId = request.OrderId,
            ReviewerId = request.ReviewerId,
            ReviewerUsername = request.ReviewerUsername,
            ReviewedUserId = request.ReviewedUserId,
            ReviewedUsername = request.ReviewedUsername,
            Rating = request.Rating,
            Title = request.Title,
            Comment = request.Comment
        };

        await _reviewRepository.CreateAsync(review, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(_mapper.Map<ReviewDto>(review));
    }
}
