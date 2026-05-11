using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper]
public static partial class TournamentMappers
{
    public static TournamentcheckResponse Map(CommonTournamentCheckResponse common)
    {
        return new TournamentcheckResponse
        {
            Result = common.Result,
            RareRate = common.RareRate,
            SongHashVer = common.SongHashVer
        };
    }
}
