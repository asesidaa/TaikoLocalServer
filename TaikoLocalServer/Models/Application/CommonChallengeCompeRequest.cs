namespace TaikoLocalServer.Models.Application;

public class CommonChallengeCompeRequest
{
    public uint Baid { get; set; }
    public string ChassisId { get; set; } = string.Empty;
    public string ShopId { get; set; } = string.Empty;
}
