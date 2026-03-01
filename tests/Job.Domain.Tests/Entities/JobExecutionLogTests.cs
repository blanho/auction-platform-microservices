using BuildingBlocks.Domain.Exceptions;
using Jobs.Domain.Entities;
using Jobs.Domain.Enums;
using ExecutionContextVO = Jobs.Domain.ValueObjects.ExecutionContext;

namespace Job.Domain.Tests.Entities;

public class JobExecutionLogTests
{
    private static readonly Guid TestJobId = Guid.NewGuid();

    [Fact]
    public void CreateStateTransition_WithValidParameters_ShouldCreateLog()
    {
        var log = JobExecutionLog.CreateStateTransition(
            TestJobId,
            JobStatus.Pending,
            JobStatus.Processing,
            "Job started processing");

        log.Id.Should().NotBeEmpty();
        log.JobId.Should().Be(TestJobId);
        log.LogLevel.Should().Be(JobLogLevel.StateTransition);
        log.Message.Should().Be("Job started processing");
        log.PreviousStatus.Should().Be(JobStatus.Pending);
        log.NewStatus.Should().Be(JobStatus.Processing);
        log.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void CreateStateTransition_WithContext_ShouldSetContext()
    {
        var context = ExecutionContextVO.ForMachine("worker-01");

        var log = JobExecutionLog.CreateStateTransition(
            TestJobId,
            JobStatus.Processing,
            JobStatus.Completed,
            "Job completed",
            context);

        log.Context.Should().NotBeNull();
        log.Context!.MachineName.Should().Be("worker-01");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateStateTransition_WithEmptyMessage_ShouldThrowDomainInvariantException(string? message)
    {
        var act = () => JobExecutionLog.CreateStateTransition(
            TestJobId,
            JobStatus.Pending,
            JobStatus.Processing,
            message!);

        act.Should().Throw<DomainInvariantException>()
            .WithMessage("*Log message is required*");
    }

    [Fact]
    public void CreateInformation_WithValidParameters_ShouldCreateLog()
    {
        var log = JobExecutionLog.CreateInformation(
            TestJobId,
            "Processing batch 5 of 10");

        log.LogLevel.Should().Be(JobLogLevel.Information);
        log.Message.Should().Be("Processing batch 5 of 10");
        log.PreviousStatus.Should().BeNull();
        log.NewStatus.Should().BeNull();
        log.Duration.Should().BeNull();
    }

    [Fact]
    public void CreateInformation_WithDuration_ShouldSetDuration()
    {
        var duration = TimeSpan.FromSeconds(45);

        var log = JobExecutionLog.CreateInformation(
            TestJobId,
            "Batch processed",
            duration: duration);

        log.Duration.Should().Be(duration);
    }

    [Fact]
    public void CreateInformation_WithContextAndDuration_ShouldSetBoth()
    {
        var context = ExecutionContextVO.ForBatch("worker-02", "ImportAuctions", 3, 500);
        var duration = TimeSpan.FromMinutes(2);

        var log = JobExecutionLog.CreateInformation(
            TestJobId,
            "Batch import completed",
            context,
            duration);

        log.Context.Should().NotBeNull();
        log.Context!.MachineName.Should().Be("worker-02");
        log.Context.OperationName.Should().Be("ImportAuctions");
        log.Context.BatchNumber.Should().Be(3);
        log.Context.BatchSize.Should().Be(500);
        log.Duration.Should().Be(duration);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateInformation_WithEmptyMessage_ShouldThrowDomainInvariantException(string? message)
    {
        var act = () => JobExecutionLog.CreateInformation(TestJobId, message!);

        act.Should().Throw<DomainInvariantException>()
            .WithMessage("*Log message is required*");
    }

    [Fact]
    public void CreateWarning_WithValidParameters_ShouldCreateLog()
    {
        var log = JobExecutionLog.CreateWarning(
            TestJobId,
            "Retry attempt 2 of 3");

        log.LogLevel.Should().Be(JobLogLevel.Warning);
        log.Message.Should().Be("Retry attempt 2 of 3");
        log.PreviousStatus.Should().BeNull();
        log.NewStatus.Should().BeNull();
    }

    [Fact]
    public void CreateWarning_WithContext_ShouldSetContext()
    {
        var context = ExecutionContextVO.ForOperation("worker-03", "RetryHandler");

        var log = JobExecutionLog.CreateWarning(
            TestJobId,
            "Transient error detected",
            context);

        log.Context.Should().NotBeNull();
        log.Context!.OperationName.Should().Be("RetryHandler");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateWarning_WithEmptyMessage_ShouldThrowDomainInvariantException(string? message)
    {
        var act = () => JobExecutionLog.CreateWarning(TestJobId, message!);

        act.Should().Throw<DomainInvariantException>()
            .WithMessage("*Log message is required*");
    }

    [Fact]
    public void CreateError_WithValidParameters_ShouldCreateLog()
    {
        var log = JobExecutionLog.CreateError(
            TestJobId,
            "Database connection failed");

        log.LogLevel.Should().Be(JobLogLevel.Error);
        log.Message.Should().Be("Database connection failed");
    }

    [Fact]
    public void CreateError_WithContext_ShouldSetContext()
    {
        var context = ExecutionContextVO.ForMachine("worker-04");

        var log = JobExecutionLog.CreateError(
            TestJobId,
            "Unhandled exception",
            context);

        log.Context.Should().NotBeNull();
        log.Context!.MachineName.Should().Be("worker-04");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateError_WithEmptyMessage_ShouldThrowDomainInvariantException(string? message)
    {
        var act = () => JobExecutionLog.CreateError(TestJobId, message!);

        act.Should().Throw<DomainInvariantException>()
            .WithMessage("*Log message is required*");
    }

    [Fact]
    public void AllFactoryMethods_ShouldAssignUniqueIds()
    {
        var log1 = JobExecutionLog.CreateInformation(TestJobId, "msg1");
        var log2 = JobExecutionLog.CreateWarning(TestJobId, "msg2");
        var log3 = JobExecutionLog.CreateError(TestJobId, "msg3");
        var log4 = JobExecutionLog.CreateStateTransition(
            TestJobId, JobStatus.Pending, JobStatus.Processing, "msg4");

        var ids = new[] { log1.Id, log2.Id, log3.Id, log4.Id };
        ids.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void AllFactoryMethods_ShouldSetTimestampCloseToUtcNow()
    {
        var before = DateTimeOffset.UtcNow;

        var log1 = JobExecutionLog.CreateInformation(TestJobId, "msg1");
        var log2 = JobExecutionLog.CreateWarning(TestJobId, "msg2");
        var log3 = JobExecutionLog.CreateError(TestJobId, "msg3");
        var log4 = JobExecutionLog.CreateStateTransition(
            TestJobId, JobStatus.Pending, JobStatus.Processing, "msg4");

        var after = DateTimeOffset.UtcNow;

        log1.Timestamp.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        log2.Timestamp.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        log3.Timestamp.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        log4.Timestamp.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }
}
