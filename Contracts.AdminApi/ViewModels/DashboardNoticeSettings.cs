namespace TaikoLocalServer.Contracts.AdminApi.ViewModels;

/// <summary>The Dashboard notice as admins edit it; players get it only while it shows.</summary>
public class DashboardNoticeSettings
{
    public bool Show { get; set; }
    public NoticeStyle Style { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime? Until { get; set; }
}
