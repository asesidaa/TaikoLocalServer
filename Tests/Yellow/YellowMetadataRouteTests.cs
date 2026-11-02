using TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;
using TaikoLocalServer.Application.Dtos;
using TaikoLocalServer.Application.ServerData;
using YellowWire = TaikoLocalServer.Adapters.GameProtocol.Yellow.Wire;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowMetadataRouteTests
{
    [Fact]
    public void InitialDataMapper_MapsYellowSpecificRows()
    {
        var common = new CommonInitialDataCheckResponse
        {
            Result = 1,
            SongHashVer = 123,
            DefaultSongFlg = [1, 2],
            AchievementSongBit = [3],
            UraReleaseBit = [4],
            IsDanplay = true,
            IsClose = false,
            IsItemshop = true,
            AryTelopDatas = [Info(10, 20)],
            AryEventFolderDatas = [Info(11, 21)],
            AryTaikojukuDatas = [Info(12, 22)],
            AryItemShopDatas = [Info(13, 23)],
            AryLegaltermsDatas = [Info(14, 24)]
        };

        var response = InitialDataMappers.Map(common);

        Assert.Equal(1u, response.Result);
        Assert.Equal(123u, response.SongHashVer);
        Assert.Equal([1, 2], response.HashDefaultSongFlg);
        Assert.Equal([3], response.HashMainichidojoAll);
        Assert.Equal([4], response.HashMainichidojoRare);
        Assert.True(response.IsDanplay);
        Assert.False(response.IsClose);
        Assert.True(response.IsItemshop);
        Assert.Equal(10u, Assert.Single(response.AryTelopDatas).InfoId);
        Assert.Equal(11u, Assert.Single(response.AryEventfolderDatas).InfoId);
        Assert.Equal(12u, Assert.Single(response.AryTaikojukuDatas).InfoId);
        Assert.Equal(13u, Assert.Single(response.AryItemshopDatas).InfoId);
        Assert.Equal(14u, Assert.Single(response.AryLegaltermsDatas).InfoId);
    }

    [Fact]
    public void FolderTelopTaikojukuAndItemShopMappers_MapCommonDtosToYellowWire()
    {
        var folder = FolderDataMappers.Map(new CommonGetFolderResponse
        {
            Result = 1,
            AryEventfolderDatas =
            [
                new EventFolderData { FolderId = 44, VerupNo = 8, SongNoes = [101, 102] }
            ]
        });
        var telop = GetTelopMappers.Map(new CommonGetTelopResponse
        {
            Result = 1,
            VerupNo = 9,
            StartDatetime = "2026-01-01 00:00:00",
            EndDatetime = "2026-01-31 23:59:59",
            Telop = "Yellow"
        });
        var taikojuku = TaikojukuMappers.Map(new CommonTaikojukuResponse
        {
            Result = 1,
            Packs =
            [
                new CommonTaikojukuResponse.Pack
                {
                    GetDan = 1,
                    VerupNo = 7,
                    Songs = [new CommonTaikojukuResponse.Song { SongNo = 101, Level = Difficulty.Normal }]
                }
            ]
        });
        var itemShop = ItemShopMappers.Map(new CommonItemShopInfoResponse
        {
            Result = 1,
            VerupNo = 55,
            SeasonId = 6,
            Telop = "Shop",
            StartDatetime = "20170315070000",
            EndDatetime = "20170630020000",
            AfterstartDays = 30,
            BeforecloseDays = 4,
            AryItemshopData =
            [
                new CommonItemShopInfoResponse.ItemShopData
                {
                    ItemNo = 1,
                    ItemType = 4,
                    ItemId = 300,
                    ItemPrice = 500
                }
            ]
        });

        var folderRow = Assert.Single(folder.AryEventfolderDatas);
        Assert.Equal(44u, folderRow.FolderId);
        Assert.Equal([101u, 102u], folderRow.SongNoes);
        Assert.Equal(9u, telop.VerupNo);
        Assert.Equal("Yellow", telop.Telop);
        var pack = Assert.Single(taikojuku.AryJukupackDatas);
        Assert.Equal(1u, pack.GetDan);
        Assert.Equal(101u, Assert.Single(pack.AryJukusongDatas).SongNo);
        Assert.Equal(55u, itemShop.VerupNo);
        Assert.Equal(6u, itemShop.SeasonId);
        Assert.Equal("20170315070000", itemShop.StartDatetime);
        Assert.Equal("20170630020000", itemShop.EndDatetime);
        Assert.Equal(30u, itemShop.AfterstartDays);
        Assert.Equal(4u, itemShop.BeforecloseDays);
        Assert.Equal(1u, Assert.Single(itemShop.AryItemshopDatas).ItemNo);
    }

    [Fact]
    public void RecommendTournamentAndChallengeMappers_MapSupportedYellowRows()
    {
        var recommend = RecommendMappers.Map(new CommonRecommendResponse
        {
            Result = 1,
            RecommendSong = 101,
            RecommendBestSong = [102, 103]
        });
        var tournament = TournamentMappers.Map(new CommonTournamentCheckResponse
        {
            Result = 1,
            RareRate = 2,
            SongHashVer = 3,
            AryGachaSongData =
            [
                new CommonTournamentCheckResponse.GachainfoData
                {
                    NormalGachaFlg = [1],
                    RareGachaFlg = [2]
                }
            ]
        });
        var challenge = ChallengeCompeMappers.Map(new CommonChallengeCompeResponse
        {
            Result = 1,
            AryChallengeStat =
            [
                new CommonChallengeCompeResponse.CompeData
                {
                    CompeId = 9,
                    AryTrackStat =
                    [
                        new CommonChallengeCompeResponse.TracksData
                        {
                            SongNo = 101,
                            Level = 4,
                            OptionFlg = [3],
                            StageMode = 1,
                            HighScore = 123456
                        }
                    ]
                }
            ]
        });

        Assert.Equal(101u, recommend.RecommendSong);
        Assert.Equal([102u, 103u], recommend.RecommendBestSongs);
        Assert.Equal(2u, tournament.RareRate);
        Assert.Equal(3u, tournament.SongHashVer);
        Assert.Equal([1], Assert.Single(tournament.AryGachaSongDatas).NormalGachaFlg);
        var challengeRow = Assert.Single(challenge.AryChallengeStats);
        Assert.Equal(9u, challengeRow.CompeId);
        Assert.Equal(123456u, Assert.Single(challengeRow.AryTrackStats).HighScore);
    }

    [Fact]
    public void ItemShopRequestMapper_UsesYellowEraWithoutPurchaseSemantics()
    {
        var query = ItemShopMappers.Map(new YellowWire.GetitemshopinfoRequest
        {
            ChassisId = "chassis",
            ShopId = "shop"
        });

        Assert.Equal(GameEra.Yellow, query.Era);
    }

    private static CommonInitialDataCheckResponse.InformationData Info(uint id, uint verupNo)
        => new() { InfoId = id, VerupNo = verupNo };
}
