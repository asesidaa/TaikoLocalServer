using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Domain.Entities;

/// <summary>The banner on everyone's Web UI Dashboard (Server Management › Announcements). One row (Id 1).</summary>
public class DashboardNotice
{
    public int Id { get; set; } = 1;
    public bool Show { get; set; }
    public NoticeStyle Style { get; set; }
    public string Title { get; set; } = string.Empty;
    /// <summary>Plain text; **bold** and line breaks are rendered.</summary>
    public string Message { get; set; } = string.Empty;
    /// <summary>Stops showing after this (UTC); null: until switched off.</summary>
    public DateTime? Until { get; set; }
}
