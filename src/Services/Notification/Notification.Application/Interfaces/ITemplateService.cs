using BuildingBlocks.Application.Abstractions;
using Notification.Application.DTOs;

namespace Notification.Application.Interfaces;

public interface ITemplateService
{
    Task<Result<TemplateDto>> GetByKeyAsync(string key, CancellationToken ct = default);
    Task<Result<TemplateDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<List<TemplateDto>>> GetAllActiveAsync(CancellationToken ct = default);
    Task<Result<PaginatedResult<TemplateDto>>> GetPagedAsync(int page, int pageSize, CancellationToken ct = default);
    Task<Result<TemplateDto>> CreateAsync(CreateTemplateDto dto, CancellationToken ct = default);
    Task<Result<TemplateDto>> UpdateAsync(Guid id, UpdateTemplateDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsAsync(string key, CancellationToken ct = default);
}
