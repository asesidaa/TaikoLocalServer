namespace TaikoWebUI.Pages;

public partial class DaniDojo
{
    private static Dictionary<uint, DanBestData> bestDataMap = new();

    private readonly List<BreadcrumbItem> breadcrumbs = new()
    {
        new BreadcrumbItem("Cards", "/Cards")
    };

    private DanBestDataResponse? response;

    [Parameter] public int Baid { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        response = await Client.GetFromJsonAsync<DanBestDataResponse>($"api/DanBestData/{Baid}");
        response.ThrowIfNull();
        response.DanBestDataList.ForEach(data => data.DanBestStageDataList
            .Sort((stageData, otherStageData) => stageData.SongNumber.CompareTo(otherStageData.SongNumber)));
        bestDataMap = response.DanBestDataList.ToDictionary(data => data.DanId);

        breadcrumbs.Add(new BreadcrumbItem($"Card: {Baid}", null, true));
        breadcrumbs.Add(new BreadcrumbItem("Dani Dojo", $"/Cards/{Baid}/DaniDojo"));
    }

    private static string GetDanClearStateString(DanClearState danClearState)
    {
        return danClearState switch
        {
            DanClearState.NotClear => "Not Cleared",
            DanClearState.RedNormalClear => "Red Clear",
            DanClearState.RedFullComboClear => "Red Full Combo",
            DanClearState.RedPerfectClear => "Red Donderful Combo",
            DanClearState.GoldNormalClear => "Gold Clear",
            DanClearState.GoldFullComboClear => "Gold Full Combo",
            DanClearState.GoldPerfectClear => "Gold Donderful Combo",
            _ => ""
        };
    }

    private static string GetDanRequirementString(DanConditionType danConditionType)
    {
        return danConditionType switch
        {
            DanConditionType.TotalHitCount => "Total Hits",
            DanConditionType.GoodCount => "Good Hits",
            DanConditionType.OkCount => "OK Hits",
            DanConditionType.BadCount => "Bad Hits",
            DanConditionType.SoulGauge => "Soul Gauge",
            DanConditionType.DrumrollCount => "Drumroll Hits",
            DanConditionType.Score => "Score",
            DanConditionType.ComboCount => "MAX Combo",
            _ => ""
        };
    }

    private static string GetDanRequirementTitle(DanData.OdaiBorder data)
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
            return songNumber switch
            {
                0 => data.RedBorder1,
                1 => data.RedBorder2,
                2 => data.RedBorder3,
                _ => 0
            };

        return songNumber switch
        {
            0 => data.GoldBorder1,
            1 => data.GoldBorder2,
            2 => data.GoldBorder3,
            _ => 0
        };
    }

    private static string GetDanTitle(string title)
    {
        return title switch
        {
            "5kyuu" => "Fifth Kyuu",
            "4kyuu" => "Fourth Kyuu",
            "3kyuu" => "Third Kyuu",
            "2kyuu" => "Second Kyuu",
            "1kyuu" => "First Kyuu",
            "1dan" => "First Dan",
            "2dan" => "Second Dan",
            "3dan" => "Third Dan",
            "4dan" => "Fourth Dan",
            "5dan" => "Fifth Dan",
            "6dan" => "Sixth Dan",
            "7dan" => "Seventh Dan",
            "8dan" => "Eighth Dan",
            "9dan" => "Ninth Dan",
            "10dan" => "Tenth Dan",
            "11dan" => "Kuroto",
            "12dan" => "Meijin",
            "13dan" => "Chojin",
            "14dan" => "Tatsujin",
            "15dan" => "Gaiden",
            _ => ""
        };
    }

    private string GetDanResultIcon(uint danId)
    {
        string icon;
        const string notClearIcon =
            "<image href='/images/dani_NotClear.png' width='24' height='24' style='filter: contrast(0.65)'/>";

        if (!bestDataMap.ContainsKey(danId)) return notClearIcon;

        var state = bestDataMap[danId].ClearState;

        icon = state is DanClearState.NotClear
            ? notClearIcon
            : $"<image href='/images/dani_{state}.png' width='24' height='24' />";

        return icon;
    }

    private DanClearState GetDanResultState(uint danId)
    {
        return bestDataMap.ContainsKey(danId) ? bestDataMap[danId].ClearState : DanClearState.NotClear;
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

        if (type is DanConditionType.BadCount or DanConditionType.OkCount) conditionOperator = "<";

        return conditionOperator;
    }

    private static long GetTotalScore(uint danId)
    {
        return bestDataMap.ContainsKey(danId)
            ? bestDataMap[danId].DanBestStageDataList.Sum(stageData => stageData.HighScore)
            : 0;
    }

    private static long GetTotalGoodHits(uint danId)
    {
        return bestDataMap.ContainsKey(danId)
            ? bestDataMap[danId].DanBestStageDataList.Sum(stageData => stageData.GoodCount)
            : 0;
    }

    private static long GetTotalOkHits(uint danId)
    {
        return bestDataMap.ContainsKey(danId)
            ? bestDataMap[danId].DanBestStageDataList.Sum(stageData => stageData.OkCount)
            : 0;
    }

    private static long GetTotalBadHits(uint danId)
    {
        return bestDataMap.ContainsKey(danId)
            ? bestDataMap[danId].DanBestStageDataList.Sum(stageData => stageData.BadCount)
            : 0;
    }

    private static long GetTotalDrumrollHits(uint danId)
    {
        return bestDataMap.ContainsKey(danId)
            ? bestDataMap[danId].DanBestStageDataList.Sum(stageData => stageData.DrumrollCount)
            : 0;
    }

    private static long GetTotalMaxCombo(uint danId)
    {
        return bestDataMap.ContainsKey(danId)
            ? bestDataMap[danId].DanBestStageDataList.Sum(stageData => stageData.ComboCount)
            : 0;
    }

    private static long GetTotalHits(uint danId)
    {
        return bestDataMap.ContainsKey(danId)
            ? bestDataMap[danId].DanBestStageDataList.Sum(stageData => stageData.TotalHitCount)
            : 0;
    }
}