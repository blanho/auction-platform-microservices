using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
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
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value
                ?? User.FindFirst("username")?.Value
                ?? throw new UnauthorizedAccessException("User ID not found in token");
        }

        [HttpGet]
        public async Task<ActionResult<List<NotificationDto>>> GetMyNotifications()
        {
            var userId = GetUserId();
            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            return Ok(notifications);
        }

        [HttpGet("summary")]
        public async Task<ActionResult<NotificationSummaryDto>> GetSummary()
        {
            var userId = GetUserId();
            var summary = await _notificationService.GetNotificationSummaryAsync(userId);
            return Ok(summary);
        }

        [HttpGet("unread")]
        public async Task<ActionResult<List<NotificationDto>>> GetUnreadNotifications()
        {
            var userId = GetUserId();
            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            var unread = notifications.Where(n => n.Status == "Unread").ToList();
            return Ok(unread);
        }

        [HttpPut("{id}/read")]
        public async Task<ActionResult> MarkAsRead(Guid id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return NoContent();
        }

        [HttpPut("read-all")]
        public async Task<ActionResult> MarkAllAsRead()
        {
            var userId = GetUserId();
            await _notificationService.MarkAllAsReadAsync(userId);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            await _notificationService.DeleteNotificationAsync(id);
            return NoContent();
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<NotificationDto>> Create([FromBody] CreateNotificationDto dto)
        {
            var notification = await _notificationService.CreateNotificationAsync(dto);
            return CreatedAtAction(nameof(GetMyNotifications), new { id = notification.Id }, notification);
        }

        [HttpGet("admin/all")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<PagedNotificationsDto>> GetAllNotifications(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? userId = null,
            [FromQuery] string? type = null,
            [FromQuery] string? status = null)
        {
            var notifications = await _notificationService.GetAllNotificationsAsync(
                pageNumber, pageSize, userId, type, status);
            return Ok(notifications);
        }

        [HttpPost("admin/broadcast")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> BroadcastNotification([FromBody] BroadcastNotificationDto dto)
        {
            await _notificationService.BroadcastNotificationAsync(dto);
            return Ok(new { message = "Notification broadcast successfully" });
        }

        [HttpGet("admin/stats")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<NotificationStatsDto>> GetNotificationStats()
        {
            var stats = await _notificationService.GetNotificationStatsAsync();
            return Ok(stats);
        }
    }
}
