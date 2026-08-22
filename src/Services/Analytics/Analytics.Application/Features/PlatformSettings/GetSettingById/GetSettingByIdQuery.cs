using MediatR;
using Analytics.Application.DTOs;
using BuildingBlocks.Application.Abstractions;

namespace Analytics.Application.Features.PlatformSettings.GetSettingById;

public record GetSettingByIdQuery(Guid Id) : IRequest<Result<PlatformSettingDto>>;
