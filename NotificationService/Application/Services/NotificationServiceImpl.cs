using AutoMapper;
using Common.Core.Interfaces;
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
        private readonly IDateTimeProvider _dateTime;

        public NotificationServiceImpl(
            INotificationRepository repository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            INotificationHubService hubService,
            IDateTimeProvider dateTime)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hubService = hubService;
            _dateTime = dateTime;
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

        public async Task<PagedNotificationsDto> GetAllNotificationsAsync(
            int pageNumber,
            int pageSize,
            string? userId,
            string? type,
            string? status,
            CancellationToken cancellationToken = default)
        {
            var allNotifications = await _repository.GetAllAsync(cancellationToken);

            var query = allNotifications.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(n => n.UserId == userId);

            if (!string.IsNullOrEmpty(type) && Enum.TryParse<NotificationType>(type, true, out var notificationType))
                query = query.Where(n => n.Type == notificationType);

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<NotificationStatus>(status, true, out var notificationStatus))
                query = query.Where(n => n.Status == notificationStatus);

            var totalCount = query.Count();
            var items = query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedNotificationsDto
            {
                Items = _mapper.Map<List<NotificationDto>>(items),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task BroadcastNotificationAsync(BroadcastNotificationDto dto, CancellationToken cancellationToken = default)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = "broadcast",
                Type = dto.Type,
                Title = dto.Title,
                Message = dto.Message,
                Status = NotificationStatus.Unread,
                Data = dto.TargetRole ?? "all"
            };

            await _repository.CreateAsync(notification, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var notificationDto = _mapper.Map<NotificationDto>(notification);
            await _hubService.SendNotificationToAllAsync(notificationDto);
        }

        public async Task<NotificationStatsDto> GetNotificationStatsAsync(CancellationToken cancellationToken = default)
        {
            var allNotifications = await _repository.GetAllAsync(cancellationToken);
            var today = _dateTime.UtcNow.Date;

            var byType = allNotifications
                .GroupBy(n => n.Type.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            return new NotificationStatsDto
            {
                TotalNotifications = allNotifications.Count,
                UnreadNotifications = allNotifications.Count(n => n.Status == NotificationStatus.Unread),
                TodayCount = allNotifications.Count(n => n.CreatedAt.Date == today),
                ByType = byType
            };
        }
    }
}
