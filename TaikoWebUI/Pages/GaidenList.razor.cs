namespace TaikoWebUI.Pages;

public partial class GaidenList
{
    [Parameter]
    public int Baid { get; set; }
    
    private string Search { get; set; } = string.Empty;
    private string? SongNameLanguage { get; set; }

    private SongBestResponse? response;
    private UserSetting? userSetting;
    
    private Dictionary<uint, MusicDetail> musicDetailDictionary = new();
    private List<DanData> danDatas = new ();

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

    private bool FilterGaidens(DanData danData)
    {
        var stringsToCheck = GetGaidenTitles(danData.Title);

        if (!string.IsNullOrEmpty(Search) && !stringsToCheck.Any(s => s.Contains(Search, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return true;
    }

    private List<string> GetGaidenTitles(string gaidenTitle)
    {
        List<string> titles = new ();
        foreach (var langTitle in gaidenTitle.Split(","))
        {
            if (langTitle.Contains("[JPN]")) titles.Add(langTitle.Replace("[JPN]", "").Trim());
            else if (langTitle.Contains("[ENG]")) titles.Add(langTitle.Replace("[ENG]", "").Trim());
            else if (langTitle.Contains("[CHN]")) titles.Add(langTitle.Replace("[CHN]", "").Trim());
            else if (langTitle.Contains("[KOR]")) titles.Add(langTitle.Replace("[KOR]", "").Trim());
            else if (langTitle.Contains("[CHS]")) titles.Add(langTitle.Replace("[CHS]", "").Trim());
            else titles.Add(langTitle);
        }
        return titles;
    }

    private string GetGaidenTitle(string gaidenTitle, string? language)
    {
        Dictionary<string, string> titleMap = new (); 
        foreach (var langTitle in gaidenTitle.Split(","))
        {
            if (langTitle.Contains("[JPN]")) titleMap["default"] = titleMap["JPN"] = langTitle.Replace("[JPN]", "").Trim();
            else if (langTitle.Contains("[ENG]")) titleMap["default"] = titleMap["ENG"] = langTitle.Replace("[ENG]", "").Trim();
            else if (langTitle.Contains("[CHN]")) titleMap["default"] = titleMap["CHN"] = langTitle.Replace("[CHN]", "").Trim();
            else if (langTitle.Contains("[KOR]")) titleMap["default"] = titleMap["KOR"] = langTitle.Replace("[KOR]", "").Trim();
            else if (langTitle.Contains("[CHS]")) titleMap["default"] = titleMap["CHS"] = langTitle.Replace("[CHS]", "").Trim();
            else titleMap["default"] = langTitle;
        }
        if (titleMap.ContainsKey("JPN")) titleMap["default"] = titleMap["JPN"];
        if (language == null || language == "ja")
        {
            if (titleMap.ContainsKey("JPN")) return titleMap["JPN"];
            else return titleMap["default"];
        }
        else if (language == "en-US" || language == "fr-FR")
        {
            if (titleMap.ContainsKey("ENG")) return titleMap["ENG"];
            else return titleMap["default"];
        }
        else if (language == "zh-Hans")
        {
            if (titleMap.ContainsKey("CHS")) return titleMap["CHS"];
            else return titleMap["default"];
        }
        else if (language == "zh-Hant")
        {
            if (titleMap.ContainsKey("CHN")) return titleMap["CHN"];
            else return titleMap["default"];
        }
        else if (language == "ko")
        {
            if (titleMap.ContainsKey("KOR")) return titleMap["KOR"];
            else return titleMap["default"];
        }

        return titleMap["default"];
    }
}
