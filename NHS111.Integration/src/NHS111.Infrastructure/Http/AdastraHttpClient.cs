namespace NHS111.Infrastructure.Http;

using System.Net.Http.Json;
using NHS111.Contracts.Events;

public class AdastraHttpClient(HttpClient httpClient)
{
    public async Task<bool> SendReferralAsync(DispositionCreatedEvent ev, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/referrals", ev, ct);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ResolveReferralAsync(Guid dispositionId, string resolvedBy, CancellationToken ct = default)
    {
        try
        {
            var payload = new { resolvedBy };
            var response = await httpClient.PutAsJsonAsync($"api/referrals/{dispositionId}/resolve", payload, ct);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
