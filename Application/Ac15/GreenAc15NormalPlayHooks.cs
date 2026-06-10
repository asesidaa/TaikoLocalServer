namespace TaikoLocalServer.Application.Ac15;

public sealed class GreenAc15NormalPlayHooks : IAc15EraHooks
{
    public ValueTask<Ac15SpecialModeResult> TryHandleSpecialPlayModeAsync(
        CommonPlayResultData request,
        Ac15SpecialModeContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(Ac15SpecialModeResult.ContinueNormal());
    }

    public Ac15StageSupportDecision IsSupportedStage(CommonPlayResultData.StageData stage)
        => stage.StageMode is 0 or 1 or 3 or 4
            ? Ac15StageSupportDecision.Supported
            : Ac15StageSupportDecision.Unsupported($"stage_mode {stage.StageMode} is not supported by Green");

    public Ac15BestUpdatePolicy GetBestUpdatePolicy(CommonPlayResultData.StageData stage, CrownType crown)
    {
        var isAiBattle = GreenStageModeInterpreter.IsAiBattle(stage.StageMode);
        var allowCrownUpdate = !isAiBattle || GreenAiBattleLevels.AllowsCrown(stage.Level, stage.SupportLevel);
        return new Ac15BestUpdatePolicy(AllowScoreUpdate: true, AllowCrownUpdate: allowCrownUpdate);
    }

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
