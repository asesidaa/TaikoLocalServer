namespace SharedProject.Models.Requests;

public class SetPasswordRequest
{
    public uint Baid { get; set; }
    public string Password { get; set; } = default!;
    public string Salt { get; set; } = default!;
}