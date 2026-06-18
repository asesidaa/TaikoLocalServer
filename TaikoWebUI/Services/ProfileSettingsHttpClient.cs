using System.Net.Http.Json;
using TaikoWebUI.Utilities;

namespace TaikoWebUI.Services;

public static class ProfileSettingsHttpClient
{
    public static async Task<string?> GetProfileDisplayNameAsync(
        this HttpClient client,
        string era,
        uint baid,
        CancellationToken cancellationToken = default)
    {
        if (WebUiEra.IsAc15(era))
        {
            var ac15 = await client.GetFromJsonAsync<Ac15ProfileSettingsDto>(
                WebUiEra.Api(era, $"Ac15ProfileSettings/{baid}"),
                cancellationToken);
            return ac15?.Identity.MyDonName;
        }

        var nijiiro = await client.GetFromJsonAsync<UserSetting>(
            WebUiEra.Api(era, $"UserSettings/{baid}"),
            cancellationToken);
        return nijiiro?.MyDonName;
    }
}
