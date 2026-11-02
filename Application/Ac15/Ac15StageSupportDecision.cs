namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15StageSupportDecision(bool IsSupported, string? Reason)
{
    public static Ac15StageSupportDecision Supported { get; } = new(true, null);

    public static Ac15StageSupportDecision Unsupported(string reason) => new(false, reason);
}
