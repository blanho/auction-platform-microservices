using System.Text.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using NotificationService.Application.Ports;
using NotificationService.Domain.Enums;
using NotificationService.Domain.Models;

namespace NotificationService.Infrastructure.Templates;

public class FileSystemTemplateRepository : ITemplateRepository
{
    private readonly string _templatesPath;
    private readonly ILogger<FileSystemTemplateRepository> _logger;
    private readonly Dictionary<string, Template> _templateCache = new();
    private readonly object _cacheLock = new();
    private bool _cacheLoaded = false;

    public FileSystemTemplateRepository(
        string templatesPath,
        ILogger<FileSystemTemplateRepository> logger)
    {
        _templatesPath = templatesPath;
        _logger = logger;
    }

    public async Task<Template?> GetTemplateAsync(
        NotificationType type,
        ChannelType channel,
        string? version = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureCacheLoadedAsync(cancellationToken);

        var key = GetCacheKey(type, channel);
        lock (_cacheLock)
        {
            return _templateCache.GetValueOrDefault(key);
        }
    }

    public async Task<IReadOnlyList<Template>> GetAllTemplatesAsync(CancellationToken cancellationToken = default)
    {
        await EnsureCacheLoadedAsync(cancellationToken);

        lock (_cacheLock)
        {
            return _templateCache.Values.ToList();
        }
    }

    public async Task<bool> TemplateExistsAsync(
        NotificationType type,
        ChannelType channel,
        CancellationToken cancellationToken = default)
    {
        var template = await GetTemplateAsync(type, channel, null, cancellationToken);
        return template != null;
    }

    private async Task EnsureCacheLoadedAsync(CancellationToken cancellationToken)
    {
        if (_cacheLoaded) return;

        lock (_cacheLock)
        {
            if (_cacheLoaded) return;
        }

        await LoadTemplatesAsync(cancellationToken);

        lock (_cacheLock)
        {
            _cacheLoaded = true;
        }
    }

    private async Task LoadTemplatesAsync(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_templatesPath))
        {
            _logger.LogWarning("Templates directory not found: {Path}", _templatesPath);
            return;
        }

        var channels = new[] { "email", "sms", "push", "inapp" };

        foreach (var channelDir in channels)
        {
            var channelPath = Path.Combine(_templatesPath, channelDir);
            if (!Directory.Exists(channelPath)) continue;

            var channel = ParseChannel(channelDir);

            foreach (var file in Directory.GetFiles(channelPath, "*.json"))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file, cancellationToken);
                    var templateData = JsonSerializer.Deserialize<TemplateData>(json);
                    
                    if (templateData == null) continue;

                    var notificationType = Enum.Parse<NotificationType>(templateData.Type, ignoreCase: true);

                    var template = Template.Create(
                        templateData.Name ?? Path.GetFileNameWithoutExtension(file),
                        templateData.Version ?? "1.0",
                        channel,
                        notificationType,
                        templateData.Subject ?? string.Empty,
                        templateData.Content ?? string.Empty,
                        templateData.Layout,
                        templateData.Variables);

                    var key = GetCacheKey(notificationType, channel);
                    lock (_cacheLock)
                    {
                        _templateCache[key] = template;
                    }

                    _logger.LogDebug("Loaded template: {Name} for {Type}/{Channel}", 
                        template.Name, notificationType, channel);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load template: {File}", file);
                }
            }
        }

        _logger.LogInformation("Loaded {Count} templates", _templateCache.Count);
    }

    private static string GetCacheKey(NotificationType type, ChannelType channel)
        => $"{type}:{channel}";

    private static ChannelType ParseChannel(string channelDir) => channelDir.ToLowerInvariant() switch
    {
        "email" => ChannelType.Email,
        "sms" => ChannelType.Sms,
        "push" => ChannelType.Push,
        "inapp" => ChannelType.InApp,
        _ => ChannelType.InApp
    };

    private record TemplateData
    {
        public string? Name { get; init; }
        public string? Version { get; init; }
        public string Type { get; init; } = string.Empty;
        public string? Subject { get; init; }
        public string? Content { get; init; }
        public string? Layout { get; init; }
        public List<string>? Variables { get; init; }
    }
}
