using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper]
public static partial class InitialDataMappers
{
    public static InitialdatacheckResponse Map(CommonInitialDataCheckResponse common)
    {
        // TODO iter 2: map Green hash flags and per-catalog verup data.
        return new InitialdatacheckResponse
        {
            Result = common.Result,
            IsDanplay = true,
            IsClose = false,
            IsItemshop = true,
            IsGhostbattleplay = true
        };
    }
}
