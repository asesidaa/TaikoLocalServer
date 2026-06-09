using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper]
public static partial class ChallengeCompeMappers
{
    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial ChallengeCompeResponse Map(CommonChallengeCompeResponse common);
}
