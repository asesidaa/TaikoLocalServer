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
}