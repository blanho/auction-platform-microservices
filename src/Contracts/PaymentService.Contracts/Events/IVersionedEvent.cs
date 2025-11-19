namespace PaymentService.Contracts.Events;

public interface IVersionedEvent
{
    int Version { get; }
}
