namespace BuildingBlocks.Application.Abstractions;

public interface IUserContext
{

    Guid UserId { get; }

    Guid? UserIdOrDefault { get; }

    string? Email { get; }

    string? DisplayName { get; }

    bool IsAuthenticated { get; }

    string? IpAddress { get; }

    string? UserAgent { get; }

    string? CorrelationId { get; }
}
