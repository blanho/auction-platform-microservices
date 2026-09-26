using BuildingBlocks.Application.Abstractions.Storage;
using Microsoft.Extensions.Options;
using Storage.Application.Features.Files.UploadFile;
using StorageService.Contracts.Reports;
using Xunit;

namespace Storage.Application.Tests;

public class PrivateReportUploadValidationTests
{
    private const long PublicUploadLimit = 10 * 1024 * 1024;

    [Fact]
    public void PrivateReport_UsesPrivateUploadLimit()
    {
        var validator = CreateValidator();

        Assert.True(validator.Validate(CreateCommand(ReportStorageContract.PrivateFolder,
            ReportStorageContract.MaxReportSizeBytes)).IsValid);
        Assert.False(validator.Validate(CreateCommand(ReportStorageContract.PrivateFolder,
            ReportStorageContract.MaxReportSizeBytes + 1)).IsValid);
    }

    [Fact]
    public void OrdinaryUpload_KeepsConfiguredLimit()
    {
        var validator = CreateValidator();

        Assert.True(validator.Validate(CreateCommand("documents", PublicUploadLimit)).IsValid);
        Assert.False(validator.Validate(CreateCommand("documents", PublicUploadLimit + 1)).IsValid);
    }

    private static UploadFileCommandValidator CreateValidator() =>
        new(Options.Create(new FileStorageSettings
        {
            Validation = new FileValidationSettings { MaxFileSizeBytes = PublicUploadLimit }
        }));

    private static UploadFileCommand CreateCommand(string subFolder, long fileSize) =>
        new(Stream.Null, "report.pdf", "application/pdf", fileSize, subFolder);
}
