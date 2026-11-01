namespace TaikoLocalServer.Contracts.AdminApi.Requests;

public class BanRequest
{
    public string AccessCode { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    /// <summary>Null: permanently.</summary>
    public int? Days { get; set; }
}
