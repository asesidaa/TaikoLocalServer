namespace TaikoLocalServer.Tests.Green;

public sealed class GreenAiBattlePlayResultTests
{
    [Fact]
    public async Task UpdatePlayResult_Green_AcceptsAiBattlePlay()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayDatetime = "20260518020644",
                PlayMode = 6,
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 2,
                        StageMode = 3,
                        PlayResult = 1,
                        PlayScore = 266580,
                        GoodCnt = 49, OkCnt = 13, NgCnt = 2, PoundCnt = 47, ComboCnt = 51, HitCnt = 109,
                        OptionFlg = [0], ToneFlg = new byte[16],
                        StarLevel = 1,
                        GhostStageData = new CommonPlayResultData.GhostStageData
                        {
                            IsWin = true,
                            SdCertifiedLevelId = 5,
                            ArySectionData =
                            [
                                new() { IsWin = true, GoodCnt = 13, OkCnt = 5, NgCnt = 2, PoundCnt = 0 },
                                new() { IsWin = true, GoodCnt = 19, OkCnt = 4, NgCnt = 0, PoundCnt = 0 },
                                new() { IsWin = false, GoodCnt = 17, OkCnt = 4, NgCnt = 0, PoundCnt = 47 }
                            ]
                        }
                    }
                ],
                GhostReleaseData = new CommonPlayResultData.UpdateGhostInfoData
                {
                    ReleaseInfoId = [1],
                    AryTokendata =
                    [
                        new() { TokenId = 9, TokenValue = 42 }
                    ]
                },
                GhostUpdatePerfData = new CommonPlayResultData.UpdateGhostPerfData
                {
                    InputMedian = -12,
                    InputVariance = 1668
                },
                GhostUpdateRankData = new CommonPlayResultData.UpdateGhostRankData
                {
                    RankId = 1,
                    WinPoint = 7,
                    CertifiedLevelId = 5,
                    AryWinningsData =
                    [
                        new() { LevelId = 5, Winnings = 3 }
                    ]
                }
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);

        var play = Assert.Single(await fixture.Context.SongPlayDataGreen.Where(row => row.Baid == 1).ToListAsync());
        Assert.Equal(3u, play.StageMode);
        Assert.False(play.IsShin);
        Assert.Equal(6u, play.PlayMode);

        var sections = await fixture.Context.GhostStageSectionDataGreen
            .Where(row => row.PlayId == play.Id)
            .OrderBy(row => row.SectionNo)
            .ToListAsync();
        Assert.Equal(3, sections.Count);
        Assert.Equal(0u, sections[0].SectionNo);
        Assert.True(sections[0].IsWin);
        Assert.Equal(47u, sections[2].PoundCount);

        var winnings = Assert.Single(await fixture.Context.GreenGhostWinnings.Where(row => row.Baid == 1).ToListAsync());
        Assert.Equal(5u, winnings.LevelId);
        Assert.Equal(3u, winnings.Winnings);

        var token = Assert.Single(await fixture.Context.GreenGhostTokens.Where(row => row.Baid == 1).ToListAsync());
        Assert.Equal(9u, token.TokenId);
        Assert.Equal(42u, token.TokenValue);

        var save = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
        Assert.NotNull(save);
        Assert.Equal(-12, save!.GhostInputMedian);
        Assert.Equal(1668u, save.GhostInputVariance);
        Assert.Equal(1u, save.GhostRankId);
        Assert.Equal(7u, save.GhostWinPoint);
        Assert.Equal(5u, save.GhostCertifiedLevelId);
        Assert.Equal(3u, save.GhostTotalWinnings);
        Assert.True(BitIsSet(save.GhostPlayedSongFlag, 101));
        Assert.True(BitIsSet(save.GhostReleaseInfoFlag, 1));
    }

    [Fact]
    public async Task UpdatePlayResult_Green_AiBattleShinRoutesToShinBest()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = 6,
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 2,
                        StageMode = 4,
                        PlayResult = 1,
                        PlayScore = 555555,
                        GoodCnt = 30, OkCnt = 5, NgCnt = 1, PoundCnt = 10, ComboCnt = 40, HitCnt = 46,
                        OptionFlg = [0], ToneFlg = new byte[16],
                        GhostStageData = new CommonPlayResultData.GhostStageData
                        {
                            IsWin = true,
                            SdCertifiedLevelId = 5,
                            ArySectionData = []
                        }
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);

        var shinBest = await fixture.Context.SongBestDataGreen.FindAsync(1u, 101u, Difficulty.Normal, true);
        var normalBest = await fixture.Context.SongBestDataGreen.FindAsync(1u, 101u, Difficulty.Normal, false);
        Assert.NotNull(shinBest);
        Assert.Null(normalBest);
        Assert.Equal(555555u, shinBest!.BestScore);

        var play = Assert.Single(await fixture.Context.SongPlayDataGreen.Where(row => row.Baid == 1).ToListAsync());
        Assert.Equal(4u, play.StageMode);
        Assert.True(play.IsShin);
    }

    [Fact]
    public async Task UpdatePlayResult_Green_AiSpecificLevelDoesNotUpdateCrownEvenWhenChartStarMatches()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CatalogWithSong101Stars(starNormal: 1));
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        fixture.Context.SongBestDataGreen.Add(new SongBestDatumGreen
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Normal,
            IsShin = false,
            BestScore = 100000,
            BestCrown = CrownType.None
        });
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = 6,
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 2,
                        StageMode = 3,
                        PlayResult = 2,
                        PlayScore = 200000,
                        StarLevel = 1,
                        SupportLevel = 1,
                        OptionFlg = [0], ToneFlg = new byte[16],
                        GhostStageData = new CommonPlayResultData.GhostStageData
                        {
                            IsWin = true,
                            SdCertifiedLevelId = 11,
                            ArySectionData = []
                        }
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var best = await fixture.Context.SongBestDataGreen.FindAsync(1u, 101u, Difficulty.Normal, false);
        Assert.NotNull(best);
        Assert.Equal(CrownType.None, best!.BestCrown);
        Assert.Equal(200000u, best.BestScore);
    }

    [Fact]
    public async Task UpdatePlayResult_Green_AiBattleUsualLevelUpdatesCrownWhenSupportLevelZero()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CatalogWithSong101Stars());
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        fixture.Context.SongBestDataGreen.Add(new SongBestDatumGreen
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Normal,
            IsShin = false,
            BestScore = 100000,
            BestCrown = CrownType.None
        });
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = 6,
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 2,
                        StageMode = 3,
                        PlayResult = 2,
                        PlayScore = 200000,
                        StarLevel = 1,
                        OptionFlg = [0], ToneFlg = new byte[16],
                        GhostStageData = new CommonPlayResultData.GhostStageData
                        {
                            IsWin = true,
                            SdCertifiedLevelId = 11,
                            ArySectionData = []
                        }
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var best = await fixture.Context.SongBestDataGreen.FindAsync(1u, 101u, Difficulty.Normal, false);
        Assert.NotNull(best);
        Assert.Equal(CrownType.Gold, best!.BestCrown);
        Assert.Equal(200000u, best.BestScore);
    }

    [Fact]
    public async Task UpdatePlayResult_Green_AiBattleUsualNormalLevelUpdatesCrown()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CatalogWithSong101Stars());
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        fixture.Context.SongBestDataGreen.Add(new SongBestDatumGreen
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Normal,
            IsShin = false,
            BestScore = 100000,
            BestCrown = CrownType.None
        });
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = 6,
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 2,
                        StageMode = 3,
                        PlayResult = 2,
                        PlayScore = 200000,
                        StarLevel = 2,
                        OptionFlg = [0], ToneFlg = new byte[16],
                        GhostStageData = new CommonPlayResultData.GhostStageData
                        {
                            IsWin = true,
                            SdCertifiedLevelId = 12,
                            ArySectionData = []
                        }
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var best = await fixture.Context.SongBestDataGreen.FindAsync(1u, 101u, Difficulty.Normal, false);
        Assert.NotNull(best);
        Assert.Equal(CrownType.Gold, best!.BestCrown);
        Assert.Equal(200000u, best.BestScore);
    }

    [Fact]
    public async Task UpdatePlayResult_Green_AiBattleUsualHardLevelUpdatesCrown()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CatalogWithSong101Stars());
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        fixture.Context.SongBestDataGreen.Add(new SongBestDatumGreen
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Hard,
            IsShin = false,
            BestScore = 100000,
            BestCrown = CrownType.None
        });
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = 6,
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 3,
                        StageMode = 3,
                        PlayResult = 2,
                        PlayScore = 200000,
                        StarLevel = 3,
                        OptionFlg = [0], ToneFlg = new byte[16],
                        GhostStageData = new CommonPlayResultData.GhostStageData
                        {
                            IsWin = true,
                            SdCertifiedLevelId = 23,
                            ArySectionData = []
                        }
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var best = await fixture.Context.SongBestDataGreen.FindAsync(1u, 101u, Difficulty.Hard, false);
        Assert.NotNull(best);
        Assert.Equal(CrownType.Gold, best!.BestCrown);
        Assert.Equal(200000u, best.BestScore);
    }

    [Fact]
    public async Task UpdatePlayResult_Green_AiSpecificLevelDoesNotTreatSdCertifiedLevelIdThirteenAsCrownEligible()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync(CatalogWithSong101Stars());
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        fixture.Context.SongBestDataGreen.Add(new SongBestDatumGreen
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Normal,
            IsShin = false,
            BestScore = 100000,
            BestCrown = CrownType.None
        });
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = 6,
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 2,
                        StageMode = 3,
                        PlayResult = 2,
                        PlayScore = 200000,
                        StarLevel = 3,
                        SupportLevel = 1,
                        OptionFlg = [0], ToneFlg = new byte[16],
                        GhostStageData = new CommonPlayResultData.GhostStageData
                        {
                            IsWin = true,
                            SdCertifiedLevelId = 13,
                            ArySectionData = []
                        }
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var best = await fixture.Context.SongBestDataGreen.FindAsync(1u, 101u, Difficulty.Normal, false);
        Assert.NotNull(best);
        Assert.Equal(CrownType.None, best!.BestCrown);
        Assert.Equal(200000u, best.BestScore);
    }

    [Fact]
    public async Task UpdatePlayResult_Green_AiBattleUraAlwaysUpdatesCrown()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = 6,
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 5,
                        StageMode = 3,
                        PlayResult = 1,
                        PlayScore = 300000,
                        SupportLevel = 1,
                        OptionFlg = [0], ToneFlg = new byte[16],
                        GhostStageData = new CommonPlayResultData.GhostStageData
                        {
                            IsWin = true,
                            SdCertifiedLevelId = 7,
                            ArySectionData = []
                        }
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        var best = await fixture.Context.SongBestDataGreen.FindAsync(1u, 101u, Difficulty.UraOni, false);
        Assert.NotNull(best);
        Assert.Equal(CrownType.Clear, best!.BestCrown);
    }

    [Theory]
    [InlineData(2u)]
    [InlineData(5u)]
    public async Task UpdatePlayResult_Green_SkipsAiBattleStageModeTwoAndFiveWithoutMutation(uint stageMode)
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = 6,
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 2,
                        StageMode = stageMode,
                        PlayResult = 1,
                        PlayScore = 100000,
                        OptionFlg = [0], ToneFlg = new byte[16]
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);
        Assert.Empty(await fixture.Context.SongPlayDataGreen.ToListAsync());
        Assert.Empty(await fixture.Context.SongBestDataGreen.ToListAsync());
    }

    [Fact]
    public async Task UpdatePlayResult_Green_NonAiBattlePlayDoesNotTouchGhostFields()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                PlayMode = 0,
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 1,
                        StageMode = 0,
                        PlayResult = 1,
                        PlayScore = 100000,
                        OptionFlg = [0], ToneFlg = new byte[16]
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(1u, result);

        var save = await fixture.Context.UserSaveDataGreen.FindAsync(1u);
        Assert.NotNull(save);
        Assert.All(save!.GhostPlayedSongFlag, b => Assert.Equal(0, b));
        Assert.All(save.GhostReleaseInfoFlag, b => Assert.Equal(0, b));
        Assert.Equal(0, save.GhostInputMedian);
        Assert.Equal(0u, save.GhostInputVariance);
        Assert.Equal(0u, save.GhostRankId);
        Assert.Equal(0u, save.GhostTotalWinnings);
        Assert.Empty(await fixture.Context.GreenGhostWinnings.ToListAsync());
        Assert.Empty(await fixture.Context.GreenGhostTokens.ToListAsync());
    }

    private static bool BitIsSet(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;

    private static GreenHandlerFixture.TestGreenCatalog CatalogWithSong101Stars(uint starNormal = 2)
        => new(musicInfoFileOrder:
        [
            new()
            {
                SongNo = 101,
                MusicId = "a",
                FileOrder = 0,
                StarEasy = 1,
                StarNormal = starNormal,
                StarHard = 3,
                StarOni = 4,
                StarUra = 5
            }
        ]);
}
