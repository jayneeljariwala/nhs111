namespace NHS111.Contracts.Events;

using NHS111.Domain.Enums;

public record DispositionCreatedEvent(
    Guid DispositionId,
    string NhsNumber,
    string PatientName,
    DateTime DateOfBirth,
    string DxCode,
    string DosCode,
    UrgencyLevel Urgency,
    RoutingDestination RoutedTo,
    DateTime CreatedAt);
