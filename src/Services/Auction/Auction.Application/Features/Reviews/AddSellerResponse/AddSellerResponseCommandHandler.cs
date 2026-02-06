using Auctions.Application.DTOs;
using Auctions.Application.Interfaces;
using Auctions.Application.Errors;
using AutoMapper;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.CQRS;
// using BuildingBlocks.Infrastructure.Repository; // Use BuildingBlocks.Application.Abstractions instead

namespace Auctions.Application.Features.Reviews.AddSellerResponse;

public class AddSellerResponseCommandHandler : ICommandHandler<AddSellerResponseCommand, ReviewDto>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ISanitizationService _sanitizationService;

    public AddSellerResponseCommandHandler(
        IReviewRepository reviewRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ISanitizationService sanitizationService)
    {
        _reviewRepository = reviewRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _sanitizationService = sanitizationService;
    }

    public async Task<Result<ReviewDto>> Handle(AddSellerResponseCommand request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken);

        if (review == null)
        {
            return Result.Failure<ReviewDto>(AuctionErrors.Review.NotFound);
        }

        if (review.ReviewedUsername != request.SellerUsername)
        {
            return Result.Failure<ReviewDto>(AuctionErrors.Review.Forbidden);
        }

        var sanitizedResponse = _sanitizationService.SanitizeHtml(request.Response);
        review.AddSellerResponse(sanitizedResponse);

        await _reviewRepository.UpdateAsync(review, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(_mapper.Map<ReviewDto>(review));
    }
}
