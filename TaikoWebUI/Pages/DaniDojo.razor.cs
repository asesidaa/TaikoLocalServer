using System.Collections.Immutable;
using Microsoft.JSInterop;

namespace TaikoWebUI.Pages;

public partial class DaniDojo
{
    [Parameter]
    public int Baid { get; set; }

    private string? SongNameLanguage { get; set; }

    private DanBestDataResponse? response;
    private UserSetting? userSetting;

    private static Dictionary<uint, DanBestData> _bestDataMap = new();
    private Dictionary<uint, MusicDetail> musicDetailDictionary = new();
    private ImmutableDictionary<uint, DanData> danMap = ImmutableDictionary<uint, DanData>.Empty;

    private readonly List<BreadcrumbItem> breadcrumbs = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        
        if (AuthService.LoginRequired && !AuthService.IsLoggedIn)
        {
            await AuthService.LoginWithAuthToken();
        }

        response = await Client.GetFromJsonAsync<DanBestDataResponse>($"api/DanBestData/{Baid}");
        response.ThrowIfNull();
        response.DanBestDataList.ForEach(data => data.DanBestStageDataList
            .Sort((stageData, otherStageData) => stageData.SongNumber.CompareTo(otherStageData.SongNumber)));
        _bestDataMap = response.DanBestDataList.ToDictionary(data => data.DanId);

        danMap = GameDataService.GetDanMap();

        SongNameLanguage = await LocalStorage.GetItemAsync<string>("songNameLanguage");

        userSetting = await Client.GetFromJsonAsync<UserSetting>($"api/UserSettings/{Baid}");
        
        musicDetailDictionary = await GameDataService.GetMusicDetailDictionary();

        if (AuthService.IsLoggedIn && !AuthService.IsAdmin)
        {
            breadcrumbs.Add(new BreadcrumbItem(Localizer["Dashboard"], href: "/"));
        }
        else
        {
            breadcrumbs.Add(new BreadcrumbItem(Localizer["Users"], href: "/Users"));
        };
        breadcrumbs.Add(new BreadcrumbItem($"{userSetting?.MyDonName}", href: null, disabled: true));
        breadcrumbs.Add(new BreadcrumbItem(Localizer["Dani Dojo"], href: $"/Users/{Baid}/DaniDojo", disabled: false));
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

    private string GetDanTitle(string title)
    {
        return title switch
        {
            "5kyuu" => Localizer["Fifth Kyuu"],
            "4kyuu" => Localizer["Fourth Kyuu"],
            "3kyuu" => Localizer["Third Kyuu"],
            "2kyuu" => Localizer["Second Kyuu"],
            "1kyuu" => Localizer["First Kyuu"],
            "1dan" => Localizer["First Dan"],
            "2dan" => Localizer["Second Dan"],
            "3dan" => Localizer["Third Dan"],
            "4dan" => Localizer["Fourth Dan"],
            "5dan" => Localizer["Fifth Dan"],
            "6dan" => Localizer["Sixth Dan"],
            "7dan" => Localizer["Seventh Dan"],
            "8dan" => Localizer["Eighth Dan"],
            "9dan" => Localizer["Ninth Dan"],
            "10dan" => Localizer["Tenth Dan"],
            "11dan" => Localizer["Kuroto"],
            "12dan" => Localizer["Meijin"],
            "13dan" => Localizer["Chojin"],
            "14dan" => Localizer["Tatsujin"],
            "15dan" => Localizer["Gaiden"],
            _ => ""
        };
    }

    private string GetDanResultIcon(uint danId)
    {
        string icon;
        const string notClearIcon = "<image href='/images/dani_NotClear.png' width='24' height='24' style='filter: contrast(0.65)'/>";

        if (!_bestDataMap.ContainsKey(danId))
        {
            return notClearIcon;
        }

        var state = _bestDataMap[danId].ClearState;

        icon = state is DanClearState.NotClear ? notClearIcon : $"<image href='/images/dani_{state}.png' width='24' height='24' />";

        return icon;
    }

    private DanClearState GetDanResultState(uint danId)
    {
        return _bestDataMap.ContainsKey(danId) ? _bestDataMap[danId].ClearState : DanClearState.NotClear;
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
        return _bestDataMap.ContainsKey(danId) ? _bestDataMap[danId].DanBestStageDataList.Sum(stageData => stageData.HighScore) : 0;
    }

    private static long GetTotalGoodHits(uint danId)
    {
        return _bestDataMap.ContainsKey(danId) ? _bestDataMap[danId].DanBestStageDataList.Sum(stageData => stageData.GoodCount) : 0;
    }

    private static long GetTotalOkHits(uint danId)
    {
        return _bestDataMap.ContainsKey(danId) ? _bestDataMap[danId].DanBestStageDataList.Sum(stageData => stageData.OkCount) : 0;
    }

    private static long GetTotalBadHits(uint danId)
    {
        return _bestDataMap.ContainsKey(danId) ? _bestDataMap[danId].DanBestStageDataList.Sum(stageData => stageData.BadCount) : 0;
    }

    private static long GetTotalDrumrollHits(uint danId)
    {
        return _bestDataMap.ContainsKey(danId) ? _bestDataMap[danId].DanBestStageDataList.Sum(stageData => stageData.DrumrollCount) : 0;
    }

    private static long GetTotalMaxCombo(uint danId)
    {
        return _bestDataMap.ContainsKey(danId) ? _bestDataMap[danId].DanBestStageDataList.Sum(stageData => stageData.ComboCount) : 0;
    }

    private static long GetTotalHits(uint danId)
    {
        return _bestDataMap.ContainsKey(danId) ? _bestDataMap[danId].DanBestStageDataList.Sum(stageData => stageData.TotalHitCount) : 0;
    }
}