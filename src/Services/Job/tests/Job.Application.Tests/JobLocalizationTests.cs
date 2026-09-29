using AutoMapper;
using BuildingBlocks.Application.Localization;
using Jobs.Application.DTOs;
using Jobs.Application.Mappings;
using Jobs.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Jobs.Application.Tests;

public class JobLocalizationTests
{
    [Fact]
    public void MappingLocalizesEachReadWithoutChangingStoredDiagnostics()
    {
        var mapper = new MapperConfiguration(config => config.AddProfile<JobMappingProfile>(),
            NullLoggerFactory.Instance).CreateMapper();
        var job = Jobs.Domain.Entities.Job.Create(JobType.AuctionImport, "localization", "{}", Guid.NewGuid(), 1);
        var item = job.AddItem("{}", 1);
        job.Start();
        item.MarkProcessing();
        const string failure = "Auction workflow failed after broker retries.";
        item.MarkFailed(failure, terminal: true);
        job.Fail(failure);

        using (new RequestCultureScope("ja-JP"))
        {
            Assert.Equal("再試行後もオークション処理に失敗しました。", mapper.Map<JobDto>(job).ErrorMessage);
            Assert.Equal("再試行後もオークション処理に失敗しました。", mapper.Map<JobItemDto>(item).ErrorMessage);
        }
        using (new RequestCultureScope("en-US"))
            Assert.Equal(failure, mapper.Map<JobDto>(job).ErrorMessage);
        Assert.Equal(failure, job.ErrorMessage);
        Assert.Equal(failure, item.ErrorMessage);
    }
}
