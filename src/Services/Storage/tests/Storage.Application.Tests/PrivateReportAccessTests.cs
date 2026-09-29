using System.Reflection;
using BuildingBlocks.Application.Abstractions;
using BuildingBlocks.Application.Abstractions.Auditing;
using BuildingBlocks.Application.Abstractions.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Storage.Application.Features.Files.ConfirmPresignedUpload;
using Storage.Application.Features.Files.GeneratePresignedDownload;
using Storage.Application.Features.Files.GetFileById;
using Storage.Application.Features.Files.GetFileUrl;
using Storage.Application.Interfaces;
using Storage.Domain.Entities;
using Storage.Domain.Enums;
using StorageService.Contracts.Reports;
using Xunit;

namespace Storage.Application.Tests;

public class PrivateReportAccessTests
{
    [Fact]
    public async Task GenericPresignedDownload_DoesNotExposePrivateReport()
    {
        var report = StoredFile.Create("report.pdf", "private-reports/report.pdf", "application/pdf",
            10, "/files/private-reports/report.pdf", ReportStorageContract.PrivateFolder,
            Guid.NewGuid(), StorageProvider.Local);
        var repository = DispatchProxy.Create<IStoredFileRepository, TestProxy>();
        ((TestProxy)(object)repository).Handler = (method, _) =>
            method.Name == nameof(IStoredFileRepository.GetByIdAsync)
                ? Task.FromResult<StoredFile?>(report)
                : throw new NotSupportedException(method.Name);
        var fileStorage = DispatchProxy.Create<IFileStorageService, TestProxy>();
        ((TestProxy)(object)fileStorage).Handler = (method, _) =>
            throw new InvalidOperationException($"Unexpected storage access: {method.Name}");
        var handler = new GeneratePresignedDownloadQueryHandler(repository, fileStorage,
            NullLogger<GeneratePresignedDownloadQueryHandler>.Instance);

        var result = await handler.Handle(new GeneratePresignedDownloadQuery(report.Id), CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GenericFileReads_RejectMisclassifiedPrivateBlob()
    {
        var report = StoredFile.Create("report.pdf", "private-reports/report.pdf", "application/pdf",
            10, "/files/private-reports/report.pdf", "documents",
            Guid.NewGuid(), StorageProvider.Local);
        var repository = DispatchProxy.Create<IStoredFileRepository, TestProxy>();
        ((TestProxy)(object)repository).Handler = (method, _) =>
            method.Name == nameof(IStoredFileRepository.GetByIdAsync)
                ? Task.FromResult<StoredFile?>(report)
                : throw new NotSupportedException(method.Name);
        var fileStorage = DispatchProxy.Create<IFileStorageService, TestProxy>();
        ((TestProxy)(object)fileStorage).Handler = (method, _) =>
            throw new InvalidOperationException($"Unexpected storage access: {method.Name}");

        var download = await new GeneratePresignedDownloadQueryHandler(repository, fileStorage,
            NullLogger<GeneratePresignedDownloadQueryHandler>.Instance)
            .Handle(new GeneratePresignedDownloadQuery(report.Id), CancellationToken.None);
        var url = await new GetFileUrlQueryHandler(repository, fileStorage,
            NullLogger<GetFileUrlQueryHandler>.Instance)
            .Handle(new GetFileUrlQuery(report.Id), CancellationToken.None);
        var metadata = await new GetFileByIdQueryHandler(repository,
            NullLogger<GetFileByIdQueryHandler>.Instance)
            .Handle(new GetFileByIdQuery(report.Id), CancellationToken.None);

        Assert.True(download.IsFailure);
        Assert.True(url.IsFailure);
        Assert.True(metadata.IsFailure);
    }

    [Fact]
    public async Task PresignedConfirmation_RejectsPrivateBlobKeyBeforeStorageAccess()
    {
        var fileStorage = DispatchProxy.Create<IFileStorageService, TestProxy>();
        ((TestProxy)(object)fileStorage).Handler = (method, _) =>
            throw new InvalidOperationException($"Unexpected storage access: {method.Name}");
        var handler = new ConfirmPresignedUploadCommandHandler(
            fileStorage,
            DispatchProxy.Create<IStoredFileRepository, TestProxy>(),
            DispatchProxy.Create<IUnitOfWork, TestProxy>(),
            NullLogger<ConfirmPresignedUploadCommandHandler>.Instance,
            DispatchProxy.Create<IAuditPublisher, TestProxy>());

        var result = await handler.Handle(new ConfirmPresignedUploadCommand(
            "private-reports/report.pdf", "report.pdf", "application/pdf", 10, "documents", Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(result.IsFailure);
    }

    public class TestProxy : DispatchProxy
    {
        public Func<MethodInfo, object?[]?, object?> Handler { get; set; } = null!;

        protected override object? Invoke(MethodInfo? method, object?[]? args) => Handler(method!, args);
    }
}
