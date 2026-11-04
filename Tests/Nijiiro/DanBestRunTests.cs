using TaikoLocalServer.Tests.Green;

namespace TaikoLocalServer.Tests.Nijiiro;

// A Dani Dojo rank's page shows how often it was played and when its best run was. Every run counts; only a higher
// pass, or the same pass with a higher total score, moves the best run's date.
public sealed class DanBestRunTests
{
    private static UpdatePlayResultCommand Run(DanClearState result, uint score) => new(1, GameEra.Nijiiro, new CommonPlayResultData
    {
        Baid = 1, PlayMode = (uint)PlayMode.DanMode, DanId = 13, DanResult = (uint)result, SoulGaugeTotal = 100,
        AryStageInfoes = [new CommonPlayResultData.StageData { PlayScore = score, GoodCnt = 500, OptionFlg = [0] }],
    });

    [Fact]
    public async Task EveryRunCounts_AndOnlyABetterOneMovesTheBestRun()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        await fixture.Context.SaveChangesAsync();
        var handler = new UpdatePlayResultCommandHandler(fixture.Context, fixture.Catalog, NullLogger<UpdatePlayResultCommandHandler>.Instance);
        async Task<DanScoreDatumNijiiro> After(DanClearState result, uint score)
        {
            await handler.Handle(Run(result, score), CancellationToken.None);
            fixture.Context.ChangeTracker.Clear();
            return await fixture.Context.DanScoreDataNijiiro.Include(d => d.DanStageScoreData).SingleAsync();
        }

        var first = await After(DanClearState.RedNormalClear, 900_000);
        Assert.Equal(1u, first.PlayCount);
        Assert.NotNull(first.BestRunAt);

        var worse = await After(DanClearState.NotClear, 950_000);   // a higher score, but no pass
        Assert.Equal(2u, worse.PlayCount);
        Assert.Equal(first.BestRunAt, worse.BestRunAt);

        await Task.Delay(20);
        var better = await After(DanClearState.GoldNormalClear, 800_000);
        Assert.Equal(3u, better.PlayCount);
        Assert.True(better.BestRunAt > first.BestRunAt);
    }
}
