using TaikoLocalServer.Adapters.GameProtocol.Red.Mappers;
using TaikoLocalServer.Application.ServerData;

namespace TaikoLocalServer.Tests.Red;

public sealed class RedMetadataRouteTests
{
    [Fact]
    public void InitialDataMapper_MapsRedSpecificRows()
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
            AryTelopDatas = [Info(10, 20)],
            AryEventFolderDatas = [Info(11, 21)],
            AryTaikojukuDatas = [Info(12, 22)],
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
        Assert.Equal(10u, Assert.Single(response.AryTelopDatas).InfoId);
        Assert.Equal(11u, Assert.Single(response.AryEventfolderDatas).InfoId);
        Assert.Equal(12u, Assert.Single(response.AryTaikojukuDatas).InfoId);
        Assert.Equal(14u, Assert.Single(response.AryLegaltermsDatas).InfoId);
    }

    [Fact]
    public void FolderTelopTaikojukuAndRecommendMappers_MapCommonDtosToRedWire()
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
            Telop = "Red"
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
                    Songs = [new CommonTaikojukuResponse.Song { SongNo = 101, Level = 2 }]
                }
            ]
        });
        var recommend = RecommendMappers.Map(new CommonRecommendResponse
        {
            Result = 1,
            RecommendSong = 101,
            RecommendBestSong = [102, 103]
        });

        var folderRow = Assert.Single(folder.AryEventfolderDatas);
        Assert.Equal(44u, folderRow.FolderId);
        Assert.Equal([101u, 102u], folderRow.SongNoes);
        Assert.Equal(9u, telop.VerupNo);
        Assert.Equal("Red", telop.Telop);
        var pack = Assert.Single(taikojuku.AryJukupackDatas);
        Assert.Equal(1u, pack.GetDan);
        Assert.Equal(101u, Assert.Single(pack.AryJukusongDatas).SongNo);
        Assert.Equal(101u, recommend.RecommendSong);
        Assert.Equal([102u, 103u], recommend.RecommendBestSongs);
    }

    private static CommonInitialDataCheckResponse.InformationData Info(uint id, uint verupNo)
        => new() { InfoId = id, VerupNo = verupNo };
}
