namespace TaikoLocalServer.Application.Handlers;

public partial class UpdatePlayResultCommandHandler
{
    private async ValueTask<uint> HandleBlueBattle(
        uint baid,
        CommonPlayResultData playResultData,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var playTime = ParseBluePlayDatetimeOrNow(playResultData.PlayDatetime);

        await context.AddBlueBattleStageResultsAsync(
            baid,
            playResultData,
            playTime,
            now,
            cancellationToken);
        await context.ApplyBlueBattleReleaseDataAsync(
            baid,
            playResultData.BattleReleaseData,
            now,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return 1;
    }
}
