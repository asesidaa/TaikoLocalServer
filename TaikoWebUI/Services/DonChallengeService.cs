using System.Net;
using System.Net.Http.Json;
using TaikoLocalServer.Contracts.AdminApi.Responses;
using TaikoWebUI.Utilities;

namespace TaikoWebUI.Services;

public sealed class DonChallengeService(HttpClient client)
{
    private readonly Dictionary<string, DonChallengeAvailabilityResponse> availabilityCache = new(StringComparer.OrdinalIgnoreCase);

    public Task<DonChallengeAvailabilityResponse> GetAvailabilityAsync(string? era)
    {
        var normalized = WebUiEra.Normalize(era);
        if (availabilityCache.TryGetValue(normalized, out var cached))
        {
            return Task.FromResult(cached);
        }

        return GetAvailabilityCoreAsync(normalized);
    }

    private async Task<DonChallengeAvailabilityResponse> GetAvailabilityCoreAsync(string normalized)
    {
        var response = await GetJsonOrUnavailableAsync(
            WebUiEra.Api(normalized, "DonChallenge/availability"),
            () => new DonChallengeAvailabilityResponse
            {
                Era = normalized,
                IsAvailable = false,
                Message = BuildUnavailableMessage(normalized)
            });

        availabilityCache[normalized] = response;
        return response;
    }

    public Task<DonChallengeResponse> GetDonChallengeAsync(string? era, int baid)
        => GetDonChallengeAsync(era, (uint)baid);

    public Task<DonChallengeResponse> GetDonChallengeAsync(string? era, uint baid)
    {
        var normalized = WebUiEra.Normalize(era);
        return GetJsonOrUnavailableAsync(
            WebUiEra.Api(normalized, $"DonChallenge/{baid}"),
            () => new DonChallengeResponse
            {
                Era = normalized,
                IsAvailable = false,
                Message = BuildUnavailableMessage(normalized)
            });
    }

    private static string BuildUnavailableMessage(string era)
        => $"Don Challenge is not available for {era}.";

    private async Task<T> GetJsonOrUnavailableAsync<T>(string path, Func<T> unavailableFactory)
        where T : class
    {
        try
        {
            return await client.GetFromJsonAsync<T>(path) ?? unavailableFactory();
        }
        catch (HttpRequestException exception)
            when (exception.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound)
        {
            return unavailableFactory();
        }
    }
}
