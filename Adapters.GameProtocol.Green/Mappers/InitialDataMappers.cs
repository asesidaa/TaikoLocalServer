using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper]
public static partial class InitialDataMappers
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
            IsItemshop = common.IsItemshop,
            IsGhostbattleplay = common.IsGhostbattleplay
        };

        response.AryTelopDatas.AddRange(common.AryGreenTelopDatas.Select(MapInformation));
        response.AryEventfolderDatas.AddRange(common.AryGreenEventFolderDatas.Select(MapInformation));
        response.AryTaikojukuDatas.AddRange(common.AryGreenTaikojukuDatas.Select(MapInformation));
        response.AryItemshopDatas.AddRange(common.AryGreenItemShopDatas.Select(MapInformation));

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
