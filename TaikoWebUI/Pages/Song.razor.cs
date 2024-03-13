using Microsoft.JSInterop;

namespace TaikoWebUI.Pages
{
    public partial class Song
    {
        [Parameter]
        public int SongId { get; set; }

        [Parameter]
        public int Baid { get; set; }

        private UserSetting? userSetting;
        private SongBestResponse? response;
        private SongBestData? SongBestData;
        private List<BreadcrumbItem> breadcrumbs = new List<BreadcrumbItem>();

        private string SongTitle = string.Empty;
        private string SongArtist = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            response = await Client.GetFromJsonAsync<SongBestResponse>($"api/PlayData/{Baid}");
            response.ThrowIfNull();
            SongBestData = response.SongBestData.FirstOrDefault(x => x.SongId == SongId);

            // Get user settings
            userSetting = await Client.GetFromJsonAsync<UserSetting>($"api/UserSettings/{Baid}");

            // Get song title and artist
            var language = await JSRuntime.InvokeAsync<string>("blazorCulture.get");
            SongTitle = GameDataService.GetMusicNameBySongId((uint)SongId, string.IsNullOrEmpty(language) ? "ja" : language);
            SongArtist = GameDataService.GetMusicArtistBySongId((uint)SongId, string.IsNullOrEmpty(language) ? "ja" : language);

            // Breadcrumbs
            var _songTitle = SongTitle;
            if (_songTitle.Length > 20)
            {
                _songTitle = _songTitle.Substring(0, 20) + "...";
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
            breadcrumbs.Add(new BreadcrumbItem(_songTitle, href: $"/Users/{Baid}/Songs/{SongId}", disabled: false));
        }
    }
}
