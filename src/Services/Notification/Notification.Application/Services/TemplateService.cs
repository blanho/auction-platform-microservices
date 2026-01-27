using BuildingBlocks.Application.Abstractions;
using Notification.Application.DTOs;
using Notification.Application.Errors;
using Notification.Application.Interfaces;
using Notification.Domain.Entities;

namespace Notification.Application.Services;

public class TemplateService : ITemplateService
{
    private readonly ITemplateRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public TemplateService(ITemplateRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TemplateDto>> GetByKeyAsync(string key, CancellationToken ct = default)
    {
        var template = await _repository.GetByKeyAsync(key, ct);
        if (template == null)
            return Result.Failure<TemplateDto>(NotificationErrors.Template.NotFoundByKey(key));
        return Result.Success(template.ToDto());
    }

    public async Task<Result<TemplateDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var template = await _repository.GetByIdAsync(id, ct);
        if (template == null)
            return Result.Failure<TemplateDto>(NotificationErrors.Template.NotFound);
        return Result.Success(template.ToDto());
    }

    public async Task<Result<List<TemplateDto>>> GetAllActiveAsync(CancellationToken ct = default)
    {
        var templates = await _repository.GetAllActiveAsync(ct);
        return Result.Success(templates.Select(t => t.ToDto()).ToList());
    }

    public async Task<Result<PaginatedResult<TemplateDto>>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var result = await _repository.GetPagedAsync(page, pageSize, ct);
        return Result.Success(new PaginatedResult<TemplateDto>(
            result.Items.Select(t => t.ToDto()).ToList(),
            result.TotalCount,
            result.Page,
            result.PageSize));
    }

    public async Task<Result<TemplateDto>> CreateAsync(CreateTemplateDto dto, CancellationToken ct = default)
    {
        var existing = await _repository.GetByKeyAsync(dto.Key, ct);
        if (existing != null)
            return Result.Failure<TemplateDto>(NotificationErrors.Template.KeyExists(dto.Key));

        var template = NotificationTemplate.Create(
            dto.Key,
            dto.Name,
            dto.Subject,
            dto.Body,
            dto.Description,
            dto.SmsBody,
            dto.PushTitle,
            dto.PushBody);

        await _repository.AddAsync(template, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(template.ToDto());
    }

    public async Task<Result<TemplateDto>> UpdateAsync(Guid id, UpdateTemplateDto dto, CancellationToken ct = default)
    {
        var template = await _repository.GetByIdAsync(id, ct);
        if (template == null)
            return Result.Failure<TemplateDto>(NotificationErrors.Template.NotFound);

        template.Update(
            dto.Name,
            dto.Subject,
            dto.Body,
            dto.Description,
            dto.SmsBody,
            dto.PushTitle,
            dto.PushBody);

        template.IsActive = dto.IsActive;

        await _repository.UpdateAsync(template, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(template.ToDto());
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _repository.DeleteAsync(id, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        var template = await _repository.GetByKeyAsync(key, ct);
        return template != null;
    }
}

internal static class TemplateExtensions
{
    public static TemplateDto ToDto(this NotificationTemplate template) => new()
    {
        Id = template.Id,
        Key = template.Key,
        Name = template.Name,
        Description = template.Description,
        Subject = template.Subject,
        Body = template.Body,
        SmsBody = template.SmsBody,
        PushTitle = template.PushTitle,
        PushBody = template.PushBody,
        IsActive = template.IsActive,
        CreatedAt = template.CreatedAt,
        UpdatedAt = template.UpdatedAt
    };
}
