using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;

[Mapper]
public static partial class BattleUserDataMappers
{
    public static partial BattleUserDataResponse Map(CommonBattleUserDataResponse common);

    private static partial BattleUserDataResponse.BattleUserNpcData MapNpcData(
        CommonBattleUserDataResponse.BattleUserNpcData common);

    private static partial BattleUserDataResponse.BattleUserTokenData MapTokenData(
        CommonBattleUserDataResponse.BattleUserTokenData common);
}
