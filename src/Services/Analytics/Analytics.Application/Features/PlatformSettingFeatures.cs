using MediatR;
using Analytics.Domain.Entities;
using Analytics.Domain.Enums;
using Analytics.Domain.Errors;
using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;
using Analytics.Application.Mappers;

using BuildingBlocks.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Features.PlatformSettings;

public record GetSettingsQuery(SettingCategory? Category) : IRequest<Result<List<PlatformSettingDto>>>;
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

public record GetSettingByIdQuery(Guid Id) : IRequest<Result<PlatformSettingDto>>;
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

public record GetSettingByKeyQuery(string Key) : IRequest<Result<PlatformSettingDto>>;
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

public record CreateSettingCommand(CreateSettingDto Dto, string? ModifiedBy) : IRequest<Result<PlatformSettingDto>>;
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

public record UpdateSettingCommand(Guid Id, UpdateSettingDto Dto, string? ModifiedBy) : IRequest<Result<PlatformSettingDto>>;
public class UpdateSettingCommandHandler : IRequestHandler<UpdateSettingCommand, Result<PlatformSettingDto>>
{
    private readonly IPlatformSettingRepository _settingRepository;
    private readonly ILogger<UpdateSettingCommandHandler> _logger;

    public UpdateSettingCommandHandler(IPlatformSettingRepository settingRepository, ILogger<UpdateSettingCommandHandler> logger)
    {
        _settingRepository = settingRepository;
        _logger = logger;
    }

    public async Task<Result<PlatformSettingDto>> Handle(UpdateSettingCommand request, CancellationToken cancellationToken)
    {
        var setting = await _settingRepository.GetByIdAsync(request.Id, cancellationToken);
        if (setting == null)
            return Result.Failure<PlatformSettingDto>(AnalyticsErrors.Setting.NotFound);

        setting.Value = request.Dto.Value;
        setting.LastModifiedBy = request.ModifiedBy;
        setting.UpdatedAt = DateTimeOffset.UtcNow;

        await _settingRepository.UpdateAsync(setting, cancellationToken);

        _logger.LogInformation("Platform setting '{Key}' updated by {User}", setting.Key, request.ModifiedBy);

        return Result<PlatformSettingDto>.Success(setting.ToDto());
    }
}

public record DeleteSettingCommand(Guid Id) : IRequest<Result>;
public class DeleteSettingCommandHandler : IRequestHandler<DeleteSettingCommand, Result>
{
    private readonly IPlatformSettingRepository _settingRepository;
    private readonly ILogger<DeleteSettingCommandHandler> _logger;

    public DeleteSettingCommandHandler(IPlatformSettingRepository settingRepository, ILogger<DeleteSettingCommandHandler> logger)
    {
        _settingRepository = settingRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteSettingCommand request, CancellationToken cancellationToken)
    {
        var setting = await _settingRepository.GetByIdAsync(request.Id, cancellationToken);
        if (setting == null)
            return Result.Failure(AnalyticsErrors.Setting.NotFound);

        if (setting.IsSystem)
            return Result.Failure(AnalyticsErrors.Setting.SystemSettingReadOnly);

        await _settingRepository.DeleteAsync(request.Id, cancellationToken);

        _logger.LogInformation("Platform setting '{Key}' deleted", setting.Key);

        return Result.Success();
    }
}

public record BulkUpdateSettingsCommand(List<SettingKeyValue> Settings, string? ModifiedBy) : IRequest<Result>;
public class BulkUpdateSettingsCommandHandler : IRequestHandler<BulkUpdateSettingsCommand, Result>
{
    private readonly IPlatformSettingRepository _settingRepository;
    private readonly ILogger<BulkUpdateSettingsCommandHandler> _logger;

    public BulkUpdateSettingsCommandHandler(IPlatformSettingRepository settingRepository, ILogger<BulkUpdateSettingsCommandHandler> logger)
    {
        _settingRepository = settingRepository;
        _logger = logger;
    }

    public async Task<Result> Handle(BulkUpdateSettingsCommand request, CancellationToken cancellationToken)
    {
        foreach (var item in request.Settings)
        {
            var setting = await _settingRepository.GetByKeyAsync(item.Key, cancellationToken);

            if (setting == null)
            {
                setting = new PlatformSetting
                {
                    Id = Guid.NewGuid(),
                    Key = item.Key,
                    Value = item.Value,
                    Category = SettingCategory.Platform,
                    IsSystem = false,
                    LastModifiedBy = request.ModifiedBy,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                await _settingRepository.AddAsync(setting, cancellationToken);
            }
            else
            {
                setting.Value = item.Value;
                setting.LastModifiedBy = request.ModifiedBy;
                setting.UpdatedAt = DateTimeOffset.UtcNow;
                await _settingRepository.UpdateAsync(setting, cancellationToken);
            }
        }

        _logger.LogInformation("Bulk update of {Count} settings by {User}", request.Settings.Count, request.ModifiedBy);

        return Result.Success();
    }
}
