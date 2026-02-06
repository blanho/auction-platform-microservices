using Notification.Application.DTOs;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using IUnitOfWork = Notification.Application.Interfaces.IUnitOfWork;

namespace Notification.Application.Features.Preferences.UpdatePreferences;

public class UpdatePreferencesCommandHandler : ICommandHandler<UpdatePreferencesCommand, NotificationPreferenceDto>
{
    private readonly INotificationPreferenceRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdatePreferencesCommandHandler> _logger;

    public UpdatePreferencesCommandHandler(
        INotificationPreferenceRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UpdatePreferencesCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<NotificationPreferenceDto>> Handle(UpdatePreferencesCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating preferences for user {UserId}", request.UserId);

        var preference = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (preference == null)
        {
            preference = NotificationPreference.CreateDefault(request.UserId);
            await _repository.CreateAsync(preference, cancellationToken);
        }

        preference.Update(
            request.EmailEnabled,
            request.PushEnabled,
            request.BidUpdates,
            request.AuctionUpdates,
            request.PromotionalEmails,
            request.SystemAlerts);

        await _repository.UpdateAsync(preference, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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

        _logger.LogInformation("Updated preferences for user {UserId}", request.UserId);

        return Result.Success(dto);
    }
}
