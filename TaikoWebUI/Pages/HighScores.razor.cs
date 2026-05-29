using TaikoWebUI.Utilities;
namespace TaikoWebUI.Pages;

public partial class HighScores
{
    [Parameter]
    public int Baid { get; set; }

    [Parameter]
    public string? Era { get; set; }

    private string CurrentEra => WebUiEra.Normalize(Era);
    private bool IsAc15 => WebUiEra.IsAc15(CurrentEra);

    private const string IconStyle = "width:25px; height:25px;";

    private SongBestResponse? response;
    private UserSetting? userSetting;
    private Dictionary<Difficulty, List<SongBestData>> songBestDataMap = new();
    private int selectedDifficultyTab;
    private Dictionary<uint, MusicDetail> musicDetailDictionary = new();

    private string Search { get; set; } = string.Empty;
    private string GenreFilter { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        response = await Client.GetFromJsonAsync<SongBestResponse>(WebUiEra.Api(CurrentEra, $"PlayData/{Baid}"));
        response.ThrowIfNull();

        userSetting = await Client.GetFromJsonAsync<UserSetting>(WebUiEra.Api(CurrentEra, $"UserSettings/{Baid}"));

        var songNameLanguage = await LocalStorage.GetItemAsync<string>("songNameLanguage");
        musicDetailDictionary = await GameDataService.GetMusicDetailDictionary(CurrentEra);

        response.SongBestData.ForEach(data =>
        {
            var songId = data.SongId;
            data.Genre = GameDataService.GetMusicGenreBySongId(musicDetailDictionary, songId);
            data.MusicName = GameDataService.GetMusicNameBySongId(musicDetailDictionary, songId, string.IsNullOrEmpty(songNameLanguage) ? "ja" : songNameLanguage);
            data.MusicArtist = GameDataService.GetMusicArtistBySongId(musicDetailDictionary, songId, string.IsNullOrEmpty(songNameLanguage) ? "ja" : songNameLanguage);
        });

        //We get rid of entries with empty song names
        //This can happen if the database has been used with a different version of the songlist (Omnimix/Version update)
        response.SongBestData = response.SongBestData.FindAll(x => x.MusicName != "");

        songBestDataMap = response.SongBestData.GroupBy(data => data.Difficulty)
            .ToDictionary(data => data.Key,
                          data => data.ToList());



        foreach (var songBestDataList in songBestDataMap.Values)
        {
            songBestDataList.Sort((data1, data2) => GameDataService.GetMusicIndexBySongId(musicDetailDictionary, data1.SongId)
                                      .CompareTo(GameDataService.GetMusicIndexBySongId(musicDetailDictionary, data2.SongId)));
        }

        // Set last selected tab from local storage
        selectedDifficultyTab = await LocalStorage.GetItemAsync<int>($"highScoresTab");

        // Breadcrumbs
        BreadcrumbsStateContainer.breadcrumbs.Clear();
        if (AuthService.IsLoggedIn && !AuthService.IsAdmin)
        {
            BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem(Localizer["Dashboard"], href: "/"));
        }
        else
        {
            BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem(Localizer["Users"], href: "/Users"));
        }
        BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem($"{userSetting?.MyDonName}", href: null, disabled: true));
        BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem(Localizer["High Scores"], href: WebUiEra.UserRoute(Baid, CurrentEra, "HighScores"), disabled: false));
        BreadcrumbsStateContainer.NotifyStateChanged();
    }

    private async Task OnFavoriteToggled(SongBestData data)
    {
        if (IsAc15 && !data.IsFavorite && CountCurrentFavorites() >= 5)
        {
            await DialogService.ShowMessageBoxAsync(
                Localizer["Error"],
                "AC15 eras support at most 5 favorite songs.",
                Localizer["Dialog OK"]);
            return;
        }

        var request = new SetFavoriteRequest
        {
            Baid = (uint)Baid,
            IsFavorite = !data.IsFavorite,
            SongId = data.SongId
        };
        var result = await Client.PostAsJsonAsync(WebUiEra.Api(CurrentEra, "FavoriteSongs"), request);
        if (result.IsSuccessStatusCode)
        {
            data.IsFavorite = !data.IsFavorite;
        }
    }

    private int CountCurrentFavorites()
    {
        return response?.SongBestData.Where(data => data.IsFavorite).Select(data => data.SongId).Distinct().Count() ?? 0;
    }

    private async Task OnTabChanged(int index)
    {
        selectedDifficultyTab = index;
        await LocalStorage.SetItemAsync($"highScoresTab", selectedDifficultyTab);
    }

    private bool FilterSongs(SongBestData songData)
    {
        var stringsToCheck = new List<string>
        {
            songData.MusicName,
            songData.MusicArtist,
            songData.BestScore.ToString(),
            songData.PlayTime.ToString(),
        };

        if (songData.IsFavorite) stringsToCheck.Add("Favorite");

        if (!string.IsNullOrEmpty(Search) && !stringsToCheck.Any(s => s.Contains(Search, StringComparison.OrdinalIgnoreCase))) return false;
        if (!string.IsNullOrEmpty(GenreFilter) && songData.Genre != Enum.Parse<SongGenre>(GenreFilter)) return false;

        return true;
    }
    private static string GetSpeedIcon(PlaySetting playSetting)
    {
        return $"<image href='/images/Speed/{playSetting.Speed}.png' alt='{playSetting.Speed}' width='25' height='25'/>";
    }

    private static string GetVanishIcon(PlaySetting playSetting)
    {
        if (playSetting.IsVanishOn) return $"<image href='/images/Doron.png' alt='vanish' width='25' height='25'/>";
        return "";
    }

    private static string GetInverseIcon(PlaySetting playSetting)
    {
        if (playSetting.IsInverseOn) return $"<image href='/images/Mirror.png' alt='inverse' width='25' height='25'/>";
        return "";
    }

    private static string GetRandomIcon(PlaySetting playSetting)
    {
        if (playSetting.RandomType != 0) return $"<image href='/images/Random_{playSetting.RandomType}.png' alt='{playSetting.RandomType}' width='25' height='25'/>";
        return "";
    }
}
