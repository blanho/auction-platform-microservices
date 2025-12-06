using Common.Storage.Abstractions;
using Common.Storage.Enums;
using Common.Storage.Models;
using Microsoft.AspNetCore.Mvc;

namespace UtilityService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<FilesController> _logger;

    public FilesController(
        IFileStorageService fileStorageService,
        ILogger<FilesController> logger)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50MB limit
    public async Task<ActionResult<FileMetadata>> UploadFile(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file provided");
        }

        var userId = User.Identity?.Name; 

        await using var stream = file.OpenReadStream();
        var result = await _fileStorageService.UploadToTempAsync(
            stream,
            file.FileName,
            file.ContentType,
            userId,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result.Error);
        }

        _logger.LogInformation("File {FileName} uploaded to temp storage with ID {FileId}",
            file.FileName, result.Metadata!.Id);

        return Ok(result.Metadata);
    }


    [HttpPost("upload/batch")]
    [RequestSizeLimit(100 * 1024 * 1024)] // 100MB limit for batch
    public async Task<ActionResult<List<FileUploadResult>>> UploadFiles(
        IFormFileCollection files,
        CancellationToken cancellationToken)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest("No files provided");
        }

        var userId = User.Identity?.Name;
        var results = new List<FileUploadResult>();

        foreach (var file in files)
        {
            await using var stream = file.OpenReadStream();
            var result = await _fileStorageService.UploadToTempAsync(
                stream,
                file.FileName,
                file.ContentType,
                userId,
                cancellationToken);
            
            results.Add(result);
        }

        return Ok(results);
    }

    [HttpPost("confirm")]
    public async Task<ActionResult<FileMetadata>> ConfirmFile(
        [FromBody] FileConfirmRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _fileStorageService.ConfirmFileAsync(request, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result.Error);
        }

        _logger.LogInformation("File {FileId} confirmed and moved to permanent storage", request.FileId);

        return Ok(result.Metadata);
    }

    [HttpPost("confirm/batch")]
    public async Task<ActionResult<List<FileUploadResult>>> ConfirmFiles(
        [FromBody] List<FileConfirmRequest> requests,
        CancellationToken cancellationToken)
    {
        var results = new List<FileUploadResult>();

        foreach (var request in requests)
        {
            var result = await _fileStorageService.ConfirmFileAsync(request, cancellationToken);
            results.Add(result);
        }

        return Ok(results);
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadFile(
        Guid id,
        CancellationToken cancellationToken)
    {
        var (stream, metadata) = await _fileStorageService.DownloadAsync(id, cancellationToken);

        if (stream == null || metadata == null)
        {
            return NotFound();
        }

        return File(stream, metadata.ContentType, metadata.OriginalFileName);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FileMetadata>> GetFile(
        Guid id,
        CancellationToken cancellationToken)
    {
        var metadata = await _fileStorageService.GetMetadataAsync(id, cancellationToken);

        if (metadata == null)
        {
            return NotFound();
        }

        return Ok(metadata);
    }

    [HttpGet("entity/{entityType}/{entityId}")]
    public async Task<ActionResult<IEnumerable<FileMetadata>>> GetByEntity(
        string entityType,
        string entityId,
        CancellationToken cancellationToken)
    {
        var files = await _fileStorageService.GetByEntityAsync(entityType, entityId, cancellationToken);
        return Ok(files);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFile(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _fileStorageService.DeleteAsync(id, cancellationToken);

        if (!result)
        {
            return NotFound();
        }

        _logger.LogInformation("File {FileId} deleted", id);

        return NoContent();
    }
}
