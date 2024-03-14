using Microsoft.JSInterop;
using TaikoWebUI.Shared.Models;

namespace TaikoWebUI.Pages;

public partial class Songs
{
    [Parameter]
    public int Baid { get; set; }

    private const string IconStyle = "width:25px; height:25px;";

    private SongBestResponse? response;
    private UserSetting? userSetting;

    private Dictionary<Difficulty, List<SongBestData>> songBestDataMap = new();

    private readonly List<BreadcrumbItem> breadcrumbs = new();

    private List<MusicDetail> musicMap = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        response = await Client.GetFromJsonAsync<SongBestResponse>($"api/PlayData/{Baid}");
        response.ThrowIfNull();

        userSetting = await Client.GetFromJsonAsync<UserSetting>($"api/UserSettings/{Baid}");

        var language = await JSRuntime.InvokeAsync<string>("blazorCulture.get");

        musicMap = GameDataService.GetMusicList();

        response.SongBestData.ForEach(data =>
        {
            var songId = data.SongId;
            data.Genre = GameDataService.GetMusicGenreBySongId(songId);
            data.MusicName = GameDataService.GetMusicNameBySongId(songId, string.IsNullOrEmpty(language) ? "ja" : language);
            data.MusicArtist = GameDataService.GetMusicArtistBySongId(songId, string.IsNullOrEmpty(language) ? "ja" : language);
        });

        songBestDataMap = response.SongBestData.GroupBy(data => data.Difficulty)
            .ToDictionary(data => data.Key,
                          data => data.ToList());
        foreach (var songBestDataList in songBestDataMap.Values)
        {
            songBestDataList.Sort((data1, data2) => GameDataService.GetMusicIndexBySongId(data1.SongId)
                                      .CompareTo(GameDataService.GetMusicIndexBySongId(data2.SongId)));
        }

        if (LoginService.IsLoggedIn && !LoginService.IsAdmin)
        {
            breadcrumbs.Add(new BreadcrumbItem("Dashboard", href: "/"));
        }
        else
        {
            breadcrumbs.Add(new BreadcrumbItem("Users", href: "/Users"));
        };
        breadcrumbs.Add(new BreadcrumbItem($"{userSetting?.MyDonName}", href: null, disabled: true));
        breadcrumbs.Add(new BreadcrumbItem("Songs", href: $"/Users/{Baid}/Songs", disabled: false));
    }

    private async Task OnFavoriteToggled(SongBestData data)
    {
        var request = new SetFavoriteRequest
        {
            Baid = (uint)Baid,
            IsFavorite = !data.IsFavorite,
            SongId = data.SongId
        };
        var result = await Client.PostAsJsonAsync("api/FavoriteSongs", request);
        if (result.IsSuccessStatusCode)
        {
            data.IsFavorite = !data.IsFavorite;
        }
    }

    private static void ToggleShowAiData(SongBestData data)
    {
        data.ShowAiData = !data.ShowAiData;
    }
}
