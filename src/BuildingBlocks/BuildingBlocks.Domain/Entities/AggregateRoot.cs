using System.ComponentModel.DataAnnotations;

namespace BuildingBlocks.Domain.Entities;

public abstract class AggregateRoot : BaseEntity
{
    [Timestamp]
    public uint Version { get; protected set; }
}
