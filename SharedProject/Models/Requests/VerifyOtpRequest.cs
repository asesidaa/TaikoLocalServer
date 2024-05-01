namespace SharedProject.Models.Requests;

public class VerifyOtpRequest
{
    public string Otp { get; set; } = "";
    
    public uint Baid { get; set; }
}