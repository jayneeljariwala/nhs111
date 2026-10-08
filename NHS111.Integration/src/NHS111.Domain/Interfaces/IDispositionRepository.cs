namespace NHS111.Domain.Interfaces;

using NHS111.Domain.Entities;
using NHS111.Domain.Enums;

public interface IDispositionRepository
{
    Task<Disposition?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Disposition>> GetPendingAsync(CancellationToken ct = default);
    Task<IEnumerable<Disposition>> GetByNhsNumberAsync(string nhsNumber, CancellationToken ct = default);
    Task<IEnumerable<Disposition>> GetByStatusAsync(DispositionStatus status, CancellationToken ct = default);
    Task AddAsync(Disposition disposition, CancellationToken ct = default);
    Task UpdateAsync(Disposition disposition, CancellationToken ct = default);
}
