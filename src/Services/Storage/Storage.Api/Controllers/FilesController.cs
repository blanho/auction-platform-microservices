using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Storage.Application.DTOs;
using Storage.Application.Interfaces;
using Storage.Domain.Enums;
using System.Security.Claims;

namespace Storage.Api.Controllers;

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

    #region Standard Upload Flow

    [HttpPost("request-upload")]
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
    public async Task<IActionResult> ConfirmUpload([FromBody] ConfirmUploadRequest request, CancellationToken cancellationToken)
    {
        var result = await _fileStorageService.ConfirmUploadAsync(request, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Metadata);
    }

    #endregion

    #region Scan Status & Submission

    [HttpGet("{fileId:guid}/scan-status")]
    public async Task<IActionResult> GetScanStatus(Guid fileId, CancellationToken cancellationToken)
    {
        var response = await _fileStorageService.CheckScanStatusAsync(fileId, cancellationToken);
        return Ok(response);
    }

    [HttpPost("{fileId:guid}/submit")]
    public async Task<IActionResult> SubmitFile(
        Guid fileId,
        [FromBody] SubmitFileRequest request,
        CancellationToken cancellationToken)
    {

        var submitRequest = request with { FileId = fileId };

        var result = await _fileStorageService.SubmitFileAsync(submitRequest, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result.Error);
        }

        return Ok(result);
    }

    #endregion

    #region Download URLs

    [HttpGet("{fileId:guid}/download-url")]
    public async Task<IActionResult> GetDownloadUrl(Guid fileId, CancellationToken cancellationToken)
    {
        var response = await _fileStorageService.GetDownloadUrlAsync(fileId, cancellationToken);

        if (response == null)
        {
            return NotFound();
        }

        return Ok(response);
    }

    [HttpGet("{fileId:guid}/secure-download-url")]
    public async Task<IActionResult> GetSecureDownloadUrl(Guid fileId, CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);

        var response = await _fileStorageService.GetDownloadUrlWithPermissionCheckAsync(
            fileId,
            userId,
            roles,
            cancellationToken);

        if (response == null)
        {
            return NotFound("File not found or access denied");
        }

        return Ok(response);
    }

    #endregion

    #region Direct Upload

    [HttpPost("direct-upload")]
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

    #endregion

    #region Multipart Upload

    [HttpPost("multipart/initiate")]
    public async Task<IActionResult> InitiateMultipartUpload(
        [FromBody] InitiateMultipartUploadRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var uploadedBy = User.Identity?.Name;
            var session = await _fileStorageService.InitiateMultipartUploadAsync(
                request,
                uploadedBy,
                cancellationToken);

            if (session == null)
            {
                return BadRequest("Failed to initiate multipart upload. File may be too small or provider doesn't support multipart.");
            }

            return Ok(session);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("multipart/complete")]
    public async Task<IActionResult> CompleteMultipartUpload(
        [FromBody] CompleteMultipartUploadRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _fileStorageService.CompleteMultipartUploadAsync(request, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Metadata);
    }

    #endregion

    #region Confirm & Batch Operations

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

    #endregion

    #region Query Operations

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

    [HttpGet("owner/{ownerService}/{ownerId}")]
    public async Task<IActionResult> GetByOwner(string ownerService, string ownerId, CancellationToken cancellationToken)
    {
        var files = await _fileStorageService.GetByOwnerAsync(ownerService, ownerId, cancellationToken);
        return Ok(files);
    }

    [HttpGet("resource/{resourceType}/{resourceId:guid}")]
    public async Task<IActionResult> GetByResource(
        StorageResourceType resourceType,
        Guid resourceId,
        CancellationToken cancellationToken)
    {
        var files = await _fileStorageService.GetByResourceAsync(resourceId, resourceType, cancellationToken);
        return Ok(files);
    }

    #endregion

    #region Delete

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

    #endregion
}
