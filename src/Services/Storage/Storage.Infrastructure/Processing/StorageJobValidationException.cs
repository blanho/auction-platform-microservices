namespace Storage.Infrastructure.Processing;

public sealed class StorageJobValidationException(string message) : Exception(message);
