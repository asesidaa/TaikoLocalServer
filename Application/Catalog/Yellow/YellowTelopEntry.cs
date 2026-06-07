namespace TaikoLocalServer.Application.Catalog.Yellow;

public sealed class YellowTelopEntry
{
    public uint TelopId { get; init; }

    public uint VerupNo { get; init; }

    public string StartDatetime { get; init; } = string.Empty;

    public string EndDatetime { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;
}
