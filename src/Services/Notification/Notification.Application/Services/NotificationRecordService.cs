using BuildingBlocks.Application.Abstractions;
using Notification.Application.DTOs;
using Notification.Application.Filtering;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;

namespace Notification.Application.Services;

public class NotificationRecordService : INotificationRecordService
{
    private readonly INotificationRecordRepository _repository;

    public NotificationRecordService(INotificationRecordRepository repository)
    {
        _repository = repository;
    }

    public async Task<NotificationRecordDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var record = await _repository.GetByIdAsync(id, ct);
        return record?.ToDto();
    }

    public async Task<PaginatedResult<NotificationRecordDto>> GetPagedAsync(NotificationRecordFilterDto filter, CancellationToken ct = default)
    {
        var result = await _repository.GetPagedAsync(filter, ct);

        return new PaginatedResult<NotificationRecordDto>(
            result.Items.Select(r => r.ToDto()).ToList(),
            result.TotalCount,
            result.Page,
            result.PageSize);
    }

    public async Task<PaginatedResult<NotificationRecordDto>> GetByUserIdAsync(NotificationRecordQueryParams queryParams, CancellationToken ct = default)
    {
        var result = await _repository.GetRecordsByUserIdAsync(queryParams, ct);
        
        return new PaginatedResult<NotificationRecordDto>(
            result.Items.Select(r => r.ToDto()).ToList(),
            result.TotalCount,
            result.Page,
            result.PageSize);
    }

    public async Task<NotificationRecordStatsDto> GetStatsAsync(CancellationToken ct = default)
    {
        var total = await _repository.GetTotalCountAsync(ct);
        var sent = await _repository.GetCountByStatusAsync(NotificationRecordStatus.Sent, ct);
        var failed = await _repository.GetCountByStatusAsync(NotificationRecordStatus.Failed, ct);
        var pending = await _repository.GetCountByStatusAsync(NotificationRecordStatus.Pending, ct);
        var byChannel = await _repository.GetCountByChannelAsync(ct);
        var byTemplate = await _repository.GetCountByTemplateAsync(ct);

        return new NotificationRecordStatsDto
        {
            TotalRecords = total,
            SentCount = sent,
            FailedCount = failed,
            PendingCount = pending,
            ByChannel = byChannel,
            ByTemplate = byTemplate
        };
    }
}

internal static class NotificationRecordExtensions
{
    public static NotificationRecordDto ToDto(this NotificationRecord record) => new()
    {
        Id = record.Id,
        UserId = record.UserId,
        TemplateKey = record.TemplateKey,
        Channel = record.Channel,
        Subject = record.Subject,
        Recipient = record.Recipient,
        Status = record.Status.ToString(),
        ErrorMessage = record.ErrorMessage,
        SentAt = record.SentAt,
        ExternalId = record.ExternalId,
        CreatedAt = record.CreatedAt
    };
}
