namespace SharedProject.Models;

public class UserCredential
{
    public uint Baid { get; set; }
    
    public string Password { get; set; } = string.Empty;
    
    public string Salt { get; set; } = string.Empty;
}