namespace TaikoLocalServer.Models.Application;

public class CommonAiDataResponse
{
    public uint Result { get; set; }
    public uint   TotalWinnings { get; set; }
    public string InputMedian   { get; set; } = "1";
    public string InputVariance { get; set; } = "0";
}