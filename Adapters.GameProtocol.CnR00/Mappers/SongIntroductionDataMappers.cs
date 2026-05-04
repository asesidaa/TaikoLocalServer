using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.CnR00.Mappers;

[Mapper]
public static partial class SongIntroductionDataMappers
{
    public static partial GetSongIntroductionResponse MapToCN00(CommonGetSongIntroductionResponse response);
}
