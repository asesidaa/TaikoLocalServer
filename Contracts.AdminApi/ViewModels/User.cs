namespace TaikoLocalServer.Contracts.AdminApi.ViewModels;

public class User
{
    public uint Baid { get; set; }
    
    public List<string> AccessCodes { get; set; } = new();
    
    public bool IsAdmin { get; set; }
    
    public UserSetting UserSetting { get; set; } = new();
}