using Common.Core.Authorization;
using Common.Core.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Enums;
using System.Security.Claims;

namespace NotificationService.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            INotificationService notificationService,
            ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        private string GetUserId()
        {
            return User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value
                ?? User.FindFirst("username")?.Value
                ?? throw new UnauthorizedAccessException("User ID not found in token");
        }

        [HttpGet]
        [HasPermission(Permissions.Notifications.View)]
        public async Task<ActionResult<List<NotificationDto>>> GetMyNotifications(CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var notifications = await _notificationService.GetUserNotificationsAsync(userId, cancellationToken);
            return Ok(notifications);
        }

        [HttpGet("summary")]
        [HasPermission(Permissions.Notifications.View)]
        public async Task<ActionResult<NotificationSummaryDto>> GetSummary(CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var summary = await _notificationService.GetNotificationSummaryAsync(userId, cancellationToken);
            return Ok(summary);
        }

        [HttpGet("unread")]
        [HasPermission(Permissions.Notifications.View)]
        public async Task<ActionResult<List<NotificationDto>>> GetUnreadNotifications(CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var notifications = await _notificationService.GetUserNotificationsAsync(userId, cancellationToken);
            var unread = notifications.Where(n => n.Status == nameof(NotificationStatus.Unread)).ToList();
            return Ok(unread);
        }

        [HttpPut("{id}/read")]
        [HasPermission(Permissions.Notifications.View)]
        public async Task<ActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var notifications = await _notificationService.GetUserNotificationsAsync(userId, cancellationToken);
            var notification = notifications.FirstOrDefault(n => n.Id == id);
            
            if (notification == null)
            {
                return NotFound(new { error = "Notification not found or access denied" });
            }

            await _notificationService.MarkAsReadAsync(id, cancellationToken);
            return NoContent();
        }

        [HttpPut("read-all")]
        [HasPermission(Permissions.Notifications.View)]
        public async Task<ActionResult> MarkAllAsRead(CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            await _notificationService.MarkAllAsReadAsync(userId, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [HasPermission(Permissions.Notifications.View)]
        public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var userId = GetUserId();
            var notifications = await _notificationService.GetUserNotificationsAsync(userId, cancellationToken);
            var notification = notifications.FirstOrDefault(n => n.Id == id);
            
            if (notification == null)
            {
                return NotFound(new { error = "Notification not found or access denied" });
            }

            await _notificationService.DeleteNotificationAsync(id, cancellationToken);
            return NoContent();
        }

        [HttpPost]
        [HasPermission(Permissions.Notifications.Send)]
        public async Task<ActionResult<NotificationDto>> Create([FromBody] CreateNotificationDto dto)
        {
            var notification = await _notificationService.CreateNotificationAsync(dto);
            return CreatedAtAction(nameof(GetMyNotifications), new { id = notification.Id }, notification);
        }

        [HttpGet("admin/all")]
        [HasPermission(Permissions.Notifications.ManageTemplates)]
        public async Task<ActionResult<PagedNotificationsDto>> GetAllNotifications(
            [FromQuery] int pageNumber = PaginationDefaults.DefaultPage,
            [FromQuery] int pageSize = PaginationDefaults.LargePageSize,
            [FromQuery] string? userId = null,
            [FromQuery] string? type = null,
            [FromQuery] string? status = null)
        {
            var notifications = await _notificationService.GetAllNotificationsAsync(
                pageNumber, pageSize, userId, type, status);
            return Ok(notifications);
        }

        [HttpPost("admin/broadcast")]
        [HasPermission(Permissions.Notifications.Send)]
        public async Task<ActionResult> BroadcastNotification([FromBody] BroadcastNotificationDto dto)
        {
            await _notificationService.BroadcastNotificationAsync(dto);
            return Ok(new { message = "Notification broadcast successfully" });
        }

        [HttpGet("admin/stats")]
        [HasPermission(Permissions.Notifications.ManageTemplates)]
        public async Task<ActionResult<NotificationStatsDto>> GetNotificationStats()
        {
            var stats = await _notificationService.GetNotificationStatsAsync();
            return Ok(stats);
        }
    }
}
