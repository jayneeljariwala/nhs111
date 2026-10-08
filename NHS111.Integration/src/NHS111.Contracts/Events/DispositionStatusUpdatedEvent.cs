namespace NHS111.Contracts.Events;

public record DispositionStatusUpdatedEvent(
    Guid DispositionId,
    string NewStatus,
    string UpdatedBy,
    string Notes,
    DateTime UpdatedAt);
