using Notification.Application.DTOs;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using IUnitOfWork = Notification.Application.Interfaces.IUnitOfWork;

namespace Notification.Application.Features.Preferences.GetPreferences;

public class GetPreferencesQueryHandler : IQueryHandler<GetPreferencesQuery, NotificationPreferenceDto>
{
    private readonly INotificationPreferenceRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetPreferencesQueryHandler> _logger;

    public GetPreferencesQueryHandler(
        INotificationPreferenceRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<GetPreferencesQueryHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<NotificationPreferenceDto>> Handle(GetPreferencesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting preferences for user {UserId}", request.UserId);

        var preference = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (preference == null)
        {
            preference = NotificationPreference.CreateDefault(request.UserId);
            await _repository.CreateAsync(preference, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var dto = new NotificationPreferenceDto
        {
            Id = preference.Id,
            UserId = preference.UserId,
            EmailEnabled = preference.EmailEnabled,
            PushEnabled = preference.PushEnabled,
            BidUpdates = preference.BidUpdates,
            AuctionUpdates = preference.AuctionUpdates,
            PromotionalEmails = preference.PromotionalEmails,
            SystemAlerts = preference.SystemAlerts
        };

        return Result.Success(dto);
    }
}
