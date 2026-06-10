using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Catalog.Yellow;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowDaniTests
{
    [Theory]
    [InlineData(1, true, false, 0)]
    [InlineData(25, true, false, 24)]
    [InlineData(101, false, true, 0)]
    [InlineData(128, false, true, 27)]
    public void YellowDanHelpers_RecognizesNormalAndExtraRanges(
        uint danId,
        bool isNormal,
        bool isExtra,
        uint packedIndex)
    {
        Assert.Equal(isNormal, YellowDanHelpers.IsNormalDanId(danId));
        Assert.Equal(isExtra, YellowDanHelpers.IsExtraDanId(danId));
        Assert.True(YellowDanHelpers.IsKnownYellowDanId(danId));
        Assert.Equal(packedIndex, YellowDanHelpers.GetPackedIndex(danId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(26)]
    [InlineData(100)]
    [InlineData(129)]
    public void YellowDanHelpers_RejectsInvalidRanges(uint danId)
    {
        Assert.False(YellowDanHelpers.IsKnownYellowDanId(danId));
        Assert.Throws<ArgumentOutOfRangeException>(() => YellowDanHelpers.GetPackedIndex(danId));
    }

    [Fact]
    public void YellowDanHelpers_WritesAndReadsTwoBitPackedGrades()
    {
        var flags = new byte[Ac15EraProfiles.Yellow.Limits.DanFlagBytes];

        flags = YellowDanHelpers.SetPackedGrade(flags, 0, Ac15DanClearGrade.NormalClear);
        flags = YellowDanHelpers.SetPackedGrade(flags, 1, Ac15DanClearGrade.GoldClear);
        flags = YellowDanHelpers.SetPackedGrade(flags, 2, YellowDanHelpers.ClampGrade(9));

        Assert.Equal(Ac15DanClearGrade.NormalClear, YellowDanHelpers.GetPackedGrade(flags, 0));
        Assert.Equal(Ac15DanClearGrade.GoldClear, YellowDanHelpers.GetPackedGrade(flags, 1));
        Assert.Equal(Ac15DanClearGrade.GoldClear, YellowDanHelpers.GetPackedGrade(flags, 2));
        Assert.Equal(Ac15DanClearGrade.NotClear, YellowDanHelpers.GetPackedGrade(flags, 3));
    }

    [Fact]
    public void YellowDanHelpers_DerivesMaxClearAndDisplayFallback()
    {
        var grades = new Dictionary<uint, Ac15DanClearGrade>
        {
            [1] = Ac15DanClearGrade.GoldClear,
            [2] = Ac15DanClearGrade.NotClear,
            [3] = Ac15DanClearGrade.NormalClear,
            [101] = Ac15DanClearGrade.GoldClear
        };

        Assert.Equal(3u, YellowDanHelpers.GetGotDanMax(grades));
        Assert.Equal(2u, YellowDanHelpers.GetNextUnclearedNormalDan(grades));
        Assert.Equal(2u, YellowDanHelpers.GetDisplayDanAfterNormalClear(1));
        Assert.Equal(2u, YellowDanHelpers.NormalizeDisplayDan(1, grades));
        Assert.Equal(2u, YellowDanHelpers.NormalizeDisplayDan(0, grades));
        Assert.Equal(2u, YellowDanHelpers.NormalizeDisplayDan(2, grades));
    }

    [Fact]
    public void YellowDanHelpers_DisplayDanCapsAtFinalNormalDan()
    {
        var allCleared = Enumerable.Range((int)YellowDanHelpers.MinNormalDanId, (int)YellowDanHelpers.MaxNormalDanId)
            .ToDictionary(dan => (uint)dan, _ => Ac15DanClearGrade.GoldClear);

        Assert.Equal(25u, YellowDanHelpers.GetDisplayDanAfterNormalClear(25));
        Assert.Equal(25u, YellowDanHelpers.GetNextUnclearedNormalDan(allCleared));
        Assert.Equal(25u, YellowDanHelpers.NormalizeDisplayDan(25, allCleared));
    }

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
