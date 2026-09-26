using System.Reflection;
using BuildingBlocks.Application.Abstractions.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Storage.Application.Features.Files.GeneratePresignedDownload;
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

    public class TestProxy : DispatchProxy
    {
        public Func<MethodInfo, object?[]?, object?> Handler { get; set; } = null!;

        protected override object? Invoke(MethodInfo? method, object?[]? args) => Handler(method!, args);
    }
}
