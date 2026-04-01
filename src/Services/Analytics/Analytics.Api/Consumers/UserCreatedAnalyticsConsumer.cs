using Analytics.Api.Data;
using Analytics.Api.Entities;
using IdentityService.Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Api.Consumers;

public class UserCreatedAnalyticsConsumer : IConsumer<UserCreatedEvent>
{
    private readonly AnalyticsDbContext _context;
    private readonly ILogger<UserCreatedAnalyticsConsumer> _logger;

    public UserCreatedAnalyticsConsumer(
        AnalyticsDbContext context,
        ILogger<UserCreatedAnalyticsConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var @event = context.Message;

        var exists = await _context.FactUsers
            .AnyAsync(f => f.UserId == Guid.Parse(@event.UserId) && f.EventType == "Registered",
                context.CancellationToken);

        if (exists)
        {
            _logger.LogWarning(
                "Duplicate UserCreated event skipped for User {UserId}",
                @event.UserId);
            return;
        }

        var fact = new FactUser
        {
            EventId = Guid.NewGuid(),
            UserId = Guid.Parse(@event.UserId),
            EventTime = @event.CreatedAt,
            IngestedAt = DateTimeOffset.UtcNow,
            DateKey = DateOnly.FromDateTime(@event.CreatedAt.UtcDateTime),

            Username = @event.Username,
            Email = @event.Email,
            EmailConfirmed = @event.EmailConfirmed,
            Role = @event.Role,
            FullName = @event.FullName,

            EventType = "Registered",
            EventVersion = (short)@event.Version
        };

        _context.FactUsers.Add(fact);
        await _context.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Recorded UserCreated fact for {UserId}, Username: {Username}, Role: {Role}",
            @event.UserId, @event.Username, @event.Role);
    }
}
