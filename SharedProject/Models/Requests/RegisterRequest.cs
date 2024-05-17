namespace SharedProject.Models.Requests;

public class RegisterRequest
{
    public string AccessCode { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RegisterWithLastPlayTime { get; set; }
    public DateTime LastPlayDateTime { get; set; }
    public string InviteCode { get; set; } = string.Empty;
}