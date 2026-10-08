namespace NHS111.Domain.Events;

using NHS111.Domain.Enums;

public class DispositionStatusChangedDomainEvent(
    Guid dispositionId,
    DispositionStatus oldStatus,
    DispositionStatus newStatus,
    DateTime occurredAt = default) : IDomainEvent
{
    public Guid DispositionId { get; } = dispositionId;
    public DispositionStatus OldStatus { get; } = oldStatus;
    public DispositionStatus NewStatus { get; } = newStatus;
    public DateTime OccurredAt { get; } = occurredAt == default ? DateTime.UtcNow : occurredAt;
}
