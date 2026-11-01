namespace TaikoLocalServer.Contracts.AdminApi.ViewModels;

public class User
{
    public uint Baid { get; set; }
    
    public List<string> AccessCodes { get; set; } = new();
    
    public bool IsAdmin { get; set; }

    /// <summary>Has a Web UI password (Server mode accounts).</summary>
    public bool IsRegistered { get; set; }

    /// <summary>Has 2-step verification on.</summary>
    public bool TwoFactor { get; set; }

    public int Plays { get; set; }

    public UserSetting UserSetting { get; set; } = new();
}