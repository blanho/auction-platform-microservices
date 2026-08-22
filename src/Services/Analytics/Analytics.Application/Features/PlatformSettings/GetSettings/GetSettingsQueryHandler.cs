using MediatR;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;
using Analytics.Application.Mappers;
using Analytics.Domain.Entities;
using BuildingBlocks.Application.Abstractions;

namespace Analytics.Application.Features.PlatformSettings.GetSettings;

public class GetSettingsQueryHandler : IRequestHandler<GetSettingsQuery, Result<List<PlatformSettingDto>>>
{
    private readonly IPlatformSettingRepository _settingRepository;

    public GetSettingsQueryHandler(IPlatformSettingRepository settingRepository)
    {
        _settingRepository = settingRepository;
    }

    public async Task<Result<List<PlatformSettingDto>>> Handle(GetSettingsQuery request, CancellationToken cancellationToken)
    {
        List<PlatformSetting> settings;
        if (request.Category.HasValue)
        {
            settings = await _settingRepository.GetByCategoryAsync(request.Category.Value, cancellationToken);
        }
        else
        {
            var result = await _settingRepository.GetPagedAsync(1, 200, cancellationToken);
            settings = result.Items.ToList();
        }

        return Result<List<PlatformSettingDto>>.Success(settings.ToDtoList());
    }
}
