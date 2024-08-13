namespace TaikoWebUI.Pages;

public partial class SongList
{
    [Parameter]
    public int Baid { get; set; }
    
    private string Search { get; set; } = string.Empty;
    private string GenreFilter { get; set; } = string.Empty;
    private string? SongNameLanguage { get; set; }

    private SongBestResponse? response;
    private UserSetting? userSetting;
    
    private Dictionary<uint, MusicDetail> musicDetailDictionary = new();

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
        musicDetailDictionary = await GameDataService.GetMusicDetailDictionary();

        SongNameLanguage = await LocalStorage.GetItemAsync<string>("songNameLanguage");

        foreach (var best in response.SongBestData)
            foreach (var song in musicDetailDictionary)
                if (best.SongId == song.Value.SongId)
                    song.Value.IsFavorite = best.IsFavorite;

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
        BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem(Localizer["Song List"], href: $"/Users/{Baid}/Songs", disabled: false));
        BreadcrumbsStateContainer.NotifyStateChanged();
    }

    private bool FilterSongs(MusicDetail musicDetail)
    {
        var stringsToCheck = new List<string>
        {
            musicDetail.SongName,
            musicDetail.SongNameEN,
            musicDetail.SongNameCN,
            musicDetail.SongNameKO,
            musicDetail.ArtistName,
            musicDetail.ArtistNameEN,
            musicDetail.ArtistNameCN,
            musicDetail.ArtistNameKO
        };

        if (musicDetail.IsFavorite) stringsToCheck.Add("Favorite");

        if (!string.IsNullOrEmpty(Search) && !stringsToCheck.Any(s => s.Contains(Search, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(GenreFilter) && musicDetail.Genre != Enum.Parse<SongGenre>(GenreFilter))
        {
            return false;
        }

        return true;
    }
    
    private async Task OnFavoriteToggled(MusicDetail data)
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
}
