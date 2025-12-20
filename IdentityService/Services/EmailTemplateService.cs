using System.Text.Json;
using System.Text.RegularExpressions;

namespace IdentityService.Services;

public partial class EmailTemplateService : IEmailTemplateService
{
    private readonly string _templatesPath;
    private readonly ILogger<EmailTemplateService> _logger;
    private readonly string _baseLayout;
    private readonly Dictionary<string, EmailTemplate> _templateCache = new();

    public EmailTemplateService(IWebHostEnvironment env, ILogger<EmailTemplateService> logger)
    {
        _templatesPath = Path.Combine(env.ContentRootPath, "Templates");
        _logger = logger;
        _baseLayout = LoadBaseLayout();
        LoadTemplates();
    }

    public Task<(string Subject, string HtmlBody)> RenderTemplateAsync(string templateName, Dictionary<string, string> variables)
    {
        if (!_templateCache.TryGetValue(templateName, out var template))
        {
            _logger.LogError("Template not found: {TemplateName}", templateName);
            throw new InvalidOperationException($"Template '{templateName}' not found");
        }

        variables["year"] = DateTime.UtcNow.Year.ToString();
        variables["headerGradient"] = template.HeaderGradient;
        variables["buttonColor"] = template.ButtonColor;

        var subject = ReplaceVariables(template.Subject, variables);
        var body = ReplaceVariables(template.Body, variables);
        var htmlBody = _baseLayout
            .Replace("{{subject}}", subject)
            .Replace("{{body}}", body)
            .Replace("{{year}}", variables["year"]);

        return Task.FromResult((subject, htmlBody));
    }

    private string LoadBaseLayout()
    {
        var layoutPath = Path.Combine(_templatesPath, "base-layout.html");
        if (!File.Exists(layoutPath))
        {
            _logger.LogWarning("Base layout not found at {Path}", layoutPath);
            return "{{body}}";
        }
        return File.ReadAllText(layoutPath);
    }

    private void LoadTemplates()
    {
        if (!Directory.Exists(_templatesPath))
        {
            _logger.LogWarning("Templates directory not found: {Path}", _templatesPath);
            return;
        }

        foreach (var file in Directory.GetFiles(_templatesPath, "*.json"))
        {
            try
            {
                var json = File.ReadAllText(file);
                var template = JsonSerializer.Deserialize<EmailTemplate>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (template != null)
                {
                    _templateCache[template.Name] = template;
                    _logger.LogInformation("Loaded email template: {Name}", template.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load template: {File}", file);
            }
        }
    }

    private static string ReplaceVariables(string template, Dictionary<string, string> variables)
    {
        return VariablePattern().Replace(template, match =>
        {
            var variableName = match.Groups[1].Value;
            return variables.TryGetValue(variableName, out var value) ? value : match.Value;
        });
    }

    [GeneratedRegex(@"\{\{(\w+)\}\}")]
    private static partial Regex VariablePattern();

    private sealed class EmailTemplate
    {
        public string Name { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string[] Variables { get; set; } = [];
        public string HeaderGradient { get; set; } = "linear-gradient(135deg, #667eea 0%, #764ba2 100%)";
        public string ButtonColor { get; set; } = "#667eea";
        public string Body { get; set; } = string.Empty;
    }
}
