namespace TaikoLocalServer.Contracts.AdminApi.ViewModels;

/// <summary>A user's Web UI preferences (theme, language, 3D quality). On save, a null field is left as it was.</summary>
public class UiTheme
{
    public bool? Dark { get; set; }
    /// <summary>An accent id from the top bar's colour menu ("don", "katsu", …).</summary>
    public string? Accent { get; set; }
    /// <summary>A culture code from the WebUI's SupportedLanguages ("en-US", "ja", …).</summary>
    public string? Language { get; set; }
    /// <summary>How finely player cards draw the 3D don-chan (Settings › Display).</summary>
    public RenderQuality? Quality3D { get; set; }
}
