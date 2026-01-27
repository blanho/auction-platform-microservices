using Auctions.Application.Errors;
using Auctions.Application.DTOs;
using Auctions.Application.Interfaces;
using Auctions.Domain.Entities;
using AutoMapper;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS;
using BuildingBlocks.Infrastructure.Repository;

namespace Auctions.Application.Features.Reviews.CreateReview;

public class CreateReviewCommandHandler : ICommandHandler<CreateReviewCommand, ReviewDto>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IAuctionRepository _auctionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ISanitizationService _sanitizationService;

    public CreateReviewCommandHandler(
        IReviewRepository reviewRepository,
        IAuctionRepository auctionRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ISanitizationService sanitizationService)
    {
        _reviewRepository = reviewRepository;
        _auctionRepository = auctionRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _sanitizationService = sanitizationService;
    }

    public async Task<Result<ReviewDto>> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var existingReview = await _reviewRepository.GetByAuctionAndReviewerAsync(
            request.AuctionId, request.ReviewerUsername, cancellationToken);
        
        if (existingReview != null)
            return Result.Failure<ReviewDto>(AuctionErrors.Review.AlreadyExists);

        var auction = await _auctionRepository.GetByIdAsync(request.AuctionId, cancellationToken);
        if (auction == null)
        {
            return Result.Failure<ReviewDto>(AuctionErrors.Auction.NotFoundById(request.AuctionId));
        }

        if (request.Rating < 1 || request.Rating > 5)
            return Result.Failure<ReviewDto>(AuctionErrors.Review.InvalidRating);

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
            Title = _sanitizationService.SanitizeText(request.Title),
            Comment = _sanitizationService.SanitizeHtml(request.Comment)
        };

        await _reviewRepository.CreateAsync(review, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(_mapper.Map<ReviewDto>(review));
    }
}

