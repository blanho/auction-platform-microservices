using BuildingBlocks.Application.Filtering;
using BuildingBlocks.Application.Paging;
using Notification.Domain.Entities;

namespace Notification.Application.Filtering;

public class NotificationRecordFilterCriteria : IFilter<NotificationRecord>
{
    public Guid? UserId { get; set; }
    public string? Channel { get; set; }
    public NotificationRecordStatus? Status { get; set; }
    public string? TemplateKey { get; set; }
    public DateTimeOffset? FromDate { get; set; }
    public DateTimeOffset? ToDate { get; set; }

    public IQueryable<NotificationRecord> Apply(IQueryable<NotificationRecord> query)
    {
        return FilterBuilder<NotificationRecord>.Create()
            .WhenHasValue(UserId, r => r.UserId == UserId!.Value)
            .WhenNotEmpty(Channel, r => r.Channel == Channel)
            .WhenHasValue(Status, r => r.Status == Status!.Value)
            .WhenNotEmpty(TemplateKey, r => r.TemplateKey == TemplateKey)
            .WhenHasValue(FromDate, r => r.CreatedAt >= FromDate!.Value)
            .WhenHasValue(ToDate, r => r.CreatedAt <= ToDate!.Value)
            .Apply(query);
    }
}

public class NotificationRecordQueryParams : QueryParameters<NotificationRecordFilterCriteria>
{
}
