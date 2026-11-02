using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;

[Mapper]
public static partial class TaikojukuMappers
{
    [MapProperty(nameof(CommonTaikojukuResponse.Packs), nameof(TaikojukuResponse.AryJukupackDatas))]
    public static partial TaikojukuResponse Map(CommonTaikojukuResponse common);

    [MapProperty(nameof(CommonTaikojukuResponse.Pack.Songs), nameof(TaikojukuResponse.JukupackData.AryJukusongDatas))]
    private static partial TaikojukuResponse.JukupackData MapPack(
        CommonTaikojukuResponse.Pack pack);

    [MapProperty(nameof(CommonTaikojukuResponse.Song.Level), nameof(TaikojukuResponse.JukupackData.JukusongData.Level), Use = nameof(@Ac15MapperNormalization.ToProtocolDifficulty))]
    private static partial TaikojukuResponse.JukupackData.JukusongData MapSong(
        CommonTaikojukuResponse.Song song);
}
