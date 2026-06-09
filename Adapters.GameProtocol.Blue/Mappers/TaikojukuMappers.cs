using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;

[Mapper]
public static partial class TaikojukuMappers
{
    [MapProperty(nameof(CommonTaikojukuResponse.Packs), nameof(TaikojukuResponse.AryJukupackDatas))]
    public static partial TaikojukuResponse Map(CommonTaikojukuResponse common);

    [MapProperty(nameof(CommonTaikojukuResponse.Pack.Songs), nameof(TaikojukuResponse.JukupackData.AryJukusongDatas))]
    private static partial TaikojukuResponse.JukupackData MapPack(
        CommonTaikojukuResponse.Pack pack);

    private static partial TaikojukuResponse.JukupackData.JukusongData MapSong(
        CommonTaikojukuResponse.Song song);
}
