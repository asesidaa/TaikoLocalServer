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
        var response = new BattleUserDataResponse.BattleUserNpcData
        {
            NpcId = common.NpcId,
            TotalExp = common.TotalExp,
            MaxDpn = common.MaxDpn,
            NpcCostumeId = common.NpcCostumeId,
            NpcCostumeFlg = common.NpcCostumeFlg,
            LastSelectSpecial1 = common.LastSelectSpecial1,
            LastSelectSpecial2 = common.LastSelectSpecial2,
            LastSelectSpecial3 = common.LastSelectSpecial3
        };

        if (common.ReleaseSpecialFlg is not null)
        {
            response.ReleaseSpecialFlg = common.ReleaseSpecialFlg;
        }

        return response;
    }

    private static BattleUserDataResponse.BattleUserTokenData MapTokenData(
        CommonBattleUserDataResponse.BattleUserTokenData common)
        => new()
        {
            TokenId = common.TokenId,
            TokenValue = common.TokenValue
        };
}
