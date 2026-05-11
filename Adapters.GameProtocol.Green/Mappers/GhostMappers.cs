using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper]
public static partial class GhostMappers
{
    public static GetghostdataResponse Map(CommonGhostDataResponse common)
    {
        return new GetghostdataResponse
        {
            Result = common.Result,
            ReleaseInfoFlag = common.ReleaseInfoFlag,
            PlayedSongFlag = common.PlayedSongFlag,
            TotalWinnings = common.TotalWinnings
        };
    }

    public static GetghostscoreResponse Map(CommonGhostScoreResponse common)
    {
        return new GetghostscoreResponse { Result = common.Result };
    }
}
