namespace TaikoWebUI.Pages;

public partial class HighScores
{
    [Parameter]
    public int Baid { get; set; }

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
        
        if (AuthService.LoginRequired && !AuthService.IsLoggedIn)
        {
            await AuthService.LoginWithAuthToken();
        }
        
        response = await Client.GetFromJsonAsync<SongBestResponse>($"api/PlayData/{Baid}");
        response.ThrowIfNull();

        userSetting = await Client.GetFromJsonAsync<UserSetting>($"api/UserSettings/{Baid}");

        var songNameLanguage = await LocalStorage.GetItemAsync<string>("songNameLanguage");
        musicDetailDictionary = await GameDataService.GetMusicDetailDictionary();

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
        };
        BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem($"{userSetting?.MyDonName}", href: null, disabled: true));
        BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem(Localizer["High Scores"], href: $"/Users/{Baid}/HighScores", disabled: false));
        BreadcrumbsStateContainer.NotifyStateChanged();
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

        if (!string.IsNullOrEmpty(Search) && !stringsToCheck.Any(s => s.Contains(Search, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(GenreFilter) && songData.Genre != Enum.Parse<SongGenre>(GenreFilter))
        {
            return false;
        }

        return true;
    }
}
