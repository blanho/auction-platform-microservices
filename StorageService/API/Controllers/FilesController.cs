using Common.Core.Authorization;
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

    [HttpPost("request-upload")]
    [HasPermission(Permissions.Storage.Upload)]
    public async Task<IActionResult> RequestUpload([FromBody] RequestUploadDto request, CancellationToken cancellationToken)
    {
        try
        {
            var uploadedBy = User.Identity?.Name;
            var response = await _fileStorageService.RequestUploadAsync(request, uploadedBy, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("confirm-upload")]
    [HasPermission(Permissions.Storage.Upload)]
    public async Task<IActionResult> ConfirmUpload([FromBody] ConfirmUploadRequest request, CancellationToken cancellationToken)
    {
        var result = await _fileStorageService.ConfirmUploadAsync(request, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Metadata);
    }

    [HttpGet("{fileId:guid}/download-url")]
    [HasPermission(Permissions.Storage.View)]
    public async Task<IActionResult> GetDownloadUrl(Guid fileId, CancellationToken cancellationToken)
    {
        var response = await _fileStorageService.GetDownloadUrlAsync(fileId, cancellationToken);

        if (response == null)
        {
            return NotFound();
        }

        return Ok(response);
    }

    [HttpPost("direct-upload")]
    [HasPermission(Permissions.Storage.Upload)]
    public async Task<IActionResult> DirectUpload(
        IFormFile file, 
        [FromQuery] string ownerService,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file provided");
        }

        if (string.IsNullOrEmpty(ownerService))
        {
            return BadRequest("Owner service is required");
        }

        var uploadedBy = User.Identity?.Name;

        await using var stream = file.OpenReadStream();
        var result = await _fileStorageService.DirectUploadAsync(
            stream,
            file.FileName,
            file.ContentType,
            ownerService,
            uploadedBy,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Metadata);
    }

    [HttpPost("direct-upload/batch")]
    [HasPermission(Permissions.Storage.Upload)]
    public async Task<IActionResult> DirectUploadBatch(
        List<IFormFile> files, 
        [FromQuery] string ownerService,
        CancellationToken cancellationToken)
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest("No files provided");
        }

        if (string.IsNullOrEmpty(ownerService))
        {
            return BadRequest("Owner service is required");
        }

        var uploadedBy = User.Identity?.Name;
        var results = new List<FileMetadataDto>();
        var errors = new List<string>();

        foreach (var file in files)
        {
            await using var stream = file.OpenReadStream();
            var result = await _fileStorageService.DirectUploadAsync(
                stream,
                file.FileName,
                file.ContentType,
                ownerService,
                uploadedBy,
                cancellationToken);

            if (result.Success && result.Metadata != null)
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
    [HasPermission(Permissions.Storage.Upload)]
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
    [HasPermission(Permissions.Storage.Upload)]
    public async Task<IActionResult> ConfirmBatch([FromBody] BatchConfirmRequest request, CancellationToken cancellationToken)
    {
        var results = new List<FileMetadataDto>();
        var errors = new List<string>();

        foreach (var fileRequest in request.Files)
        {
            var confirmRequest = new FileConfirmRequest
            {
                FileId = fileRequest.FileId,
                OwnerId = fileRequest.OwnerId
            };
            
            var result = await _fileStorageService.ConfirmUploadAsync(fileRequest, cancellationToken);

            if (result.Success && result.Metadata != null)
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
    [HasPermission(Permissions.Storage.View)]
    public async Task<IActionResult> GetMetadata(Guid fileId, CancellationToken cancellationToken)
    {
        var metadata = await _fileStorageService.GetMetadataAsync(fileId, cancellationToken);

        if (metadata == null)
        {
            return NotFound();
        }

        return Ok(metadata);
    }

    [HttpGet("owner/{ownerService}/{ownerId}")]
    [HasPermission(Permissions.Storage.View)]
    public async Task<IActionResult> GetByOwner(string ownerService, string ownerId, CancellationToken cancellationToken)
    {
        var files = await _fileStorageService.GetByOwnerAsync(ownerService, ownerId, cancellationToken);
        return Ok(files);
    }

    [HttpDelete("{fileId:guid}")]
    [HasPermission(Permissions.Storage.Delete)]
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

