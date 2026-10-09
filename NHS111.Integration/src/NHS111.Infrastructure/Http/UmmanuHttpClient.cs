namespace NHS111.Infrastructure.Http;

using System.Net.Http.Json;
using NHS111.Contracts.Events;

public class UmmanuHttpClient(HttpClient httpClient)
{
    public async Task<bool> SendAppointmentAsync(DispositionCreatedEvent ev, CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("api/appointments", ev, ct);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
