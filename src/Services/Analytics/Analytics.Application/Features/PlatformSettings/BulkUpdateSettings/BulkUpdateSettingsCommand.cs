using MediatR;
using Analytics.Application.DTOs;
using BuildingBlocks.Application.Abstractions;

namespace Analytics.Application.Features.PlatformSettings.BulkUpdateSettings;

public record BulkUpdateSettingsCommand(List<SettingKeyValue> Settings, string? ModifiedBy) : IRequest<Result>;
