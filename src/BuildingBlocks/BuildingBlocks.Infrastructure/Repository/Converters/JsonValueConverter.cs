using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BuildingBlocks.Infrastructure.Repository.Converters;

public sealed class JsonValueConverter<T>() : ValueConverter<T, string>(
    value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
    value => JsonSerializer.Deserialize<T>(value, (JsonSerializerOptions?)null)!);
