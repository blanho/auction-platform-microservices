using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BuildingBlocks.Infrastructure.Repository.Converters;

public class DateTimeOffsetUtcConverter : ValueConverter<DateTimeOffset, DateTimeOffset>
{
    public DateTimeOffsetUtcConverter()
        : base(
            dto => dto.ToUniversalTime(),
            dto => dto.ToUniversalTime())
    {
    }
}

public class NullableDateTimeOffsetUtcConverter : ValueConverter<DateTimeOffset?, DateTimeOffset?>
{
    public NullableDateTimeOffsetUtcConverter()
        : base(
            dto => dto.HasValue ? dto.Value.ToUniversalTime() : null,
            dto => dto.HasValue ? dto.Value.ToUniversalTime() : null)
    {
    }
}
