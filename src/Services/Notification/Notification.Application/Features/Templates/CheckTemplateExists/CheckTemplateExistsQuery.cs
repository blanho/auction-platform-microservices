namespace Notification.Application.Features.Templates.CheckTemplateExists;

public record CheckTemplateExistsQuery(string Key) : IQuery<bool>;
