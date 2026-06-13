using TaikoWebUI.Utilities;
namespace TaikoWebUI.Pages;

public partial class SongList
{
    [Parameter]
    public int Baid { get; set; }

    [Parameter]
    public string? Era { get; set; }

    private string CurrentEra => WebUiEra.NormalizeOrDefault(Era, AuthService.DefaultEra);
    private bool IsAc15 => WebUiEra.IsAc15(CurrentEra);

    private string Search { get; set; } = string.Empty;
    private string GenreFilter { get; set; } = string.Empty;
    private string? SongNameLanguage { get; set; }

    private SongBestResponse? response;
    private UserSetting? userSetting;

    private Dictionary<uint, MusicDetail> musicDetailDictionary = new();

    private MudTable<MusicDetail>? _table;
    private int currentPage = 0;
    private int rowsPerPage = 25;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        response = await Client.GetFromJsonAsync<SongBestResponse>(WebUiEra.Api(CurrentEra, $"PlayData/{Baid}"));
        response.ThrowIfNull();

        userSetting = await Client.GetFromJsonAsync<UserSetting>(WebUiEra.Api(CurrentEra, $"UserSettings/{Baid}"));
        musicDetailDictionary = await GameDataService.GetMusicDetailDictionary(CurrentEra);

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
        }
        ;
        BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem($"{userSetting?.MyDonName}", href: null, disabled: true));
        BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem(Localizer["Song List"], href: WebUiEra.UserRoute(Baid, CurrentEra, "Songs"), disabled: false));
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
        return musicDetailDictionary.Values.Count(data => data.IsFavorite);
    }

    private void OnCurrentPageChanged(int page)
    {
        currentPage = page;
    }

    private void OnRowsPerPageChanged(int pageSize)
    {
        rowsPerPage = pageSize;
    }
}
