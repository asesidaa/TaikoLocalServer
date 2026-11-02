using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.AdminApi.Mapping;

[Mapper(EnumMappingStrategy = EnumMappingStrategy.ByName)]
public static partial class AdminApiDataMapper
{
    public static partial DanBestData ToDanBestData(this DanScoreDatumNijiiro source);

    public static partial DanBestStageData ToDanBestStageData(this DanStageScoreDatumNijiiro source);

    public static partial SongBestData ToSongBestData(this SongBestDatumNijiiro source);

    public static partial AiSectionBestData ToAiSectionBestData(this AiSectionScoreDatumNijiiro source);

    public static partial SongPlayDatumDto ToSongPlayDatumDto(this SongPlayDatumNijiiro source);

    public static partial void ApplyBestLogTo(this SongPlayDatumNijiiro source, [MappingTarget] SongBestData target);
}
