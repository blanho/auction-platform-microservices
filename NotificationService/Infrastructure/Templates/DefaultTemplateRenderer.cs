using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Ports;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Models;

namespace NotificationService.Infrastructure.Templates;

public class DefaultTemplateRenderer : ITemplateRenderer
{
    private readonly ILogger<DefaultTemplateRenderer> _logger;
    private static readonly Regex VariablePattern = new(@"\{\{(\w+)\}\}", RegexOptions.Compiled);
    private static readonly Regex ConditionalPattern = new(@"\{\{#if\s+(\w+)\}\}(.*?)\{\{/if\}\}", 
        RegexOptions.Compiled | RegexOptions.Singleline);

    public ChannelType SupportedChannel => ChannelType.InApp | ChannelType.Email | ChannelType.Sms | ChannelType.Push;

    public DefaultTemplateRenderer(ILogger<DefaultTemplateRenderer> logger)
    {
        _logger = logger;
    }

    public Task<RenderedContent> RenderAsync(
        Template template,
        Dictionary<string, object> data,
        CancellationToken cancellationToken = default)
    {
        var subject = RenderString(template.Subject, data);
        var body = RenderString(template.Content, data);

        string? htmlBody = null;
        if (!string.IsNullOrEmpty(template.Layout))
        {
            htmlBody = ApplyLayout(template.Layout, body, data);
        }
        else if (template.Content.Contains("<html", StringComparison.OrdinalIgnoreCase) ||
                 template.Content.Contains("<div", StringComparison.OrdinalIgnoreCase))
        {
            htmlBody = body;
            body = StripHtml(body);
        }

        var metadata = new Dictionary<string, string>
        {
            ["templateName"] = template.Name,
            ["templateVersion"] = template.Version,
            ["channel"] = template.Channel.ToString()
        };

        return Task.FromResult(new RenderedContent(subject, body, htmlBody, metadata));
    }

    private string RenderString(string template, Dictionary<string, object> data)
    {
        if (string.IsNullOrEmpty(template)) return string.Empty;

        var result = ProcessConditionals(template, data);
        result = SubstituteVariables(result, data);

        return result;
    }

    private string ProcessConditionals(string template, Dictionary<string, object> data)
    {
        return ConditionalPattern.Replace(template, match =>
        {
            var variable = match.Groups[1].Value;
            var content = match.Groups[2].Value;

            if (data.TryGetValue(variable, out var value))
            {
                var shouldRender = value switch
                {
                    bool b => b,
                    string s => !string.IsNullOrEmpty(s),
                    null => false,
                    _ => true
                };

                return shouldRender ? content : string.Empty;
            }

            return string.Empty;
        });
    }

    private string SubstituteVariables(string template, Dictionary<string, object> data)
    {
        return VariablePattern.Replace(template, match =>
        {
            var variable = match.Groups[1].Value;

            if (data.TryGetValue(variable, out var value))
            {
                return FormatValue(value);
            }

            _logger.LogWarning("Missing template variable: {Variable}", variable);
            return match.Value;
        });
    }

    private static string FormatValue(object? value)
    {
        return value switch
        {
            null => string.Empty,
            DateTime dt => dt.ToString("MMMM d, yyyy"),
            DateTimeOffset dto => dto.ToString("MMMM d, yyyy"),
            decimal d => d.ToString("C"),
            double dbl => dbl.ToString("N2"),
            bool b => b ? "Yes" : "No",
            _ => value.ToString() ?? string.Empty
        };
    }

    private string ApplyLayout(string layout, string body, Dictionary<string, object> data)
    {
        var layoutData = new Dictionary<string, object>(data)
        {
            ["body"] = body
        };
        return RenderString(layout, layoutData);
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html)) return string.Empty;

        var result = Regex.Replace(html, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"</p>", "\n\n", RegexOptions.IgnoreCase);
        result = Regex.Replace(result, @"<[^>]+>", string.Empty);
        result = System.Net.WebUtility.HtmlDecode(result);
        result = Regex.Replace(result, @"\n{3,}", "\n\n");

        return result.Trim();
    }
}
