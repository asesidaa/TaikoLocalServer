using Microsoft.Extensions.Options;
using System.Collections.Immutable;
using TaikoWebUI.Settings;

namespace TaikoWebUI.Pages;

public partial class Gaiden
{
    [Inject]
    IOptions<WebUiSettings> UiSettings { get; set; } = default!;

    [Parameter]
    public int Baid { get; set; }

    [Parameter]
    public int danId { get; set; }

    private string? SongNameLanguage { get; set; }

    private DanBestDataResponse? response;
    private UserSetting? userSetting;

    private static Dictionary<uint, DanBestData> _bestDataMap = new();
    private Dictionary<uint, MusicDetail> musicDetailDictionary = new();
    private ImmutableDictionary<uint, DanData> danMap = ImmutableDictionary<uint, DanData>.Empty;
    private Dictionary<uint, DanData> danMapTemp = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        if (AuthService.LoginRequired && !AuthService.IsLoggedIn)
        {
            await AuthService.LoginWithAuthToken();
        }

        response = await Client.GetFromJsonAsync<DanBestDataResponse>($"api/DanBestData/gaiden/{Baid}");
        response.ThrowIfNull();
        response.DanBestDataList.ForEach(data => data.DanBestStageDataList
            .Sort((stageData, otherStageData) => stageData.SongNumber.CompareTo(otherStageData.SongNumber)));
        
        _bestDataMap = response.DanBestDataList.ToDictionary(data => data.DanId);
        danMap = GameDataService.GetGaidenMap();

        if (!UiSettings.Value.DisplayUnplayedDans)
        {
            foreach (var best in _bestDataMap)
            {
                var value = danMap.First(dan => dan.Key == best.Key);
                danMapTemp.Add(value.Key, value.Value);
            }
            danMap = danMapTemp.ToImmutableDictionary();
        }


        SongNameLanguage = await LocalStorage.GetItemAsync<string>("songNameLanguage");

        userSetting = await Client.GetFromJsonAsync<UserSetting>($"api/UserSettings/{Baid}");

        musicDetailDictionary = await GameDataService.GetMusicDetailDictionary();

