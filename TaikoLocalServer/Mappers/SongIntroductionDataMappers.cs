using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Mappers;

[Mapper]
public static partial class SongIntroductionDataMappers
{
    public static partial GetSongIntroductionResponse MapTo3906(CommonGetSongIntroductionResponse response);

    public static partial Models.v3209.GetSongIntroductionResponse
        MapTo3209(CommonGetSongIntroductionResponse response);
}