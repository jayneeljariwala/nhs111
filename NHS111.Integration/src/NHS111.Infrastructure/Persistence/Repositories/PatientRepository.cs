namespace NHS111.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using NHS111.Domain.Entities;
using NHS111.Domain.Interfaces;

public class PatientRepository(NHS111DbContext dbContext) : IPatientRepository
{
    public async Task<Patient?> GetByNhsNumberAsync(string nhsNumber, CancellationToken ct = default)
    {
        return await dbContext.Patients
            .FirstOrDefaultAsync(p => p.NhsNumber == nhsNumber, ct);
    }

    public async Task AddAsync(Patient patient, CancellationToken ct = default)
    {
        await dbContext.Patients.AddAsync(patient, ct);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Patient patient, CancellationToken ct = default)
    {
        dbContext.Patients.Update(patient);
        await dbContext.SaveChangesAsync(ct);
    }
}
