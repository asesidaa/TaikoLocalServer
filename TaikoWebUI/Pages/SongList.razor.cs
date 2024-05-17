using System.Reflection.Emit;
using Microsoft.JSInterop;
using TaikoWebUI.Shared.Models;


namespace TaikoWebUI.Pages;

public partial class SongList
{
    [Parameter]
    public int Baid { get; set; }
    
    private string Search { get; set; } = string.Empty;
    private string GenreFilter { get; set; } = string.Empty;
    private string CurrentLanguage { get; set; } = "ja";

    private SongBestResponse? response;
    private UserSetting? userSetting;

    private readonly List<BreadcrumbItem> breadcrumbs = new();

    private List<MusicDetail> musicMap = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        response = await Client.GetFromJsonAsync<SongBestResponse>($"api/PlayData/{Baid}");
        response.ThrowIfNull();

        userSetting = await Client.GetFromJsonAsync<UserSetting>($"api/UserSettings/{Baid}");
        musicMap = GameDataService.GetMusicList();

        CurrentLanguage = await JsRuntime.InvokeAsync<string>("blazorCulture.get");

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
