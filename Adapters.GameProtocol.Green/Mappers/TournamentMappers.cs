using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper]
public static partial class TournamentMappers
{
    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial TournamentcheckResponse Map(CommonTournamentCheckResponse common);
}
