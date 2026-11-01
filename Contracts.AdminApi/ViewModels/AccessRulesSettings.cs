namespace TaikoLocalServer.Contracts.AdminApi.ViewModels;

/// <summary>Server-mode access rules, edited by admins in Admin › Access (stored in the database).</summary>
public class AccessRulesSettings
{
    public bool OnlyAdmin { get; set; }
    public bool RegisterWithLastPlayTime { get; set; }
    public bool AllowUserDelete { get; set; } = true;
    public bool AllowFreeProfileEditing { get; set; } = true;
    public int AccessCodeLimit { get; set; } = 3;
}
