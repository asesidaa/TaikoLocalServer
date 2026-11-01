using System.Net;

namespace TaikoWebUI.Services;

/// <summary>
/// Whether the operator has run tools/extract: checks the index files it writes next to the UI art.
/// Only a 404 counts as missing (a 401 just means the user isn't logged in yet).
/// </summary>
public sealed class AssetStatus(HttpClient client)
{
    private static readonly string[] Indexes = ["api/Assets/nameplate/plates.json", "api/Assets/scoreboard/scoreboard.json"];

    private Task<bool>? missing;

    /// <param name="refresh">Check again (after running extract) instead of the answer from earlier.</param>
    public Task<bool> IsMissingAsync(bool refresh = false) => refresh ? missing = CheckAsync() : missing ??= CheckAsync();

    private async Task<bool> CheckAsync()
    {
        foreach (var url in Indexes)
        {
            try
            {
                using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                if (response.StatusCode == HttpStatusCode.NotFound) return true;
            }
            catch (HttpRequestException)
            {
                // Server unreachable: not the extractor's fault.
            }
        }
        return false;
    }
}
