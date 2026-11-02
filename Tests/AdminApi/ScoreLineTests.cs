using TaikoLocalServer.Adapters.AdminApi.Controllers;
using TaikoLocalServer.Contracts.AdminApi.Responses;

namespace TaikoLocalServer.Tests.AdminApi;

// The Song page's score line (SongsController.ScoreLine): at most ScorePoint.Max points, keeping the best play.
public sealed class ScoreLineTests
{
    private static readonly DateTime Start = new(2026, 1, 1);

    private static List<(DateTime, uint)> Plays(int n, int bestAt) =>
        Enumerable.Range(0, n).Select(i => (Start.AddMinutes(i), i == bestAt ? 1_000_000u : (uint)(500_000 + i % 1000))).ToList();

    [Fact]
    public void UpToTheLimit_APointPerPlay()
    {
        var few = SongsController.ScoreLine(Plays(50, 20));
        Assert.Equal(50, few.Count);
        Assert.All(few, p => Assert.Equal(1, p.Plays));
        Assert.Equal(1_000_000u, few[20].Score);
    }

    [Fact]
    public void ThousandsOfPlays_FitKeepingTheBest()
    {
        var many = SongsController.ScoreLine(Plays(6_464, 4_321));
        Assert.InRange(many.Count, ScorePoint.Max / 2 + 1, ScorePoint.Max);
        Assert.Equal(6_464, many.Sum(p => p.Plays));
        Assert.Contains(many, p => p.Score == 1_000_000 && p.PlayTime == Start.AddMinutes(4_321));
        Assert.All(many.Zip(many.Skip(1)), x => Assert.True(x.First.PlayTime < x.Second.PlayTime));
    }

    [Fact]
    public void NoPlays_NoPoints() => Assert.Empty(SongsController.ScoreLine([]));
}
