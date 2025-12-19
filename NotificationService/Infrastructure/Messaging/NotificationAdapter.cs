using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Application.UseCases.SendNotification;
using NotificationService.Domain.Enums;

namespace NotificationService.Infrastructure.Messaging;

public static class NotificationAdapter
{
    public static SendNotificationRequest ToRequest(
        CreateNotificationDto dto,
        string? recipientEmail = null,
        string? recipientPhone = null,
        ChannelType? channels = null)
    {
        var templateData = new Dictionary<string, object>();
        
        if (!string.IsNullOrEmpty(dto.Data))
        {
            try
            {
                var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(dto.Data);
                if (data != null)
                {
                    foreach (var kvp in data)
                    {
                        templateData[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch
            {
            }
        }

        templateData["title"] = dto.Title;
        templateData["message"] = dto.Message;

        return new SendNotificationRequest
        {
            RecipientId = dto.UserId,
            RecipientUsername = dto.UserId,
            RecipientEmail = recipientEmail,
            RecipientPhone = recipientPhone,
            NotificationType = dto.Type,
            Channels = channels,
            TemplateData = templateData,
            AuctionId = dto.AuctionId,
            BidId = dto.BidId,
            ReferenceId = dto.AuctionId?.ToString() ?? dto.BidId?.ToString()
        };
    }

    public static NotificationDto ToDto(SendNotificationResponse response, SendNotificationRequest request)
    {
        return new NotificationDto
        {
            Id = response.NotificationId,
            UserId = request.RecipientId,
            Type = request.NotificationType.ToString(),
            Title = request.TemplateData.TryGetValue("title", out var title) ? title?.ToString() ?? "" : "",
            Message = request.TemplateData.TryGetValue("message", out var message) ? message?.ToString() ?? "" : "",
            AuctionId = request.AuctionId,
            BidId = request.BidId,
            CreatedAt = DateTimeOffset.UtcNow,
            Status = "Sent"
        };
    }
}
