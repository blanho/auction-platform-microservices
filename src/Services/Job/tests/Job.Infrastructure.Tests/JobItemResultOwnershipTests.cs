using System.Reflection;
using BuildingBlocks.Application.Abstractions;
using Jobs.Application.Interfaces;
using Jobs.Domain.Entities;
using Jobs.Domain.Enums;
using Jobs.Infrastructure.Messaging.Consumers;
using JobService.Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Jobs.Infrastructure.Tests;

public class JobItemResultOwnershipTests
{
    [Fact]
    public async Task SingleResult_ForAnotherJobsItem_DoesNotUpdateEitherJob()
    {
        var item = JobItem.Create(Guid.NewGuid(), "{}", 1, 1);
        item.MarkProcessing();
        var message = new ReportJobItemResultCommand
        {
            JobId = Guid.NewGuid(),
            JobItemId = item.Id,
            IsSuccess = true
        };
        var context = Context(message);
        var items = Stub<IJobItemRepository>((method, _) => method.Name switch
        {
            nameof(IJobItemRepository.GetByIdForUpdateAsync) => Task.FromResult<JobItem?>(item),
            _ => throw new NotSupportedException(method.Name)
        });
        var consumer = new ReportJobItemResultConsumer(null!, items, null!,
            NullLogger<ReportJobItemResultConsumer>.Instance);

        await consumer.Consume(context);

        Assert.Equal(JobItemStatus.Processing, item.Status);
    }

    [Fact]
    public async Task BatchResult_SkipsItemsFromAnotherJob()
    {
        var job = Job.Create(JobType.DataExport, Guid.NewGuid().ToString(), "{}", Guid.NewGuid(), 2);
        job.Start();
        var ownedItem = job.AddItem("{}", 1);
        ownedItem.MarkProcessing();
        var foreignItem = JobItem.Create(Guid.NewGuid(), "{}", 1, 1);
        foreignItem.MarkProcessing();
        var message = new ReportJobItemBatchResultCommand
        {
            JobId = job.Id,
            Results =
            [
                new JobItemBatchResult { JobItemId = ownedItem.Id, IsSuccess = true },
                new JobItemBatchResult { JobItemId = foreignItem.Id, IsSuccess = true }
            ]
        };
        var updatedItems = new List<Guid>();
        var items = Stub<IJobItemRepository>((method, args) => method.Name switch
        {
            nameof(IJobItemRepository.GetByIdsForUpdateAsync) => Task.FromResult(new List<JobItem> { ownedItem, foreignItem }),
            nameof(IJobItemRepository.UpdateAsync) => RecordUpdate((JobItem)args![0]!),
            _ => throw new NotSupportedException(method.Name)
        });
        Task RecordUpdate(JobItem item)
        {
            updatedItems.Add(item.Id);
            return Task.CompletedTask;
        }
        var jobs = Stub<IJobRepository>((method, _) => method.Name switch
        {
            nameof(IJobRepository.GetByIdForUpdateAsync) => Task.FromResult<Job?>(job),
            nameof(IJobRepository.UpdateAsync) => Task.CompletedTask,
            _ => throw new NotSupportedException(method.Name)
        });
        var unitOfWork = Stub<IUnitOfWork>((method, _) => method.Name switch
        {
            nameof(IUnitOfWork.SaveChangesAsync) => Task.FromResult(1),
            _ => throw new NotSupportedException(method.Name)
        });
        var consumer = new ReportJobItemBatchResultConsumer(jobs, items, unitOfWork,
            NullLogger<ReportJobItemBatchResultConsumer>.Instance);

        await consumer.Consume(Context(message));

        Assert.Equal(new[] { ownedItem.Id }, updatedItems);
        Assert.Equal(JobItemStatus.Completed, ownedItem.Status);
        Assert.Equal(JobItemStatus.Processing, foreignItem.Status);
        Assert.Equal(1, job.CompletedItems);
    }

    private static ConsumeContext<T> Context<T>(T message) where T : class =>
        Stub<ConsumeContext<T>>((method, _) => method.Name switch
        {
            "get_Message" => message,
            "get_CancellationToken" => CancellationToken.None,
            _ => throw new NotSupportedException(method.Name)
        });

    private static T Stub<T>(Func<MethodInfo, object?[]?, object?> handler) where T : class
    {
        var proxy = DispatchProxy.Create<T, TestProxy>();
        ((TestProxy)(object)proxy).Handler = handler;
        return proxy;
    }

    public class TestProxy : DispatchProxy
    {
        public Func<MethodInfo, object?[]?, object?> Handler { get; set; } = null!;
        protected override object? Invoke(MethodInfo? method, object?[]? args) => Handler(method!, args);
    }
}
