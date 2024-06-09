namespace TaikoWebUI.Settings;

public class WebUiSettings
{
    public string Title { get; set; } = "TaikoWebUI";
    public bool LoginRequired { get; set; }
    public bool OnlyAdmin { get; set; }
    public int BoundAccessCodeUpperLimit { get; set; }
    public bool RegisterWithLastPlayTime { get; set; }
    public bool AllowUserDelete { get; set; }
    public bool AllowFreeProfileEditing { get; set; }
    
    public SongLeaderboardSettings SongLeaderboardSettings { get; set; } = new SongLeaderboardSettings();
    public Language[] SupportedLanguages { get; set; } = Array.Empty<Language>();
}

public class SongLeaderboardSettings
{
    public bool DisablePagination { get; set; } = false;
    public int PageSize { get; set; } = 10;
}