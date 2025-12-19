using NotificationService.Application.Ports;
using NotificationService.Domain.Enums;

namespace NotificationService.Application.UseCases.GetNotifications;

public record GetNotificationsRequest
{
    public required string UserId { get; init; }
    public NotificationStatus? Status { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public record GetNotificationsResponse
{
    public List<NotificationDto> Notifications { get; init; } = new();
    public int TotalCount { get; init; }
    public int UnreadCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}

public interface IGetNotificationsUseCase
{
    Task<GetNotificationsResponse> ExecuteAsync(
        GetNotificationsRequest request,
        CancellationToken cancellationToken = default);
}

public class GetNotificationsUseCase : IGetNotificationsUseCase
{
    private readonly INotificationRepository _repository;

    public GetNotificationsUseCase(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetNotificationsResponse> ExecuteAsync(
        GetNotificationsRequest request,
        CancellationToken cancellationToken = default)
    {
        var (notifications, totalCount) = await _repository.GetPagedAsync(
            request.UserId,
            request.Status,
            request.Page,
            request.PageSize,
            cancellationToken);

        var unreadCount = await _repository.GetUnreadCountAsync(request.UserId, cancellationToken);

        var dtos = notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            UserId = n.UserId,
            Username = n.Username,
            Type = n.Type.ToString(),
            Title = n.Title,
            Message = n.Message,
            HtmlContent = n.HtmlContent,
            Data = n.Data,
            Status = n.Status.ToString(),
            Channels = n.Channels.ToString(),
            AuctionId = n.AuctionId,
            BidId = n.BidId,
            OrderId = n.OrderId,
            CreatedAt = n.CreatedAt,
            ReadAt = n.ReadAt
        }).ToList();

        return new GetNotificationsResponse
        {
            Notifications = dtos,
            TotalCount = totalCount,
            UnreadCount = unreadCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
    }
}

public record NotificationDto
{
    public Guid Id { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string? Username { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? HtmlContent { get; init; }
    public string? Data { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? Channels { get; init; }
    public Guid? AuctionId { get; init; }
    public Guid? BidId { get; init; }
    public Guid? OrderId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ReadAt { get; init; }
}
