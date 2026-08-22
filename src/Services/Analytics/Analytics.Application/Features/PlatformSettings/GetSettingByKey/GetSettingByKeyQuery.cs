using MediatR;
using Analytics.Application.DTOs;
using BuildingBlocks.Application.Abstractions;

namespace Analytics.Application.Features.PlatformSettings.GetSettingByKey;

public record GetSettingByKeyQuery(string Key) : IRequest<Result<PlatformSettingDto>>;
