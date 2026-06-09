using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;

[Mapper]
public static partial class BattleUserDataMappers
{
    public static BattleUserDataResponse Map(CommonBattleUserDataResponse common)
    {
        var response = new BattleUserDataResponse
        {
            Result = common.Result
        };

        if (common.ReleaseInfoFlg is not null)
        {
            response.ReleaseInfoFlg = common.ReleaseInfoFlg;
        }

        if (common.ReleaseBattleStageFlg is not null)
        {
            response.ReleaseBattleStageFlg = common.ReleaseBattleStageFlg;
        }

        if (common.LastBattleStageId is { } lastBattleStageId)
        {
            response.LastBattleStageId = lastBattleStageId;
        }

        if (common.LastBossLife is { } lastBossLife)
        {
            response.LastBossLife = lastBossLife;
        }

        if (common.LastNpcId is { } lastNpcId)
        {
            response.LastNpcId = lastNpcId;
        }

        response.NpcDatas.AddRange(common.NpcDatas.Select(MapNpcData));
        response.AryTokenDatas.AddRange(common.AryTokenDatas.Select(MapTokenData));

        if (common.AssignStageId is { } assignStageId)
        {
            response.AssignStageId = assignStageId;
        }

        return response;
    }

    private static BattleUserDataResponse.BattleUserNpcData MapNpcData(
        CommonBattleUserDataResponse.BattleUserNpcData common)
    {
        var response = MapNpcDataCore(common);

        if (common.ReleaseSpecialFlg is not null)
        {
            response.ReleaseSpecialFlg = common.ReleaseSpecialFlg;
        }

        return response;
    }

    private static partial BattleUserDataResponse.BattleUserNpcData MapNpcDataCore(
        CommonBattleUserDataResponse.BattleUserNpcData common);

    private static partial BattleUserDataResponse.BattleUserTokenData MapTokenData(
        CommonBattleUserDataResponse.BattleUserTokenData common);
}
