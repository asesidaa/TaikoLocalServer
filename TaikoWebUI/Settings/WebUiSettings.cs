namespace TaikoWebUI.Settings;

public class WebUiSettings
{
    public bool LoginRequired { get; set; }
    public bool OnlyAdmin { get; set; }
    public int BoundAccessCodeUpperLimit { get; set; }
    public bool RegisterWithLastPlayTime { get; set; }
    public bool AllowUserDelete { get; set; }
    public bool AllowFreeProfileEditing { get; set; }
    public Language[] SupportedLanguages { get; set; } = Array.Empty<Language>();
}