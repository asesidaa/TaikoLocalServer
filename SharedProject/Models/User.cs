namespace SharedProject.Models;

public class User
{
    public string AccessCode { get; set; } = string.Empty;

    public uint Baid { get; set; }

    public string Password { get; set; } = string.Empty;
    
    public string Salt { get; set; } = string.Empty;
}