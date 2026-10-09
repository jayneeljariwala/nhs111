namespace NHS111.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using NHS111.Domain.Entities;
using NHS111.Domain.Interfaces;

public class AuditRepository(NHS111DbContext dbContext) : IAuditRepository
{
    public async Task LogAsync(AuditLog log, CancellationToken ct = default)
    {
        await dbContext.AuditLogs.AddAsync(log, ct);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<AuditLog>> GetByDispositionIdAsync(Guid dispositionId, CancellationToken ct = default)
    {
        return await dbContext.AuditLogs
            .Where(a => a.DispositionId == dispositionId)
            .OrderByDescending(a => a.PerformedAt)
            .ToListAsync(ct);
    }
}
