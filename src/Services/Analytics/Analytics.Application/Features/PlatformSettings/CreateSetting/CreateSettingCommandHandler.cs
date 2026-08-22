using MediatR;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;
using Analytics.Application.Mappers;
using Analytics.Application.Errors;
using Analytics.Domain.Entities;
using BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Features.PlatformSettings.CreateSetting;

public class CreateSettingCommandHandler : IRequestHandler<CreateSettingCommand, Result<PlatformSettingDto>>
{
    private readonly IPlatformSettingRepository _settingRepository;
    private readonly ILogger<CreateSettingCommandHandler> _logger;

    public CreateSettingCommandHandler(IPlatformSettingRepository settingRepository, ILogger<CreateSettingCommandHandler> logger)
    {
        _settingRepository = settingRepository;
        _logger = logger;
    }

    public async Task<Result<PlatformSettingDto>> Handle(CreateSettingCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        if (await _settingRepository.ExistsAsync(dto.Key, cancellationToken))
            return Result.Failure<PlatformSettingDto>(AnalyticsErrors.Setting.KeyExists);

        var setting = new PlatformSetting
        {
            Id = Guid.NewGuid(),
            Key = dto.Key,
            Value = dto.Value,
            Description = dto.Description,
            Category = dto.Category,
            DataType = dto.DataType,
            ValidationRules = dto.ValidationRules,
            IsSystem = false,
            LastModifiedBy = request.ModifiedBy,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _settingRepository.AddAsync(setting, cancellationToken);

        _logger.LogInformation("Platform setting '{Key}' created by {User}", dto.Key, request.ModifiedBy);

        return Result<PlatformSettingDto>.Success(setting.ToDto());
    }
}
