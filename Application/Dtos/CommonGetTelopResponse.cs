namespace TaikoLocalServer.Application.Dtos;

public class CommonGetTelopResponse
{
    public uint Result { get; set; }

    public uint VerupNo { get; set; }

    public string StartDatetime { get; set; } = string.Empty;

    public string EndDatetime { get; set; } = string.Empty;

    public string Telop { get; set; } = string.Empty;
}
