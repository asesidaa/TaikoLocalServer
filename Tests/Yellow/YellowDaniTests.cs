using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Catalog.Yellow;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowDaniTests
{
    [Fact]
    public async Task GetDanScore_Yellow_ReturnsSavedChallengeLevelRowsOnlyFromYellowState()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync(CreateDanCatalog(1));
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.DanScoreDataYellow.Add(new DanScoreDatumYellow
        {
            Baid = 1,
            DanId = 1,
            IsExtra = false,
            MedleyUniqueId = 20001,
            ClearGrade = Ac15DanClearGrade.GoldClear,
            ArrivalSongCount = 2,
            SoulGaugeTotal = 150,
            ComboCountTotal = 300,
            DanStageScoreData =
            [
                new() { Baid = 1, DanId = 1, IsExtra = false, StageIndex = 0, SongNumber = 101, PlayScore = 1000, HighScore = 1000, GoodCount = 10, OkCount = 2, BadCount = 1, DrumrollCount = 4, TotalHitCount = 13, ComboCount = 12 },
                new() { Baid = 1, DanId = 1, IsExtra = false, StageIndex = 1, SongNumber = 102, PlayScore = 2000, HighScore = 2000, GoodCount = 20, OkCount = 3, BadCount = 0, DrumrollCount = 5, TotalHitCount = 23, ComboCount = 22 }
            ]
        });
        fixture.Context.DanScoreDataBlue.Add(new DanScoreDatumBlue
        {
            Baid = 1,
            DanId = 1,
            IsExtra = false,
            MedleyUniqueId = 90001,
            ClearGrade = Ac15DanClearGrade.GoldClear,
            ArrivalSongCount = 1,
            SoulGaugeTotal = 1,
            ComboCountTotal = 1
        });
        fixture.Context.DanScoreDataGreen.Add(new DanScoreDatumGreen
        {
            Baid = 1,
            DanId = 1,
            IsExtra = false,
            MedleyUniqueId = 90002,
            ClearGrade = Ac15DanClearGrade.GoldClear,
            ArrivalSongCount = 1,
            SoulGaugeTotal = 2,
            ComboCountTotal = 2
        });
        await fixture.Context.SaveChangesAsync();
        var handler = new GetDanScoreQueryHandler(
            NullLogger<GetDanScoreQueryHandler>.Instance,
            fixture.Context,
            fixture.Catalog);

        var response = await handler.Handle(new GetDanScoreQuery(1, GameEra.Yellow, 0, [1, 999]), CancellationToken.None);

        var dan = Assert.Single(response.AryDanScoreDatas);
        Assert.Equal(1u, dan.DanId);
        Assert.Equal(2u, dan.ArrivalSongCnt);
        Assert.Equal(150u, dan.SoulGaugeTotal);
        Assert.Equal(300u, dan.ComboCntTotal);
        Assert.Equal(2, dan.AryDanScoreDataStages.Count);
        Assert.Equal(1000u, dan.AryDanScoreDataStages[0].HighScore);
        Assert.Equal(2000u, dan.AryDanScoreDataStages[1].HighScore);
    }

    [Fact]
    public async Task GetDanScore_Yellow_IgnoresUnknownRequestedIds()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync(CreateDanCatalog(1));
        var handler = new GetDanScoreQueryHandler(
            NullLogger<GetDanScoreQueryHandler>.Instance,
            fixture.Context,
            fixture.Catalog);

        var response = await handler.Handle(new GetDanScoreQuery(1, GameEra.Yellow, 0, [20001, 999]), CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Empty(response.AryDanScoreDatas);
    }

    private static YellowHandlerFixture.TestYellowCatalog CreateDanCatalog(params uint[] challengeLevels)
        => new(taikojukuFileOrder: challengeLevels
            .Select((dan, index) => new YellowTaikojukuEntry
            {
                UniqueId = 20001u + (uint)index,
                ChallengeLevel = dan,
                DanLevel = dan,
                Name = $"Dan {dan}",
                Songs =
                [
                    new YellowTaikojukuSong { SongNo = 101, Level = 1 },
                    new YellowTaikojukuSong { SongNo = 102, Level = 1 }
                ]
            })
            .ToArray());
}
