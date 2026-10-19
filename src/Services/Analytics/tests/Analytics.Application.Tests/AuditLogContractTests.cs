using Analytics.Application.Mappers;
using Analytics.Domain.Entities;
using Analytics.Domain.Enums;
using System.Text.Json;
using Xunit;

namespace Analytics.Application.Tests;

public class AuditLogContractTests
{
    [Theory]
    [InlineData(AuditAction.Created)]
    [InlineData(AuditAction.Updated)]
    [InlineData(AuditAction.Deleted)]
    [InlineData(AuditAction.SoftDeleted)]
    [InlineData(AuditAction.Restored)]
    public void Response_UsesNamedActionsAndPreservesStructuredMetadata(AuditAction action)
    {
        var log = new AuditLog
        {
            Action = action,
            Metadata = """{"Reason":"cancelled by seller","Price":100,"Nested":{"Allowed":true}}""",
            ChangedProperties = """["Status"]"""
        };
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(log.ToDto(), new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        Assert.Equal(action.ToString(), json.RootElement.GetProperty("action").GetString());
        var metadata = json.RootElement.GetProperty("metadata");
        Assert.Equal("cancelled by seller", metadata.GetProperty("Reason").GetString());
        Assert.Equal(100, metadata.GetProperty("Price").GetInt32());
        Assert.True(metadata.GetProperty("Nested").GetProperty("Allowed").GetBoolean());
        Assert.Equal("Status", json.RootElement.GetProperty("changedProperties")[0].GetString());
    }

    [Fact]
    public void LegacyLog_WithoutMetadataCanStillBeRead() =>
        Assert.Null(new AuditLog().ToDto().Metadata);
}
