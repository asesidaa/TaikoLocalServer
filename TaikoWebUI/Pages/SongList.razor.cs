﻿namespace TaikoWebUI.Pages;

public partial class SongList
{
    [Parameter]
    public int Baid { get; set; }
    
    private string Search { get; set; } = string.Empty;
    private string GenreFilter { get; set; } = string.Empty;
    private string? SongNameLanguage { get; set; }

    private SongBestResponse? response;
    private UserSetting? userSetting;

    private readonly List<BreadcrumbItem> breadcrumbs = new();
    
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

        if (AuthService.IsLoggedIn && !AuthService.IsAdmin)
        {
            breadcrumbs.Add(new BreadcrumbItem(Localizer["Dashboard"], href: "/"));
        }
        else
        {
            breadcrumbs.Add(new BreadcrumbItem(Localizer["Users"], href: "/Users"));
        };
        breadcrumbs.Add(new BreadcrumbItem($"{userSetting?.MyDonName}", href: null, disabled: true));
        breadcrumbs.Add(new BreadcrumbItem(Localizer["Song List"], href: $"/Users/{Baid}/Songs", disabled: false));
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
}
