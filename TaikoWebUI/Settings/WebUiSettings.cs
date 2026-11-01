namespace TaikoWebUI.Settings;

public class WebUiSettings
{
    public string Title { get; set; } = "TaikoWebUI";

    public bool DisplayUnplayedDans { get; set; }

    /// <summary>URL of the display-only 3D don-chan embed: by default the bundled don-chan-previewer embed build
    /// (wwwroot/don-embed, rebuilt with its <c>npm run build:embed</c>). Empty = no 3D preview.</summary>
    public string DonEmbedUrl { get; set; } = string.Empty;

    public SongLeaderboardSettings SongLeaderboardSettings { get; set; } = new SongLeaderboardSettings();
    public Language[] SupportedLanguages { get; set; } = Array.Empty<Language>();
}

public class SongLeaderboardSettings
{
    public bool DisablePagination { get; set; } = false;
    public int PageSize { get; set; } = 10;
}
