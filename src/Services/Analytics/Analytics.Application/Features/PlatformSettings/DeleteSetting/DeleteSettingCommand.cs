using MediatR;
using BuildingBlocks.Application.Abstractions;

namespace Analytics.Application.Features.PlatformSettings.DeleteSetting;

public record DeleteSettingCommand(Guid Id) : IRequest<Result>;
