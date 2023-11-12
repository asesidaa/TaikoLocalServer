namespace SharedProject.Models.Requests;

public class BindAccessCodeRequest
{
    public string AccessCode { get; set; } = string.Empty;
    
    public uint Baid { get; set; }
}