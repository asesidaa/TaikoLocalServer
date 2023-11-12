namespace TaikoWebUI.Settings;

public class WebUiSettings
{
    public bool LoginRequired { get; set; }
    public string AdminUsername { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
    public bool OnlyAdmin { get; set; }
}