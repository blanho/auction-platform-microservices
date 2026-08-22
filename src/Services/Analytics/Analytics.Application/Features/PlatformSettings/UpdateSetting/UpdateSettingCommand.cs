using MediatR;
using Analytics.Application.DTOs;
using BuildingBlocks.Application.Abstractions;

namespace Analytics.Application.Features.PlatformSettings.UpdateSetting;

public record UpdateSettingCommand(Guid Id, UpdateSettingDto Dto, string? ModifiedBy) : IRequest<Result<PlatformSettingDto>>;
