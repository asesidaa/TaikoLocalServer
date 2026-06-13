using TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;
using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Catalog.Yellow;
using TaikoLocalServer.Tests.Ac15;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowUserDataProtocolTests
{
    [Fact]
    public async Task UserData_Yellow_ComposesSaveCatalogFavoritesRecentAndSupportedFlags()
    {
        var catalog = new YellowHandlerFixture.TestYellowCatalog(
            musicInfoFileOrder:
            [
                new YellowMusicInfoEntry { SongNo = 101, MusicId = "a", FileOrder = 0 },
                new YellowMusicInfoEntry { SongNo = 102, MusicId = "b", FileOrder = 1 },
                new YellowMusicInfoEntry { SongNo = 103, MusicId = "c", FileOrder = 2 }
            ],
            itemShopCatalog: new YellowItemShopCatalog
            {
                IsEnabled = true,
                ActiveSeasonId = 1,
                Seasons = new Dictionary<uint, YellowItemShopSeason>
                {
                    [1] = new()
                    {
                        SeasonId = 1,
                        Items =
                        [
                            new YellowItemShopEntry
                            {
                                ItemNo = 1,
                                ItemType = Ac15ShopItemType.Song,
                                ItemId = 103,
                                Price = 10
                            },
                            new YellowItemShopEntry
                            {
                                ItemNo = 2,
                                ItemType = Ac15ShopItemType.Tone,
                                ItemId = 4,
                                Price = 20
                            }
                        ]
                    }
                }
            })
        {
            Recommend = new YellowRecommendEntry
            {
                RecommendSong = 102,
                RecommendBestSongs = [101, 102]
            }
        };
        await using var fixture = await YellowHandlerFixture.CreateAsync(catalog);
        fixture.Context.UserData.Add(new UserDatum { Baid = 5, MyDonName = "DON" });
        var save = UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(5);
        save.ReleaseSongFlg = Ac15ProtocolBytes.CreateFixedBitset([104], Ac15EraProfiles.Yellow.Limits.SongFlagBytes);
        save.ToneFlg = Ac15ProtocolBytes.CreateFixedBitset([0, 4], Ac15EraProfiles.Yellow.Limits.ToneFlagBytes);
        save.TitleFlg = Ac15ProtocolBytes.CreateFixedBitset([10], Ac15EraProfiles.Yellow.Limits.TitleFlagBytes);
        save.OptionFlg = [1, 2, 3];
        save.DefaultOptionSetting = [4, 5];
        save.CategJpopCnt = 2;
        save.SongFavoriteCnt = 3;
        save.SongRecentCnt = 4;
        save.TotalCreditCnt = 5;
        save.PrevAreaCode = 6;
        save.ConsecAreaCnt = 7;
        save.DefaultShinSetting = true;
        save.DispLevelTotal = 8;
        save.DispLevelChassis = 9;
        save.DispLevelSelf = 10;
        save.DispTaikojukuDan = 0;
        save.GotDanMax = 3;
        var limits = Ac15EraProfiles.Yellow.Limits;
        save.GotDanFlg = Ac15DanHelpers.SetPackedGrade(
            save.GotDanFlg,
            Ac15DanHelpers.GetPackedIndex(3, limits),
            Ac15DanClearGrade.GoldClear,
            limits.DanFlagBytes);
        save.GotDanExtraFlg = Ac15DanHelpers.SetPackedGrade(
            save.GotDanExtraFlg,
            Ac15DanHelpers.GetPackedIndex(101, limits),
            Ac15DanClearGrade.NormalClear,
            limits.DanExtraFlagBytes);
        save.DifficultyPlayedCourse = 11;
        save.DifficultyPlayedStar = 12;
        save.IsChallengeCompe = true;
        save.IsTojiru = false;
        save.IsDevil = true;
        save.IsExplain = true;
        save.TokkunTutorialFlg = 77;
        fixture.Context.UserSaveDataYellow.Add(save);
        fixture.Context.YellowFavoriteSongs.AddRange(
            new YellowFavoriteSongs { Baid = 5, SongNo = 102 },
            new YellowFavoriteSongs { Baid = 5, SongNo = 101 });
        fixture.Context.YellowRecentSongs.AddRange(
            new YellowRecentSongs { Baid = 5, SongNo = 101, LastPlayed = new DateTime(2026, 6, 8, 12, 0, 0) },
            new YellowRecentSongs { Baid = 5, SongNo = 102, LastPlayed = new DateTime(2026, 6, 8, 13, 0, 0) });
        await fixture.Context.SaveChangesAsync();
        var handler = CreateUserDataHandler(fixture);

        var response = await handler.Handle(new Ac15UserDataQuery(5, GameEra.Yellow), CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Equal(789u, response.SongFlags.SongHashVer);
        Assert.Equal(Ac15EraProfiles.Yellow.Limits.SongFlagBytes, response.SongFlags.ReleaseSongFlg.Length);
        Assert.True(BitIsSet(response.SongFlags.ReleaseSongFlg, 101));
        Assert.True(BitIsSet(response.SongFlags.ReleaseSongFlg, 102));
        Assert.False(BitIsSet(response.SongFlags.ReleaseSongFlg, 103));
        Assert.True(BitIsSet(response.SongFlags.ReleaseSongFlg, 104));
        Assert.True(BitIsSet(response.SongFlags.ToneFlg, 0));
        Assert.False(BitIsSet(response.SongFlags.ToneFlg, 4));
        Assert.True(BitIsSet(response.SongFlags.TitleFlg, 10));
        Assert.Equal([101u, 102u], response.SongLists.AryFavoriteSongNoes.Order().ToArray());
        Assert.Equal([102u, 101u], response.SongLists.AryRecentSongNoes);
        Assert.Equal(102u, response.Recommendations.RecommendSong);
        Assert.Equal(new List<uint> { 101, 102 }, response.Recommendations.RecommendBestSong);
        Assert.Equal(2u, response.Counters.CategJpopCnt);
        Assert.Equal(3u, response.Counters.SongFavoriteCnt);
        Assert.Equal(4u, response.Counters.SongRecentCnt);
        Assert.Equal(5u, response.Counters.TotalCreditCnt);
        Assert.Equal(6u, response.Counters.PrevAreaCode);
        Assert.Equal(7u, response.Counters.ConsecAreaCnt);
        Assert.True(response.Display.DefaultShinSetting);
        Assert.Equal(8u, response.Display.DispLevelTotal);
        Assert.Equal(9u, response.Display.DispLevelChassis);
        Assert.Equal(10u, response.Display.DispLevelSelf);
        Assert.Equal(1u, response.Display.DispTaikojukuDan);
        Assert.Equal(11u, response.Display.DifficultyPlayedCourse);
        Assert.Equal(12u, response.Display.DifficultyPlayedStar);
        Assert.True(response.Display.IsChallengeCompe);
        Assert.False(response.Display.IsTojiru);
        Assert.True(response.ModeFlags!.IsDevil);
        Assert.True(response.ModeFlags.IsExplain);
        Assert.Equal(77u, response.Tutorial!.TokkunTutorialFlg);

        var wire = UserDataMappers.Map(response);
        Assert.True(wire.ShouldSerializeDispTaikojukuDan());
        Assert.Equal(1u, wire.DispTaikojukuDan);
        Assert.True(wire.ShouldSerializeTokkunTutorialFlg());
        Assert.Equal(77u, wire.TokkunTutorialFlg);
    }

    [Fact]
    public void UserDataMapper_Yellow_MapsSupportedFieldsAndOmitsAbsentTokkunTutorial()
    {
        var common = new Ac15UserDataResponse
        {
            Result = 1,
            SongLists = new Ac15UserDataSongLists
            {
                AryFavoriteSongNoes = [101, 102],
                AryRecentSongNoes = [103]
            },
            SongFlags = new Ac15UserDataSongFlags
            {
                SongHashVer = 789,
                ReleaseSongFlg = new byte[Ac15EraProfiles.Yellow.Limits.SongFlagBytes],
                OptionFlg = [1],
                ToneFlg = new byte[Ac15EraProfiles.Yellow.Limits.ToneFlagBytes],
                TitleFlg = new byte[Ac15EraProfiles.Yellow.Limits.TitleFlagBytes]
            },
            Counters = new Ac15UserDataProfileCounters
            {
                CategJpopCnt = 2,
                SongFavoriteCnt = 3,
                PrevAreaCode = 4,
                TotalCreditCnt = 8,
                SongRecentCnt = 9
            },
            Recommendations = new Ac15UserDataRecommendations
            {
                RecommendSong = 5,
                RecommendBestSong = [6, 7]
            },
            Display = new Ac15UserDataDisplaySettings
            {
                DefaultOptionSetting = [10, 11],
                DefaultShinSetting = true,
                DispLevelTotal = 12,
                DispLevelChassis = 13,
                DispLevelSelf = 14,
                DispTaikojukuDan = 0,
                DifficultyPlayedCourse = 15,
                DifficultyPlayedStar = 16,
                IsChallengeCompe = true,
                IsTojiru = true
            },
            ModeFlags = new Ac15UserDataModeFlags(true, true)
        };

        var response = UserDataMappers.Map(common);

        Assert.Equal(1u, response.Result);
        Assert.Equal([101u, 102u], response.AryFavoriteSongNoes);
        Assert.Equal([103u], response.AryRecentSongNoes);
        Assert.Equal(789u, response.SongHashVer);
        Assert.Same(common.SongFlags.ReleaseSongFlg, response.HashReleaseSongFlg);
        Assert.Same(common.SongFlags.ToneFlg, response.ToneFlg);
        Assert.Same(common.SongFlags.TitleFlg, response.TitleFlg);
        Assert.Equal(2u, response.CategJpopCnt);
        Assert.Equal(3u, response.SongFavoriteCnt);
        Assert.Equal(4u, response.PrevAreaCode);
        Assert.Equal(5u, response.RecommendSong);
        Assert.Equal([6u, 7u], response.RecommendBestSongs);
        Assert.Equal(8u, response.TotalCreditCnt);
        Assert.Equal(9u, response.SongRecentCnt);
        Assert.Equal([10, 11], response.DefaultOptionSetting);
        Assert.True(response.DefaultShinSetting);
        Assert.Equal(12u, response.DispLevelTotal);
        Assert.Equal(13u, response.DispLevelChassis);
        Assert.Equal(14u, response.DispLevelSelf);
        Assert.Equal(1u, response.DispTaikojukuDan);
        Assert.Equal(15u, response.DifficultyPlayedCourse);
        Assert.Equal(16u, response.DifficultyPlayedStar);
        Assert.True(response.IsChallengecompe);
        Assert.True(response.IsTojiru);
        Assert.True(response.IsDevil);
        Assert.True(response.IsExplain);
        Assert.False(response.ShouldSerializeTokkunTutorialFlg());
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(1u)]
    [InlineData(7u)]
    public void UserDataMapper_Yellow_MapsRawTokkunTutorialFlagWhenPresent(uint tokkunTutorialFlg)
    {
        var response = UserDataMappers.Map(new Ac15UserDataResponse
        {
            Result = 1,
            SongFlags = new Ac15UserDataSongFlags
            {
                ReleaseSongFlg = new byte[Ac15EraProfiles.Yellow.Limits.SongFlagBytes],
                ToneFlg = new byte[Ac15EraProfiles.Yellow.Limits.ToneFlagBytes],
                TitleFlg = new byte[Ac15EraProfiles.Yellow.Limits.TitleFlagBytes]
            },
            Display = new Ac15UserDataDisplaySettings
            {
                DefaultOptionSetting = new byte[2],
                DispTaikojukuDan = 1
            },
            Tutorial = new Ac15UserDataTutorial(tokkunTutorialFlg, DifficultyTutorialFlg: null)
        });

        Assert.True(response.ShouldSerializeTokkunTutorialFlg());
        Assert.Equal(tokkunTutorialFlg, response.TokkunTutorialFlg);
    }

    [Fact]
    public async Task UserData_Yellow_OmitsAbsentTokkunTutorialFlag()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 5, MyDonName = "DON" });
        fixture.Context.UserSaveDataYellow.Add(UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(5));
        await fixture.Context.SaveChangesAsync();
        var handler = CreateUserDataHandler(fixture);

        var response = await handler.Handle(new Ac15UserDataQuery(5, GameEra.Yellow), CancellationToken.None);
        var wire = UserDataMappers.Map(response);

        Assert.Null(response.Tutorial!.TokkunTutorialFlg);
        Assert.False(wire.ShouldSerializeTokkunTutorialFlg());
    }

    [Fact]
    public async Task UserData_Yellow_ReturnsPersistedRawTokkunTutorialFlag()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 5, MyDonName = "DON" });
        var save = UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(5);
        save.TokkunTutorialFlg = 7;
        fixture.Context.UserSaveDataYellow.Add(save);
        await fixture.Context.SaveChangesAsync();
        var handler = CreateUserDataHandler(fixture);

        var response = await handler.Handle(new Ac15UserDataQuery(5, GameEra.Yellow), CancellationToken.None);
        var wire = UserDataMappers.Map(response);

        Assert.Equal(7u, response.Tutorial!.TokkunTutorialFlg);
        Assert.True(wire.ShouldSerializeTokkunTutorialFlg());
        Assert.Equal(7u, wire.TokkunTutorialFlg);
        AssertNoTokkunHistorySurface(response);
        AssertNoTokkunHistorySurface(wire);
    }

    [Fact]
    public async Task UserData_Yellow_ReadsBackTokkunTutorialFlagPersistedByPlayResultOnly()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 5, MyDonName = "DON" });
        fixture.Context.UserSaveDataYellow.Add(UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(5));
        await fixture.Context.SaveChangesAsync();
        var playResultHandler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);
        var userDataHandler = CreateUserDataHandler(fixture);

        var playResult = await playResultHandler.Handle(
            CreateTokkunPlayResult(5),
            CancellationToken.None);
        var response = await userDataHandler.Handle(new Ac15UserDataQuery(5, GameEra.Yellow), CancellationToken.None);
        var wire = UserDataMappers.Map(response);

        Assert.Equal(1u, playResult);
        Assert.Equal(7u, response.Tutorial!.TokkunTutorialFlg);
        Assert.True(wire.ShouldSerializeTokkunTutorialFlg());
        Assert.Equal(7u, wire.TokkunTutorialFlg);
        AssertNoTokkunHistorySurface(response);
        AssertNoTokkunHistorySurface(wire);
    }

    [Fact]
    public async Task UserData_Yellow_NormalizesDisplayDanFromYellowDanRows()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 5, MyDonName = "DON" });
        var save = UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(5);
        save.DispTaikojukuDan = 1;
        fixture.Context.UserSaveDataYellow.Add(save);
        fixture.Context.DanScoreDataYellow.Add(new DanScoreDatumYellow
        {
            Baid = 5,
            DanId = 1,
            IsExtra = false,
            MedleyUniqueId = 20001,
            ClearGrade = Ac15DanClearGrade.GoldClear
        });
        fixture.Context.DanScoreDataBlue.Add(new DanScoreDatumBlue
        {
            Baid = 5,
            DanId = 2,
            IsExtra = false,
            MedleyUniqueId = 90001,
            ClearGrade = Ac15DanClearGrade.GoldClear
        });
        fixture.Context.DanScoreDataGreen.Add(new DanScoreDatumGreen
        {
            Baid = 5,
            DanId = 3,
            IsExtra = false,
            MedleyUniqueId = 90002,
            ClearGrade = Ac15DanClearGrade.GoldClear
        });
        await fixture.Context.SaveChangesAsync();
        var handler = CreateUserDataHandler(fixture);

        var response = await handler.Handle(new Ac15UserDataQuery(5, GameEra.Yellow), CancellationToken.None);
        var wire = UserDataMappers.Map(response);

        Assert.Equal(2u, response.Display.DispTaikojukuDan);
        Assert.True(wire.ShouldSerializeDispTaikojukuDan());
        Assert.Equal(2u, wire.DispTaikojukuDan);
    }

    [Fact]
    public async Task UserData_Yellow_UsesOnlyYellowPurchasedShopRowsForSongAndToneLocks()
    {
        var catalog = new YellowHandlerFixture.TestYellowCatalog(
            musicInfoFileOrder:
            [
                new YellowMusicInfoEntry { SongNo = 101, MusicId = "a", FileOrder = 0 },
                new YellowMusicInfoEntry { SongNo = 103, MusicId = "c", FileOrder = 1 }
            ],
            itemShopCatalog: new YellowItemShopCatalog
            {
                IsEnabled = true,
                ActiveSeasonId = 2,
                Seasons = new Dictionary<uint, YellowItemShopSeason>
                {
                    [2] = new()
                    {
                        SeasonId = 2,
                        Items =
                        [
                            new YellowItemShopEntry { ItemNo = 1, ItemType = Ac15ShopItemType.Song, ItemId = 103, Price = 10 },
                            new YellowItemShopEntry { ItemNo = 2, ItemType = Ac15ShopItemType.Tone, ItemId = 4, Price = 20 }
                        ]
                    }
                }
            });
        await using var fixture = await YellowHandlerFixture.CreateAsync(catalog);
        fixture.Context.UserData.Add(new UserDatum { Baid = 5, MyDonName = "DON" });
        var save = UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(5);
        save.ReleaseSongFlg = Ac15ProtocolBytes.CreateFixedBitset([103], Ac15EraProfiles.Yellow.Limits.SongFlagBytes);
        save.ToneFlg = Ac15ProtocolBytes.CreateFixedBitset([0, 4], Ac15EraProfiles.Yellow.Limits.ToneFlagBytes);
        fixture.Context.UserSaveDataYellow.Add(save);
        fixture.Context.BlueShopItemStates.Add(new BlueShopItemState
        {
            Baid = 5,
            SeasonId = 2,
            ItemType = Ac15ShopItemType.Song.ToProtocolValue(),
            ItemId = 103,
            ItemNo = 1,
            ItemPrice = 10,
            Status = Ac15ShopItemStatus.Unlocked,
            PurchasedAt = DateTime.UtcNow,
            UnlockedAt = DateTime.UtcNow
        });
        fixture.Context.GreenShopItemStates.Add(new GreenShopItemState
        {
            Baid = 5,
            SeasonId = 2,
            ItemType = Ac15ShopItemType.Tone.ToProtocolValue(),
            ItemId = 4,
            ItemNo = 2,
            ItemPrice = 20,
            Status = Ac15ShopItemStatus.Unlocked,
            PurchasedAt = DateTime.UtcNow,
            UnlockedAt = DateTime.UtcNow
        });
        await fixture.Context.SaveChangesAsync();
        var handler = CreateUserDataHandler(fixture);

        var crossEraOnly = await handler.Handle(new Ac15UserDataQuery(5, GameEra.Yellow), CancellationToken.None);

        Assert.False(BitIsSet(crossEraOnly.SongFlags.ReleaseSongFlg, 103));
        Assert.False(BitIsSet(crossEraOnly.SongFlags.ToneFlg, 4));

        fixture.Context.YellowShopItemStates.AddRange(
            new YellowShopItemState
            {
                Baid = 5,
                SeasonId = 2,
                ItemType = Ac15ShopItemType.Song.ToProtocolValue(),
                ItemId = 103,
                ItemNo = 1,
                ItemPrice = 10,
                Status = Ac15ShopItemStatus.Unlocked,
                PurchasedAt = DateTime.UtcNow,
                UnlockedAt = DateTime.UtcNow
            },
            new YellowShopItemState
            {
                Baid = 5,
                SeasonId = 2,
                ItemType = Ac15ShopItemType.Tone.ToProtocolValue(),
                ItemId = 4,
                ItemNo = 2,
                ItemPrice = 20,
                Status = Ac15ShopItemStatus.Unlocked,
                PurchasedAt = DateTime.UtcNow,
                UnlockedAt = DateTime.UtcNow
            });
        await fixture.Context.SaveChangesAsync();

        var yellowPurchased = await handler.Handle(new Ac15UserDataQuery(5, GameEra.Yellow), CancellationToken.None);

        Assert.True(BitIsSet(yellowPurchased.SongFlags.ReleaseSongFlg, 103));
        Assert.True(BitIsSet(yellowPurchased.SongFlags.ToneFlg, 4));
    }

    private static bool BitIsSet(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;

    private static UserDataQueryHandler CreateUserDataHandler(YellowHandlerFixture fixture)
        => new(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UserDataQueryHandler>.Instance,
            Options.Create(new ServerSettings()));

    private static UpdateAc15PlayResultCommand CreateTokkunPlayResult(uint baid)
        => Ac15PlayResultTestFactory.Command(
            baid,
            GameEra.Yellow,
            playDatetime: "20260608120000",
            playMode: (uint)PlayMode.Tokkun,
            tokkun: new Ac15TokkunPlayResult(
                TutorialFlg: 7,
                StageData: new Ac15TokkunStageData(
                    BanacoinDatetime: "20260608120100",
                    TokkunSongCnt: 3,
                    TookunSongnoes: [101, 102, 101],
                    TokkunSpeedchangeCnt: 2,
                    TokkunAutoplayCnt: 3,
                    TokkunJumpCnt: 4)));

    private static void AssertNoTokkunHistorySurface(object response)
    {
        var propertyNames = response.GetType()
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();

        Assert.DoesNotContain(propertyNames, name => name.Contains("TokkunStage", StringComparison.Ordinal));
        Assert.DoesNotContain(propertyNames, name => name.Contains("BanacoinDatetime", StringComparison.Ordinal));
        Assert.DoesNotContain(propertyNames, name => name.Contains("TookunSongno", StringComparison.Ordinal));
        Assert.DoesNotContain(propertyNames, name => name.Contains("TokkunSongCnt", StringComparison.Ordinal));
        Assert.DoesNotContain(propertyNames, name => name.Contains("TokkunSpeedchange", StringComparison.Ordinal));
        Assert.DoesNotContain(propertyNames, name => name.Contains("TokkunAutoplay", StringComparison.Ordinal));
        Assert.DoesNotContain(propertyNames, name => name.Contains("TokkunJump", StringComparison.Ordinal));
    }
}
