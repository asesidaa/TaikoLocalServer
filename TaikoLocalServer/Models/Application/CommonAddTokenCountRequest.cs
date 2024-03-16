namespace TaikoLocalServer.Models.Application;

public class CommonAddTokenCountRequest
{
    public uint       Baid                  { get; set; }
    public List<AddTokenCountData> AryAddTokenCountDatas { get; set; } = new();
}

public class AddTokenCountData
{
    public uint TokenId       { get; set; }
    public int  AddTokenCount { get; set; }
}