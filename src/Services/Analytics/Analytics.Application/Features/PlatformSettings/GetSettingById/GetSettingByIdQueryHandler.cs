using MediatR;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;
using Analytics.Application.Mappers;
using Analytics.Application.Errors;
using BuildingBlocks.Application.Abstractions;

namespace Analytics.Application.Features.PlatformSettings.GetSettingById;

public class GetSettingByIdQueryHandler : IRequestHandler<GetSettingByIdQuery, Result<PlatformSettingDto>>
{
    private readonly IPlatformSettingRepository _settingRepository;

    public GetSettingByIdQueryHandler(IPlatformSettingRepository settingRepository)
    {
        _settingRepository = settingRepository;
    }

    public async Task<Result<PlatformSettingDto>> Handle(GetSettingByIdQuery request, CancellationToken cancellationToken)
    {
        var setting = await _settingRepository.GetByIdAsync(request.Id, cancellationToken);
        if (setting == null)
            return Result.Failure<PlatformSettingDto>(AnalyticsErrors.Setting.NotFound);

        return Result<PlatformSettingDto>.Success(setting.ToDto());
    }
}
