using MediatR;
using Analytics.Application.DTOs;
using BuildingBlocks.Application.Abstractions;

namespace Analytics.Application.Features.PlatformSettings.CreateSetting;

public record CreateSettingCommand(CreateSettingDto Dto, string? ModifiedBy) : IRequest<Result<PlatformSettingDto>>;
