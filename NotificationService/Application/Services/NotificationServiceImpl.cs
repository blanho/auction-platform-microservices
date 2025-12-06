using AutoMapper;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;

namespace NotificationService.Application.Services
{
    public class NotificationServiceImpl : INotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly INotificationHubService _hubService;

        public NotificationServiceImpl(
            INotificationRepository repository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            INotificationHubService hubService)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hubService = hubService;
        }

        public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default)
        {
            var notification = _mapper.Map<Notification>(dto);
            notification.Status = NotificationStatus.Unread;

            await _repository.CreateAsync(notification, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var notificationDto = _mapper.Map<NotificationDto>(notification);

            // Send real-time notification via SignalR
            await _hubService.SendNotificationToUserAsync(dto.UserId, notificationDto);

            return notificationDto;
        }

        public async Task<List<NotificationDto>> GetUserNotificationsAsync(string userId, CancellationToken cancellationToken = default)
        {
            var notifications = await _repository.GetByUserIdAsync(userId, cancellationToken);
            return _mapper.Map<List<NotificationDto>>(notifications);
        }

        public async Task<NotificationSummaryDto> GetNotificationSummaryAsync(string userId, CancellationToken cancellationToken = default)
        {
            var allNotifications = await _repository.GetByUserIdAsync(userId, cancellationToken);
            var unreadCount = await _repository.GetUnreadCountByUserIdAsync(userId, cancellationToken);

            return new NotificationSummaryDto
            {
                UnreadCount = unreadCount,
                TotalCount = allNotifications.Count,
                RecentNotifications = _mapper.Map<List<NotificationDto>>(allNotifications.Take(10).ToList())
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
    }
}
