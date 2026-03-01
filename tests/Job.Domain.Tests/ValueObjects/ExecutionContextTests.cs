using BuildingBlocks.Domain.Exceptions;
using ExecutionContextVO = Jobs.Domain.ValueObjects.ExecutionContext;

namespace Job.Domain.Tests.ValueObjects;

public class ExecutionContextTests
{
    [Fact]
    public void Constructor_WithValidMachineName_ShouldCreateContext()
    {
        var context = new ExecutionContextVO("worker-01");

        context.MachineName.Should().Be("worker-01");
        context.OperationName.Should().BeNull();
        context.BatchNumber.Should().BeNull();
        context.BatchSize.Should().BeNull();
        context.AdditionalData.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithAllParameters_ShouldSetAllProperties()
    {
        var context = new ExecutionContextVO(
            "worker-01",
            "ImportBatch",
            5,
            500,
            "extra-info");

        context.MachineName.Should().Be("worker-01");
        context.OperationName.Should().Be("ImportBatch");
        context.BatchNumber.Should().Be(5);
        context.BatchSize.Should().Be(500);
        context.AdditionalData.Should().Be("extra-info");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyMachineName_ShouldThrowDomainInvariantException(string? machineName)
    {
        var act = () => new ExecutionContextVO(machineName!);

        act.Should().Throw<DomainInvariantException>()
            .WithMessage("*Machine name is required*");
    }

    [Fact]
    public void ForBatch_ShouldCreateContextWithBatchInfo()
    {
        var context = ExecutionContextVO.ForBatch("worker-02", "ExportAuctions", 3, 1000);

        context.MachineName.Should().Be("worker-02");
        context.OperationName.Should().Be("ExportAuctions");
        context.BatchNumber.Should().Be(3);
        context.BatchSize.Should().Be(1000);
    }

    [Fact]
    public void ForOperation_ShouldCreateContextWithOperationInfo()
    {
        var context = ExecutionContextVO.ForOperation("worker-03", "CleanupJob");

        context.MachineName.Should().Be("worker-03");
        context.OperationName.Should().Be("CleanupJob");
        context.BatchNumber.Should().BeNull();
        context.BatchSize.Should().BeNull();
    }

    [Fact]
    public void ForMachine_ShouldCreateContextWithMachineNameOnly()
    {
        var context = ExecutionContextVO.ForMachine("worker-04");

        context.MachineName.Should().Be("worker-04");
        context.OperationName.Should().BeNull();
        context.BatchNumber.Should().BeNull();
        context.BatchSize.Should().BeNull();
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        var a = ExecutionContextVO.ForBatch("worker-01", "Import", 1, 500);
        var b = ExecutionContextVO.ForBatch("worker-01", "Import", 1, 500);

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_WithDifferentMachineName_ShouldNotBeEqual()
    {
        var a = ExecutionContextVO.ForMachine("worker-01");
        var b = ExecutionContextVO.ForMachine("worker-02");

        a.Should().NotBe(b);
    }

    [Fact]
    public void Equality_WithDifferentOperationName_ShouldNotBeEqual()
    {
        var a = ExecutionContextVO.ForOperation("worker-01", "Import");
        var b = ExecutionContextVO.ForOperation("worker-01", "Export");

        a.Should().NotBe(b);
    }

    [Fact]
    public void Equality_WithDifferentBatchNumber_ShouldNotBeEqual()
    {
        var a = ExecutionContextVO.ForBatch("worker-01", "Import", 1, 500);
        var b = ExecutionContextVO.ForBatch("worker-01", "Import", 2, 500);

        a.Should().NotBe(b);
    }

    [Fact]
    public void Equality_NullOptionalFieldsTreatedAsDefaults_ShouldBeEqual()
    {
        var a = new ExecutionContextVO("worker-01");
        var b = new ExecutionContextVO("worker-01");

        a.Should().Be(b);
    }
}
