namespace NHS111.Domain.Interfaces;

using NHS111.Domain.Entities;

public interface IAuditRepository
{
    Task LogAsync(AuditLog log, CancellationToken ct = default);
    Task<IEnumerable<AuditLog>> GetByDispositionIdAsync(Guid dispositionId, CancellationToken ct = default);
}
