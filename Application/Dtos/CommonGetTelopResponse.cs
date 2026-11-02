namespace TaikoLocalServer.Application.Dtos;

public sealed class CommonGetTelopResponse
{
    public uint Result { get; set; } = 1;

    public uint? VerupNo { get; set; }

    public string? StartDatetime { get; set; }

    public string? EndDatetime { get; set; }

    public string? Telop { get; set; }
}
