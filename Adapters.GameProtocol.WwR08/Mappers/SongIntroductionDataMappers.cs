using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.WwR08.Mappers;

[Mapper]
public static partial class SongIntroductionDataMappers
{
    public static partial GetSongIntroductionResponse MapToWW08(CommonGetSongIntroductionResponse response);
}
