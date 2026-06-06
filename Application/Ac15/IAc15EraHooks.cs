namespace TaikoLocalServer.Application.Ac15;

public enum Ac15SpecialModeAction
{
    ContinueNormal = 0,
    Handled = 1
}

public sealed record Ac15SpecialModeResult(Ac15SpecialModeAction Action, uint Result)
{
    public static Ac15SpecialModeResult ContinueNormal(uint result = 1)
        => new(Ac15SpecialModeAction.ContinueNormal, result);

    public static Ac15SpecialModeResult Handled(uint result = 1)
        => new(Ac15SpecialModeAction.Handled, result);
}

public sealed record Ac15SpecialModeContext(uint Baid, GameEra Era);

public sealed record Ac15StageSupportDecision(bool IsSupported, string? Reason)
{
    public static Ac15StageSupportDecision Supported { get; } = new(true, null);

    public static Ac15StageSupportDecision Unsupported(string reason) => new(false, reason);
}

public sealed record Ac15NormalSaveContext(uint Baid, GameEra Era, CommonPlayResultData PlayResultData);

public sealed record Ac15UserDataContext(uint Baid, GameEra Era);

public sealed record Ac15InitialDataContext(GameEra Era);

public interface IAc15EraHooks
{
    ValueTask<Ac15SpecialModeResult> TryHandleSpecialPlayModeAsync(
        CommonPlayResultData request,
        Ac15SpecialModeContext context,
        CancellationToken cancellationToken);

    Ac15StageSupportDecision IsSupportedStage(CommonPlayResultData.StageData stage);

    Ac15BestUpdatePolicy GetBestUpdatePolicy(CommonPlayResultData.StageData stage, CrownType crown);

    ValueTask BeforeNormalSaveAsync(Ac15NormalSaveContext context, CancellationToken cancellationToken);

    ValueTask AfterNormalSaveAsync(Ac15NormalSaveContext context, CancellationToken cancellationToken);

    void BuildUserDataExtras(CommonUserDataResponse response, Ac15UserDataContext context);

    void BuildInitialDataExtras(CommonInitialDataCheckResponse response, Ac15InitialDataContext context);
}