        // Breadcrumbs
        BreadcrumbsStateContainer.breadcrumbs.Clear();
        if (AuthService.IsLoggedIn && !AuthService.IsAdmin) BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem(Localizer["Dashboard"], href: "/"));
        else BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem(Localizer["Users"], href: "/Users"));
        BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem($"{userSetting?.MyDonName}", href: null, disabled: true));
        BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem(Localizer["Gaiden"], href: $"/Users/{Baid}/Gaidens", disabled: true));
        BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem(GetDanTitle(danMap[(uint) danId].Title, SongNameLanguage), href: $"/Users/{Baid}/Gaiden/{danMap[(uint)danId].DanId}", disabled: false));
        BreadcrumbsStateContainer.NotifyStateChanged();
    }

    private string GetDanClearStateString(DanClearState danClearState)
    {
        return danClearState switch
        {
            DanClearState.NotClear => Localizer["Not Passed"],
            DanClearState.RedNormalClear => Localizer["Red"],
            DanClearState.RedFullComboClear => Localizer["Red Full Combo"],
            DanClearState.RedPerfectClear => Localizer["Red Donderful Combo"],
            DanClearState.GoldNormalClear => Localizer["Gold"],
            DanClearState.GoldFullComboClear => Localizer["Gold Full Combo"],
            DanClearState.GoldPerfectClear => Localizer["Gold Donderful Combo"],
            _ => ""
        };
    }

    private string GetDanRequirementString(DanConditionType danConditionType)
    {
        return danConditionType switch
        {
            DanConditionType.TotalHitCount => Localizer["Total Hits"],
            DanConditionType.GoodCount => Localizer["Good"],
            DanConditionType.OkCount => Localizer["OK"],
            DanConditionType.BadCount => Localizer["Bad"],
            DanConditionType.SoulGauge => Localizer["Soul Gauge"],
            DanConditionType.DrumrollCount => Localizer["Drumroll"],
            DanConditionType.Score => Localizer["Score"],
            DanConditionType.ComboCount => Localizer["MAX Combo"],
            _ => ""
        };
    }

    private string GetDanRequirementTitle(DanData.OdaiBorder data)
    {
        var danConditionType = (DanConditionType)data.OdaiType;

        return GetDanRequirementString(danConditionType);
    }

    private static long GetAllBestFromData(DanConditionType type, DanBestData data)
    {
        return type switch
        {
            DanConditionType.SoulGauge => throw new ArgumentException("Soul gauge should not be here"),
            DanConditionType.GoodCount => data.DanBestStageDataList.Sum(stageData => stageData.GoodCount),
            DanConditionType.OkCount => data.DanBestStageDataList.Sum(stageData => stageData.OkCount),
            DanConditionType.BadCount => data.DanBestStageDataList.Sum(stageData => stageData.BadCount),
            DanConditionType.ComboCount => data.ComboCountTotal,
            DanConditionType.DrumrollCount => data.DanBestStageDataList.Sum(stageData => stageData.DrumrollCount),
            DanConditionType.Score => data.DanBestStageDataList.Sum(stageData => stageData.PlayScore),
            DanConditionType.TotalHitCount => data.DanBestStageDataList.Sum(stageData => stageData.TotalHitCount),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private static uint GetSongBestFromData(DanConditionType type, DanBestData data, int songNumber)
    {
        songNumber.Throw().IfOutOfRange(0, 2);

        return type switch
        {
            DanConditionType.SoulGauge => throw new ArgumentException("Soul gauge should not be here"),
            DanConditionType.GoodCount => data.DanBestStageDataList[songNumber].GoodCount,
            DanConditionType.OkCount => data.DanBestStageDataList[songNumber].OkCount,
            DanConditionType.BadCount => data.DanBestStageDataList[songNumber].BadCount,
            DanConditionType.ComboCount => data.DanBestStageDataList[songNumber].ComboCount,
            DanConditionType.DrumrollCount => data.DanBestStageDataList[songNumber].DrumrollCount,
            DanConditionType.Score => data.DanBestStageDataList[songNumber].PlayScore,
            DanConditionType.TotalHitCount => data.DanBestStageDataList[songNumber].TotalHitCount,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private static uint GetSongBorderCondition(DanData.OdaiBorder data, int songNumber, bool isGold)
    {
        if (!isGold)
        {
            return songNumber switch
            {
                0 => data.RedBorder1,
                1 => data.RedBorder2,
                2 => data.RedBorder3,
                _ => 0
            };
        }

        return songNumber switch
        {
            0 => data.GoldBorder1,
            1 => data.GoldBorder2,
            2 => data.GoldBorder3,
            _ => 0
        };
    }

    private string GetDanTitle(string gaidenTitle, string? language)
    {
        Dictionary<string, string> titleMap = new();
        foreach (var langTitle in gaidenTitle.Split(","))
        {
            if (langTitle.Contains("[JPN]=")) titleMap["default"] = titleMap["JPN"] = langTitle.Replace("[JPN]=", "").Trim();
            else if (langTitle.Contains("[ENG]=")) titleMap["default"] = titleMap["ENG"] = langTitle.Replace("[ENG]=", "").Trim();
            else if (langTitle.Contains("[CHN]=")) titleMap["default"] = titleMap["CHN"] = langTitle.Replace("[CHN]=", "").Trim();
            else if (langTitle.Contains("[KOR]=")) titleMap["default"] = titleMap["KOR"] = langTitle.Replace("[KOR]=", "").Trim();
            else if (langTitle.Contains("[CHS]=")) titleMap["default"] = titleMap["CHS"] = langTitle.Replace("[CHS]=", "").Trim();
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

    private static string GetDanResultIcon(uint danId)
    {
        string icon;
        const string notClearIcon = "<image href='/images/dani_NotClear.webp' width='24' height='24' style='filter: contrast(0.65)'/>";

        if (!_bestDataMap.TryGetValue(danId, out DanBestData? value))
        {
            return notClearIcon;
        }

        var state = value.ClearState;

        icon = state is DanClearState.NotClear ? notClearIcon : $"<image href='/images/dani_{state}.webp' width='24' height='24' />";

        return icon;
    }

    private static DanClearState GetDanResultState(uint danId)
    {
        return _bestDataMap.TryGetValue(danId, out DanBestData? value) ? value.ClearState : DanClearState.NotClear;
    }

    private static uint GetSoulGauge(DanData data, bool isGold)
    {
        var borders = data.OdaiBorderList;
        var soulBorder =
            borders.FirstOrDefault(border => (DanConditionType)border.BorderType == DanConditionType.SoulGauge,
                new DanData.OdaiBorder());

        return isGold ? soulBorder.GoldBorderTotal : soulBorder.RedBorderTotal;
    }

    private static string GetDanConditionOperator(DanConditionType type)
    {
        var conditionOperator = ">";

        if (type is DanConditionType.BadCount or DanConditionType.OkCount)
        {
            conditionOperator = "<";
        }

        return conditionOperator;
    }

    private static long GetTotalScore(uint danId)
    {
        return _bestDataMap.TryGetValue(danId, out DanBestData? value) ? value.DanBestStageDataList.Sum(stageData => stageData.HighScore) : 0;
    }

    private static long GetTotalGoodHits(uint danId)
    {
        return _bestDataMap.TryGetValue(danId, out DanBestData? value) ? value.DanBestStageDataList.Sum(stageData => stageData.GoodCount) : 0;
    }

    private static long GetTotalOkHits(uint danId)
    {
        return _bestDataMap.TryGetValue(danId, out DanBestData? value) ? value.DanBestStageDataList.Sum(stageData => stageData.OkCount) : 0;
    }

    private static long GetTotalBadHits(uint danId)
    {
        return _bestDataMap.TryGetValue(danId, out DanBestData? value) ? value.DanBestStageDataList.Sum(stageData => stageData.BadCount) : 0;
    }

    private static long GetTotalDrumrollHits(uint danId)
    {
        return _bestDataMap.TryGetValue(danId, out DanBestData? value) ? value.DanBestStageDataList.Sum(stageData => stageData.DrumrollCount) : 0;
    }

    private static long GetTotalMaxCombo(uint danId)
    {
        return _bestDataMap.TryGetValue(danId, out DanBestData? value) ? value.DanBestStageDataList.Sum(stageData => stageData.ComboCount) : 0;
    }

    private static long GetTotalHits(uint danId)
    {
        return _bestDataMap.TryGetValue(danId, out DanBestData? value) ? value.DanBestStageDataList.Sum(stageData => stageData.TotalHitCount) : 0;
    }
}