using TaikoWebUI.Utilities;
using Microsoft.Extensions.Options;
using System.Collections.Immutable;
using TaikoWebUI.Settings;

namespace TaikoWebUI.Pages;

public partial class DaniDojo
{
    [Inject]
    IOptions<WebUiSettings> UiSettings { get; set; } = default!;

    [Parameter]
    public int Baid { get; set; }

    [Parameter]
    public string? Era { get; set; }

    private string CurrentEra => WebUiEra.Normalize(Era);
    private bool IsGreen => string.Equals(CurrentEra, "Green", StringComparison.OrdinalIgnoreCase);
    private const int DanTabWindowSize = 10;

    private string? SongNameLanguage { get; set; }

    private DanBestDataResponse? response;
    private UserSetting? userSetting;

    private static Dictionary<uint, DanBestData> _bestDataMap = new();
    private Dictionary<uint, MusicDetail> musicDetailDictionary = new();
    private ImmutableDictionary<uint, DanData> danMap = ImmutableDictionary<uint, DanData>.Empty;
    private Dictionary<uint, DanData> danMapTemp = new();
    private List<uint> danIds = new();
    private uint? selectedDanId;
    private int danTabWindowStart;
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        danMap = GameDataService.GetDanMap(CurrentEra);

        response = await Client.GetFromJsonAsync<DanBestDataResponse>(WebUiEra.Api(CurrentEra, $"DanBestData/{Baid}"));
        response.ThrowIfNull();
        response.DanBestDataList.ForEach(data => data.DanBestStageDataList
            .Sort((stageData, otherStageData) => stageData.SongNumber.CompareTo(otherStageData.SongNumber)));
        
        _bestDataMap = response.DanBestDataList.ToDictionary(data => data.DanId);

        if (!UiSettings.Value.DisplayUnplayedDans)
        {
            foreach (var best in _bestDataMap)
            {
                var value = danMap.First(dan => dan.Key == best.Key);
                danMapTemp.Add(value.Key, value.Value);
            }
            danMap = danMapTemp.ToImmutableDictionary();
        }

        InitializeDanSelection();

        SongNameLanguage = await LocalStorage.GetItemAsync<string>("songNameLanguage");

        userSetting = await Client.GetFromJsonAsync<UserSetting>($"api/UserSettings/{Baid}");

        musicDetailDictionary = await GameDataService.GetMusicDetailDictionary(CurrentEra);

        // Breadcrumbs
        BreadcrumbsStateContainer.breadcrumbs.Clear();
        if (AuthService.IsLoggedIn && !AuthService.IsAdmin) BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem(Localizer["Dashboard"], href: "/"));
        else BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem(Localizer["Users"], href: "/Users"));
        BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem($"{userSetting?.MyDonName}", href: null, disabled: true));
        BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem(Localizer["Dani Dojo"], href: WebUiEra.UserRoute(Baid, CurrentEra, "DaniDojo"), disabled: false));
        BreadcrumbsStateContainer.NotifyStateChanged();
        isLoading = false;
    }

    private bool CanShowPreviousDanTabs => danTabWindowStart > 0;

    private bool CanShowNextDanTabs => danTabWindowStart + DanTabWindowSize < danIds.Count;

    private IEnumerable<uint> GetVisibleDanTabIds()
        => danIds.Skip(danTabWindowStart).Take(DanTabWindowSize);

    private void InitializeDanSelection()
    {
        danIds = danMap.Keys.ToList();
        if (danIds.Count == 0)
        {
            selectedDanId = null;
            danTabWindowStart = 0;
            return;
        }

        selectedDanId = danIds[0];
        danTabWindowStart = 0;
    }

    private bool TryGetSelectedDan(out uint danId, out DanData danData)
    {
        danId = selectedDanId ?? 0;
        if (selectedDanId is { } value && danMap.TryGetValue(value, out danData!))
        {
            return true;
        }

        danData = default!;
        return false;
    }

    private void SelectDan(uint danId)
    {
        if (!danMap.ContainsKey(danId))
        {
            return;
        }

        selectedDanId = danId;
        EnsureSelectedDanTabVisible();
    }

    private void ShowPreviousDanTabs()
    {
        if (!CanShowPreviousDanTabs)
        {
            return;
        }

        danTabWindowStart = Math.Max(0, danTabWindowStart - DanTabWindowSize);
        SelectDan(danIds[danTabWindowStart]);
    }

    private void ShowNextDanTabs()
    {
        if (!CanShowNextDanTabs)
        {
            return;
        }

        danTabWindowStart = Math.Min(danIds.Count - 1, danTabWindowStart + DanTabWindowSize);
        SelectDan(danIds[danTabWindowStart]);
    }

    private void EnsureSelectedDanTabVisible()
    {
        if (selectedDanId is not { } danId)
        {
            return;
        }

        var selectedIndex = danIds.IndexOf(danId);
        if (selectedIndex < 0)
        {
            return;
        }

        if (selectedIndex < danTabWindowStart)
        {
            danTabWindowStart = selectedIndex;
        }
        else if (selectedIndex >= danTabWindowStart + DanTabWindowSize)
        {
            danTabWindowStart = Math.Max(0, selectedIndex - DanTabWindowSize + 1);
        }
    }

    private string GetDanTabButtonClass(uint danId)
        => selectedDanId == danId
            ? "mud-tab mud-ripple mud-tab-active dani-tab-button"
            : "mud-tab mud-ripple dani-tab-button";

    private static string GetDanTabIcon(uint danId)
    {
        var state = GetDanResultState(danId);
        var filter = state is DanClearState.NotClear ? " style='filter: contrast(0.65)'" : "";

        return
            "<svg class='mud-icon-root mud-svg-icon mud-icon-size-medium mud-tab-icon-text' focusable='false' viewBox='0 0 24 24' aria-hidden='true'>" +
            $"<image href='/images/dani_{state}.webp' width='24' height='24'{filter}/>" +
            "</svg>";
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
            _ => title
        };
    }

    private static DanClearState GetDanResultState(uint danId)
    {
        return _bestDataMap.TryGetValue(danId, out DanBestData? value) ? value.ClearState : DanClearState.NotClear;
    }

    private static uint GetSoulGauge(DanData data, bool isGold)
    {
        var borders = data.OdaiBorderList;
        var soulBorder =
            borders.FirstOrDefault(border => (DanConditionType)border.OdaiType == DanConditionType.SoulGauge,
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
