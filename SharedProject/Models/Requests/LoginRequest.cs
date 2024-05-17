namespace SharedProject.Models.Requests;

public class LoginRequest
{
    public string AccessCode { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}