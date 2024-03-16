namespace TaikoLocalServer.Models.Application;

public class CommonGetTokenCountResponse
{
    public uint Result { get; set; }

    public List<CommonTokenCountData> AryTokenCountDatas { get; set; } = [];
}

public class CommonTokenCountData
{
    public uint TokenId { get; set; }
    public int TokenCount { get; set; }
}