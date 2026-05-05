namespace TaikoWebUI.Settings;

public class WebUiSettings
{
    public string Title { get; set; } = "TaikoWebUI";

    public bool DisplayUnplayedDans { get; set; }

    public MaxWidth MaxWidth { get; set; }

    public SongLeaderboardSettings SongLeaderboardSettings { get; set; } = new SongLeaderboardSettings();
    public Language[] SupportedLanguages { get; set; } = Array.Empty<Language>();
}

public class SongLeaderboardSettings
{
    public bool DisablePagination { get; set; } = false;
    public int PageSize { get; set; } = 10;
}
