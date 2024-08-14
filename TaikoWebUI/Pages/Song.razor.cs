namespace TaikoWebUI.Pages;

public partial class Song
{
    [Parameter]
    public int SongId { get; set; }

    [Parameter]
    public int Baid { get; set; }

    private UserSetting? userSetting;
    private SongHistoryResponse? response;
    private List<SongHistoryData>? songHistoryData;

    private string songTitle = string.Empty;
    private string songArtist = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        if (AuthService.LoginRequired && !AuthService.IsLoggedIn)
        {
            await AuthService.LoginWithAuthToken();
        }

        response = await Client.GetFromJsonAsync<SongHistoryResponse>($"api/PlayHistory/{(uint)Baid}");
        response.ThrowIfNull();
        // Get all song best data with SongId
        songHistoryData = response.SongHistoryData.Where(data => data.SongId == (uint)SongId).ToList();

        // Get user settings
        userSetting = await Client.GetFromJsonAsync<UserSetting>($"api/UserSettings/{Baid}");

        var musicDetailDictionary = await GameDataService.GetMusicDetailDictionary();

        // Get song title and artist
        var songNameLanguage = await LocalStorage.GetItemAsync<string>("songNameLanguage");
        songTitle = GameDataService.GetMusicNameBySongId(musicDetailDictionary, (uint)SongId, string.IsNullOrEmpty(songNameLanguage) ? "ja" : songNameLanguage);
        songArtist = GameDataService.GetMusicArtistBySongId(musicDetailDictionary, (uint)SongId, string.IsNullOrEmpty(songNameLanguage) ? "ja" : songNameLanguage);

        // Breadcrumbs
        var formattedSongTitle = songTitle;
        if (formattedSongTitle.Length > 20) formattedSongTitle = string.Concat(formattedSongTitle.AsSpan(0, 20), "...");

        BreadcrumbsStateContainer.breadcrumbs.Clear();
        if (AuthService.IsLoggedIn && !AuthService.IsAdmin) BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem(Localizer["Dashboard"], href: "/"));
        else BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem(Localizer["Users"], href: "/Users"));
        BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem($"{userSetting?.MyDonName}", href: null, disabled: true));
        BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem(Localizer["Song List"], href: $"/Users/{Baid}/Songs", disabled: false));
        BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem(formattedSongTitle, href: $"/Users/{Baid}/Songs/{SongId}", disabled: false));
        BreadcrumbsStateContainer.NotifyStateChanged();
    }
}