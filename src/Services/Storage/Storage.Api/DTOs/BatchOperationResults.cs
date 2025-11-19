using Storage.Application.DTOs;

namespace Storage.Api.DTOs;

public record BatchUploadResult(List<FileMetadataDto> Files, List<string> Errors);

public record BatchConfirmResult(List<FileMetadataDto> Files, List<string> Errors);
