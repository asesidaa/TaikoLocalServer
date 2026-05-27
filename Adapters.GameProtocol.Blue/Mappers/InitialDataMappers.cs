using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Application.Dtos;

namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;

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
            IsItemshop = common.IsItemshop
        };

        response.AryTelopDatas.AddRange(common.AryBlueTelopDatas.Select(MapInformation));
        response.AryEventfolderDatas.AddRange(common.AryBlueEventFolderDatas.Select(MapInformation));
        response.AryTaikojukuDatas.AddRange(common.AryBlueTaikojukuDatas.Select(MapInformation));
        response.AryItemshopDatas.AddRange(common.AryBlueItemShopDatas.Select(MapInformation));
        response.AryLegaltermsDatas.AddRange(common.AryBlueLegaltermsDatas.Select(MapInformation));

        if (common.IsBattleplay is { } isBattleplay)
        {
            response.IsBattleplay = isBattleplay;
        }

        if (common.ReleaseBattleStageFlg is not null)
        {
            response.ReleaseBattleStageFlg = common.ReleaseBattleStageFlg;
        }

        if (common.ReleaseBattleSpecialFlg is not null)
        {
            response.ReleaseBattleSpecialFlg = common.ReleaseBattleSpecialFlg;
        }

        if (common.BattleBondsLvCap is { } battleBondsLvCap)
        {
            response.BattleBondsLvCap = battleBondsLvCap;
        }

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
