namespace NHS111.Contracts.Events;

public record DispositionCreatedEvent(
    Guid DispositionId,
    string NhsNumber,
    string PatientName,
    DateTime DateOfBirth,
    string DxCode,
    string DosCode,
    string Urgency,
    string RoutedTo,
    DateTime CreatedAt);
