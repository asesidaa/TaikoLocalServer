namespace TaikoLocalServer.Application.Ac15;

public sealed class DefaultAc15EraHooks : IAc15EraHooks
{
    public static DefaultAc15EraHooks Instance { get; } = new();

    private DefaultAc15EraHooks()
    {
    }

    public ValueTask<Ac15SpecialModeResult> TryHandleSpecialPlayModeAsync(
        CommonPlayResultData request,
        Ac15SpecialModeContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(Ac15SpecialModeResult.ContinueNormal());
    }

    public Ac15StageSupportDecision IsSupportedStage(CommonPlayResultData.StageData stage)
        => stage.StageMode is 0 or 1
            ? Ac15StageSupportDecision.Supported
            : Ac15StageSupportDecision.Unsupported($"stage_mode {stage.StageMode} is not a normal AC15 stage");

    public Ac15BestUpdatePolicy GetBestUpdatePolicy(CommonPlayResultData.StageData stage, CrownType crown)
        => new(AllowScoreUpdate: true, AllowCrownUpdate: true);

    public ValueTask BeforeNormalSaveAsync(Ac15NormalSaveContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.CompletedTask;
    }

    public ValueTask AfterNormalSaveAsync(Ac15NormalSaveContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.CompletedTask;
    }

    public void BuildUserDataExtras(CommonUserDataResponse response, Ac15UserDataContext context)
    {
    }

    public void BuildInitialDataExtras(CommonInitialDataCheckResponse response, Ac15InitialDataContext context)
    {
    }
}
