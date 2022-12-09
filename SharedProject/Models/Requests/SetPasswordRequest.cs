namespace SharedProject.Models.Requests;

public class SetPasswordRequest
{
    public string AccessCode { get; set; } = default!;
    public string Password { get; set; } = default!;
}