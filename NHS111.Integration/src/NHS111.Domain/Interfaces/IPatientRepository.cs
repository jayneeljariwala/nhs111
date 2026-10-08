namespace NHS111.Domain.Interfaces;

using NHS111.Domain.Entities;

public interface IPatientRepository
{
    Task<Patient?> GetByNhsNumberAsync(string nhsNumber, CancellationToken ct = default);
    Task AddAsync(Patient patient, CancellationToken ct = default);
    Task UpdateAsync(Patient patient, CancellationToken ct = default);
}
