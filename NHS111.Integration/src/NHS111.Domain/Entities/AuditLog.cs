namespace NHS111.Domain.Entities;

public class AuditLog(
    Guid id,
    Guid dispositionId,
    string action,
    DateTime performedAt,
    string details,
    string performedBy)
{
    public Guid Id { get; private set; } = id == Guid.Empty ? Guid.NewGuid() : id;
    public Guid DispositionId { get; private set; } = dispositionId;
    public string Action { get; private set; } = action;
    public DateTime PerformedAt { get; private set; } = performedAt == default ? DateTime.UtcNow : performedAt;
    public string Details { get; private set; } = details;
    public string PerformedBy { get; private set; } = performedBy;

    public void Update(
        string? action = null,
        DateTime? performedAt = null,
        string? details = null,
        string? performedBy = null)
    {
        if (action != null) Action = action;
        if (performedAt.HasValue) PerformedAt = performedAt.Value;
        if (details != null) Details = details;
        if (performedBy != null) PerformedBy = performedBy;
    }
}
