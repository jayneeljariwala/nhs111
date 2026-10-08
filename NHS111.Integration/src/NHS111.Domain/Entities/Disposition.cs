namespace NHS111.Domain.Entities;

using NHS111.Domain.Enums;

public class Disposition(
    Guid id,
    string nhsNumber,
    string patientName,
    DateTime dateOfBirth,
    string dxCode,
    string dosCode,
    UrgencyLevel urgency,
    RoutingDestination routedTo,
    DispositionStatus status = DispositionStatus.Pending,
    DateTime createdAt = default,
    DateTime updatedAt = default)
{
    public Guid Id { get; private set; } = id == Guid.Empty ? Guid.NewGuid() : id;
    public string NhsNumber { get; private set; } = nhsNumber;
    public string PatientName { get; private set; } = patientName;
    public DateTime DateOfBirth { get; private set; } = dateOfBirth;
    public string DxCode { get; private set; } = dxCode;
    public string DosCode { get; private set; } = dosCode;
    public UrgencyLevel Urgency { get; private set; } = urgency;
    public RoutingDestination RoutedTo { get; private set; } = routedTo;
    public DispositionStatus Status { get; private set; } = status;
    public DateTime CreatedAt { get; private set; } = createdAt == default ? DateTime.UtcNow : createdAt;
    public DateTime UpdatedAt { get; private set; } = updatedAt == default ? DateTime.UtcNow : updatedAt;

    public void UpdateStatus(DispositionStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(
        string? nhsNumber = null,
        string? patientName = null,
        DateTime? dateOfBirth = null,
        string? dxCode = null,
        string? dosCode = null,
        UrgencyLevel? urgency = null,
        RoutingDestination? routedTo = null,
        DispositionStatus? status = null)
    {
        if (nhsNumber != null) NhsNumber = nhsNumber;
        if (patientName != null) PatientName = patientName;
        if (dateOfBirth.HasValue) DateOfBirth = dateOfBirth.Value;
        if (dxCode != null) DxCode = dxCode;
        if (dosCode != null) DosCode = dosCode;
        if (urgency.HasValue) Urgency = urgency.Value;
        if (routedTo.HasValue) RoutedTo = routedTo.Value;
        if (status.HasValue) Status = status.Value;

        UpdatedAt = DateTime.UtcNow;
    }
}
