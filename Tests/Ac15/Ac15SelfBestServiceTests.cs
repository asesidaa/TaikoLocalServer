using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15SelfBestServiceTests
{
    [Fact]
    public void BuildResponse_PreservesRequestedSongOrderAndAddsZeroRows()
    {
        var response = Ac15SelfBestService.BuildResponse(
            requestedDifficulty: 2,
            requestedSongs: [103, 102, 101],
            bestRows:
            [
                new Ac15BestRow(102, Difficulty.Normal, false, 222222, 88, CrownType.Gold)
            ]);

        Assert.Equal(1u, response.Result);
        Assert.Equal(2u, response.Level);
        Assert.Equal([103u, 102u, 101u], response.ArySelfbestScores.Select(row => row.SongNo));
        Assert.Equal(0u, response.ArySelfbestScores[0].SelfBestScore);
        Assert.Equal(222222u, response.ArySelfbestScores[1].SelfBestScore);
        Assert.Equal(0u, response.ArySelfbestScores[2].SelfBestScore);
    }

    [Fact]
    public void BuildResponse_SeparatesNormalAndShinRows()
    {
        var response = Ac15SelfBestService.BuildResponse(
            requestedDifficulty: 3,
            requestedSongs: [101],
            bestRows:
            [
                new Ac15BestRow(101, Difficulty.Hard, false, 111111, 77, CrownType.Clear),
                new Ac15BestRow(101, Difficulty.Hard, true, 333333, 99, CrownType.Dondaful)
            ]);

        Assert.Equal(111111u, Assert.Single(response.ArySelfbestScores).SelfBestScore);
        Assert.Equal(333333u, Assert.Single(response.AryShinSelfbestScores).SelfBestScore);
    }

    [Fact]
    public void BuildResponse_PopulatesUraBestForOniRows()
    {
        var response = Ac15SelfBestService.BuildResponse(
            requestedDifficulty: 4,
            requestedSongs: [101],
            bestRows:
            [
                new Ac15BestRow(101, Difficulty.Oni, false, 111111, 77, CrownType.Clear),
                new Ac15BestRow(101, Difficulty.UraOni, false, 222222, 88, CrownType.Gold),
                new Ac15BestRow(101, Difficulty.Oni, true, 333333, 91, CrownType.Clear),
                new Ac15BestRow(101, Difficulty.UraOni, true, 444444, 99, CrownType.Dondaful)
            ]);

        var normal = Assert.Single(response.ArySelfbestScores);
        Assert.Equal(111111u, normal.SelfBestScore);
        Assert.Equal(77u, normal.SelfBestScoreRate);
        Assert.Equal(222222u, normal.UraBestScore);
        Assert.Equal(88u, normal.UraBestScoreRate);

        var shin = Assert.Single(response.AryShinSelfbestScores);
        Assert.Equal(333333u, shin.SelfBestScore);
        Assert.Equal(91u, shin.SelfBestScoreRate);
        Assert.Equal(444444u, shin.UraBestScore);
        Assert.Equal(99u, shin.UraBestScoreRate);
    }
}
