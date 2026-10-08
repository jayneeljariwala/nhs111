namespace NHS111.Contracts.Events;

using NHS111.Domain.Enums;

public record DispositionStatusUpdatedEvent(
    Guid DispositionId,
    DispositionStatus NewStatus,
    string UpdatedBy,
    string Notes,
    DateTime UpdatedAt);
