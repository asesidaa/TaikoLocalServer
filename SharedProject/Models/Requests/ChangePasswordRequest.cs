namespace SharedProject.Models.Requests;

public class ChangePasswordRequest
{
    public string AccessCode { get; set; } = string.Empty;
    
    public string OldPassword { get; set; } = string.Empty;
    
    public string NewPassword { get; set; } = string.Empty;
}