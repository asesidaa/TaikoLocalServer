namespace TaikoLocalServer.Contracts.AdminApi.ViewModels;

public class PlaySetting
{
    public uint Speed { get; set; }

    public bool IsVanishOn { get; set; }
    
    public bool IsInverseOn { get; set; }
    
    public RandomType RandomType { get; set; }
}