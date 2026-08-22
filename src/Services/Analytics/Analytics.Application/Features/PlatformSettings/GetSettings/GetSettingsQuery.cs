using MediatR;
using Analytics.Application.DTOs;
using Analytics.Domain.Enums;
using BuildingBlocks.Application.Abstractions;

namespace Analytics.Application.Features.PlatformSettings.GetSettings;

public record GetSettingsQuery(SettingCategory? Category) : IRequest<Result<List<PlatformSettingDto>>>;
