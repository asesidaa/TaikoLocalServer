using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper]
public static partial class ChallengeCompeMappers
{
    public static ChallengeCompeResponse Map(CommonChallengeCompeResponse common)
    {
        return new ChallengeCompeResponse { Result = common.Result };
    }
}
