namespace NHS111.Infrastructure.Persistence.Repositories;

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NHS111.Domain.Entities;
using NHS111.Domain.Enums;
using NHS111.Domain.Interfaces;
using StackExchange.Redis;

public class DispositionRepository(
    NHS111DbContext dbContext,
    IConnectionMultiplexer redis) : IDispositionRepository
{
    private readonly IDatabase _redisDb = redis.GetDatabase();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<Disposition?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        string cacheKey = $"disposition:{id}";
        var cached = await _redisDb.StringGetAsync(cacheKey);
        if (cached.HasValue)
        {
            var cachedItem = JsonSerializer.Deserialize<Disposition>(cached.ToString(), JsonOptions);
            if (cachedItem != null)
            {
                return cachedItem;
            }
        }

        var disposition = await dbContext.Dispositions.FirstOrDefaultAsync(d => d.Id == id, ct);
        if (disposition != null)
        {
            var serialized = JsonSerializer.Serialize(disposition, JsonOptions);
            await _redisDb.StringSetAsync(cacheKey, serialized, TimeSpan.FromMinutes(5));
        }

        return disposition;
    }

    public async Task<IEnumerable<Disposition>> GetPendingAsync(CancellationToken ct = default)
    {
        string cacheKey = "dispositions:pending";
        var cached = await _redisDb.StringGetAsync(cacheKey);
        if (cached.HasValue)
        {
            var cachedList = JsonSerializer.Deserialize<List<Disposition>>(cached.ToString(), JsonOptions);
            if (cachedList != null)
            {
                return cachedList;
            }
        }

        var pendingList = await dbContext.Dispositions
            .Where(d => d.Status == DispositionStatus.Pending)
            .ToListAsync(ct);

        var serialized = JsonSerializer.Serialize(pendingList, JsonOptions);
        await _redisDb.StringSetAsync(cacheKey, serialized, TimeSpan.FromMinutes(1));

        return pendingList;
    }

    public async Task<IEnumerable<Disposition>> GetByNhsNumberAsync(string nhsNumber, CancellationToken ct = default)
    {
        return await dbContext.Dispositions
            .Where(d => d.NhsNumber == nhsNumber)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Disposition>> GetByStatusAsync(DispositionStatus status, CancellationToken ct = default)
    {
        return await dbContext.Dispositions
            .Where(d => d.Status == status)
            .ToListAsync(ct);
    }

    public async Task AddAsync(Disposition disposition, CancellationToken ct = default)
    {
        await dbContext.Dispositions.AddAsync(disposition, ct);
        await dbContext.SaveChangesAsync(ct);
        await _redisDb.KeyDeleteAsync("dispositions:pending");
    }

    public async Task UpdateAsync(Disposition disposition, CancellationToken ct = default)
    {
        dbContext.Dispositions.Update(disposition);
        await dbContext.SaveChangesAsync(ct);
        await _redisDb.KeyDeleteAsync($"disposition:{disposition.Id}");
        await _redisDb.KeyDeleteAsync("dispositions:pending");
    }
}
