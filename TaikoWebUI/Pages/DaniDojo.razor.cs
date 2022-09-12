namespace TaikoWebUI.Pages;

public partial class DaniDojo
{
    [Parameter]
    public int Baid { get; set; }

    private DanBestDataResponse? response;

    private Dictionary<uint, DanBestData> bestDataMap = new();

    private readonly List<BreadcrumbItem> breadcrumbs = new()
    {
        new BreadcrumbItem("Cards", href: "/Cards"),
    };

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        response = await Client.GetFromJsonAsync<DanBestDataResponse>($"api/DanBestData/{Baid}");
        response.ThrowIfNull();
        bestDataMap = response.DanBestDataList.ToDictionary(data => data.DanId);

        breadcrumbs.Add(new BreadcrumbItem($"Card: {Baid}", href: null, disabled: true));
        breadcrumbs.Add(new BreadcrumbItem("Dani Dojo", href: $"/Cards/{Baid}/DaniDojo", disabled: false));
    }

    private static string DanRequirementToString(DanData.OdaiBorder data)
    {
        var danConditionType = (DanConditionType)data.OdaiType;
        return (DanBorderType)data.BorderType switch
        {
            DanBorderType.All => $"{danConditionType}, Pass: {data.RedBorderTotal}, Gold: {data.GoldBorderTotal} ",
            DanBorderType.PerSong => $"{danConditionType}, " +
                                     $"Pass 1: {data.RedBorder1}, Pass 2: {data.RedBorder2}, Pass 3: {data.RedBorder3}" +
                                     $"Gold 1: {data.GoldBorder1}, Gold 2: {data.GoldBorder1}, Pass 3: {data.GoldBorder1}",
            _ => throw new ArgumentOutOfRangeException()
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
            "1dan"  => "First Dan",
            "2dan"  => "Second Dan",
            "3dan"  => "Third Dan",
            "4dan"  => "Fourth Dan",
            "5dan"  => "Fifth Dan",
            "6dan"  => "Sixth Dan",
            "7dan"  => "Seventh Dan",
            "8dan"  => "Eighth Dan",
            "9dan"  => "Ninth Dan",
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
        string icon = "";

        if (bestDataMap.ContainsKey(danId))
        {
            var state = bestDataMap[danId].ClearState;
            
            if (state is not DanClearState.NotClear)
            {
                icon = $"<image href='/images/dani_{state}.png' width='24' height='24'/>";
            }
        }

        return icon;
    }
}