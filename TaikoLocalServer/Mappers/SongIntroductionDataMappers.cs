using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Mappers;

[Mapper]
public static partial class SongIntroductionDataMappers
{
    public static partial GetSongIntroductionResponse MapToWW08(CommonGetSongIntroductionResponse response);

    public static partial Models.CN00.GetSongIntroductionResponse
        MapToCN00(CommonGetSongIntroductionResponse response);
}