using AutoMapper;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Providers;
using Notification.Application.DTOs;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;
using Notification.Domain.Enums;
using NotificationEntity = Notification.Domain.Entities.Notification;

namespace Notification.Application.Services
{
    public class NotificationServiceImpl : INotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly INotificationPreferenceRepository _preferenceRepository;
        private readonly Interfaces.IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationHubService _hubService;
        private readonly IDateTimeProvider _dateTime;

        public NotificationServiceImpl(
            INotificationRepository repository,
            INotificationPreferenceRepository preferenceRepository,
            Interfaces.IUnitOfWork unitOfWork,
            IMapper mapper,
            INotificationHubService hubService,
            IDateTimeProvider dateTime)
        {
            _repository = repository;
            _preferenceRepository = preferenceRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hubService = hubService;
            _dateTime = dateTime;
        }

        public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default)
        {
            var notification = NotificationEntity.Create(
                dto.UserId,
                dto.UserId,
                dto.Type,
                dto.Title,
                dto.Message,
                ChannelType.InApp,
                dto.Data,
                null,
                dto.AuctionId,
                dto.BidId,
                null,
                null);

            notification.MarkAsSent();

            await _repository.CreateAsync(notification, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var notificationDto = notification.ToDto(_mapper);
            await _hubService.SendNotificationToUserAsync(dto.UserId, notificationDto);

            return notificationDto;
        }

        public async Task<List<NotificationDto>> GetUserNotificationsAsync(string userId, CancellationToken cancellationToken = default)
        {
            var notifications = await _repository.GetByUserIdAsync(userId, cancellationToken);
            return notifications.ToDtoList(_mapper);
        }

        public async Task<NotificationSummaryDto> GetNotificationSummaryAsync(string userId, CancellationToken cancellationToken = default)
        {
            var allNotifications = await _repository.GetByUserIdAsync(userId, cancellationToken);
            var unreadCount = await _repository.GetUnreadCountByUserIdAsync(userId, cancellationToken);

            return new NotificationSummaryDto
            {
                UnreadCount = unreadCount,
                TotalCount = allNotifications.Count,
                RecentNotifications = allNotifications.Take(10).ToList().ToDtoList(_mapper)
            };
        }

        public async Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
        {
            await _repository.MarkAsReadAsync(notificationId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default)
        {
            await _repository.MarkAllAsReadAsync(userId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteNotificationAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await _repository.DeleteAsync(id, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task ArchiveNotificationAsync(Guid notificationId, CancellationToken cancellationToken = default)
        {
            await _repository.ArchiveAsync(notificationId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<PaginatedResult<NotificationDto>> GetAllNotificationsAsync(
            int page,
            int pageSize,
            string? userId,
            string? type,
            string? status,
            CancellationToken cancellationToken = default)
        {
            NotificationType? typeFilter = null;
            NotificationStatus? statusFilter = null;

            if (!string.IsNullOrEmpty(type) && Enum.TryParse<NotificationType>(type, true, out var parsedType))
                typeFilter = parsedType;

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<NotificationStatus>(status, true, out var parsedStatus))
                statusFilter = parsedStatus;

            var result = await _repository.GetPaginatedAsync(
                page,
                pageSize,
                userId,
                typeFilter,
                statusFilter,
                cancellationToken);

            return new PaginatedResult<NotificationDto>(
                result.Items.ToDtoList(_mapper),
                result.TotalCount,
                result.Page,
                result.PageSize
            );
        }

        public async Task BroadcastNotificationAsync(BroadcastNotificationDto dto, CancellationToken cancellationToken = default)
        {
            var notification = NotificationEntity.Create(
                "broadcast",
                "broadcast",
                dto.Type,
                dto.Title,
                dto.Message,
                ChannelType.InApp,
                dto.TargetRole ?? "all");

            notification.MarkAsSent();

            await _repository.CreateAsync(notification, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var notificationDto = notification.ToDto(_mapper);
            await _hubService.SendNotificationToAllAsync(notificationDto);
        }

        public async Task<NotificationStatsDto> GetNotificationStatsAsync(CancellationToken cancellationToken = default)
        {
            var stats = await _repository.GetStatsAsync(cancellationToken);

            return new NotificationStatsDto
            {
                TotalNotifications = stats.TotalCount,
                UnreadNotifications = stats.UnreadCount,
                TodayCount = stats.TodayCount,
                ByType = stats.ByType
            };
        }

        public async Task<NotificationPreferenceDto> GetPreferencesAsync(string userId, CancellationToken cancellationToken = default)
        {
            var preference = await _preferenceRepository.GetByUserIdAsync(userId, cancellationToken);

            if (preference == null)
            {
                preference = NotificationPreference.CreateDefault(userId);
                await _preferenceRepository.CreateAsync(preference, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return new NotificationPreferenceDto
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
        }

        public async Task<NotificationPreferenceDto> UpdatePreferencesAsync(string userId, UpdateNotificationPreferenceDto dto, CancellationToken cancellationToken = default)
        {
            var preference = await _preferenceRepository.GetByUserIdAsync(userId, cancellationToken);

            if (preference == null)
            {
                preference = NotificationPreference.CreateDefault(userId);
                await _preferenceRepository.CreateAsync(preference, cancellationToken);
            }

            preference.Update(
                dto.EmailEnabled,
                dto.PushEnabled,
                dto.BidUpdates,
                dto.AuctionUpdates,
                dto.PromotionalEmails,
                dto.SystemAlerts);

            await _preferenceRepository.UpdateAsync(preference, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new NotificationPreferenceDto
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
        }
    }
}
