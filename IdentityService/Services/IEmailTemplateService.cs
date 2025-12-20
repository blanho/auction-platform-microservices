namespace IdentityService.Services;

public interface IEmailTemplateService
{
    Task<(string Subject, string HtmlBody)> RenderTemplateAsync(string templateName, Dictionary<string, string> variables);
}
