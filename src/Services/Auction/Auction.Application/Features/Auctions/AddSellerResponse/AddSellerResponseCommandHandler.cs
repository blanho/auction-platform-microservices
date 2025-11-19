using Auctions.Application.DTOs;
using AutoMapper;
namespace Auctions.Application.Commands.AddSellerResponse;

public class AddSellerResponseCommandHandler : ICommandHandler<AddSellerResponseCommand, ReviewDto>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTime;

    public AddSellerResponseCommandHandler(
        IReviewRepository reviewRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDateTimeProvider dateTime)
    {
        _reviewRepository = reviewRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _dateTime = dateTime;
    }

    public async Task<Result<ReviewDto>> Handle(AddSellerResponseCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var review = await _reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken);

            if (review.ReviewedUsername != request.SellerUsername)
                return Result.Failure<ReviewDto>(Error.Create("Review.Forbidden", "Only the reviewed seller can respond to this review"));

            if (!string.IsNullOrEmpty(review.SellerResponse))
                return Result.Failure<ReviewDto>(Error.Create("Review.AlreadyResponded", "Seller has already responded to this review"));

            review.SellerResponse = request.Response;
            review.SellerResponseAt = _dateTime.UtcNow;

            await _reviewRepository.UpdateAsync(review, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(_mapper.Map<ReviewDto>(review));
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure<ReviewDto>(Error.Create("Review.NotFound", "Review not found"));
        }
    }
}

