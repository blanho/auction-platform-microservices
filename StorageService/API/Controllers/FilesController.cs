using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StorageService.Application.DTOs;
using StorageService.Application.Interfaces;

namespace StorageService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(IFileStorageService fileStorageService, ILogger<FilesController> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file provided");
        }

        var uploadedBy = User.Identity?.Name;

        await using var stream = file.OpenReadStream();
        var result = await _fileStorageService.UploadToTempAsync(
            stream,
            file.FileName,
            file.ContentType,
            uploadedBy,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Metadata);
    }

    [HttpPost("upload/batch")]
    public async Task<IActionResult> UploadBatch(List<IFormFile> files, CancellationToken cancellationToken)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest("No files provided");
        }

        var uploadedBy = User.Identity?.Name;
        var results = new List<FileMetadataDto>();
        var errors = new List<string>();

        foreach (var file in files)
        {
            await using var stream = file.OpenReadStream();
            var result = await _fileStorageService.UploadToTempAsync(
                stream,
                file.FileName,
                file.ContentType,
                uploadedBy,
                cancellationToken);

            if (result.Success)
            {
                results.Add(result.Metadata);
            }
            else
            {
                errors.Add($"{file.FileName}: {result.Error}");
            }
        }

        return Ok(new { Files = results, Errors = errors });
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm([FromBody] FileConfirmRequest request, CancellationToken cancellationToken)
    {
        var result = await _fileStorageService.ConfirmFileAsync(request, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Metadata);
    }

    [HttpPost("confirm/batch")]
    public async Task<IActionResult> ConfirmBatch([FromBody] BatchConfirmRequest request, CancellationToken cancellationToken)
    {
        var results = new List<FileMetadataDto>();
        var errors = new List<string>();

        foreach (var fileRequest in request.Files)
        {
            var result = await _fileStorageService.ConfirmFileAsync(fileRequest, cancellationToken);

            if (result.Success)
            {
                results.Add(result.Metadata);
            }
            else
            {
                errors.Add($"{fileRequest.FileId}: {result.Error}");
            }
        }

        return Ok(new { Files = results, Errors = errors });
    }

    [HttpGet("{fileId:guid}")]
    public async Task<IActionResult> GetMetadata(Guid fileId, CancellationToken cancellationToken)
    {
        var metadata = await _fileStorageService.GetMetadataAsync(fileId, cancellationToken);

        if (metadata == null)
        {
            return NotFound();
        }

        return Ok(metadata);
    }

    [HttpGet("{fileId:guid}/download")]
    [AllowAnonymous]
    public async Task<IActionResult> Download(Guid fileId, CancellationToken cancellationToken)
    {
        var (stream, metadata) = await _fileStorageService.DownloadAsync(fileId, cancellationToken);

        if (stream == null || metadata == null)
        {
            return NotFound();
        }

        return File(stream, metadata.ContentType, metadata.OriginalFileName);
    }

    [HttpGet("entity/{entityType}/{entityId}")]
    public async Task<IActionResult> GetByEntity(string entityType, string entityId, CancellationToken cancellationToken)
    {
        var files = await _fileStorageService.GetByEntityAsync(entityType, entityId, cancellationToken);
        return Ok(files);
    }

    [HttpDelete("{fileId:guid}")]
    public async Task<IActionResult> Delete(Guid fileId, CancellationToken cancellationToken)
    {
        var result = await _fileStorageService.DeleteAsync(fileId, cancellationToken);

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}
