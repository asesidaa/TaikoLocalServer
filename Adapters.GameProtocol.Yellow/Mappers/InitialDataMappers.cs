namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;

public static class InitialDataMappers
{
    public static InitialdatacheckResponse Map(CommonInitialDataCheckResponse common)
    {
        var response = new InitialdatacheckResponse
        {
            Result = common.Result,
            SongHashVer = common.SongHashVer,
            HashDefaultSongFlg = common.DefaultSongFlg,
            HashMainichidojoAll = common.AchievementSongBit,
            HashMainichidojoRare = common.UraReleaseBit,
            IsDanplay = common.IsDanplay,
            IsClose = common.IsClose,
            IsItemshop = common.IsItemshop
        };

        response.AryTelopDatas.AddRange(common.AryYellowTelopDatas.Select(MapInformation));
        response.AryEventfolderDatas.AddRange(common.AryYellowEventFolderDatas.Select(MapInformation));
        response.AryTaikojukuDatas.AddRange(common.AryYellowTaikojukuDatas.Select(MapInformation));
        response.AryItemshopDatas.AddRange(common.AryYellowItemShopDatas.Select(MapInformation));
        response.AryLegaltermsDatas.AddRange(common.AryYellowLegaltermsDatas.Select(MapInformation));

        return response;
    }

    private static InitialdatacheckResponse.InformationData MapInformation(
        CommonInitialDataCheckResponse.InformationData common)
        => new()
        {
            InfoId = common.InfoId,
            VerupNo = common.VerupNo
        };
}
