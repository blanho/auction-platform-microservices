using MediatR;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;
using Analytics.Application.Mappers;
using Analytics.Application.Errors;
using BuildingBlocks.Application.Abstractions;

namespace Analytics.Application.Features.PlatformSettings.GetSettingByKey;

public class GetSettingByKeyQueryHandler : IRequestHandler<GetSettingByKeyQuery, Result<PlatformSettingDto>>
{
    private readonly IPlatformSettingRepository _settingRepository;

    public GetSettingByKeyQueryHandler(IPlatformSettingRepository settingRepository)
    {
        _settingRepository = settingRepository;
    }

    public async Task<Result<PlatformSettingDto>> Handle(GetSettingByKeyQuery request, CancellationToken cancellationToken)
    {
        var setting = await _settingRepository.GetByKeyAsync(request.Key, cancellationToken);
        if (setting == null)
            return Result.Failure<PlatformSettingDto>(AnalyticsErrors.Setting.NotFound);

        return Result<PlatformSettingDto>.Success(setting.ToDto());
    }
}
