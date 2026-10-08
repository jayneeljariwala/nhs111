namespace NHS111.Domain.Events;

using NHS111.Domain.Enums;

public class DispositionCreatedDomainEvent(
    Guid dispositionId,
    RoutingDestination routedTo,
    DateTime occurredAt = default) : IDomainEvent
{
    public Guid DispositionId { get; } = dispositionId;
    public RoutingDestination RoutedTo { get; } = routedTo;
    public DateTime OccurredAt { get; } = occurredAt == default ? DateTime.UtcNow : occurredAt;
}
