using FinalWire = TaikoLocalServer.Adapters.GameProtocol.Kimidori.FinalWire;
using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Mappers;

[Mapper]
public static partial class FinalTaikojukuMappers
{
    [MapProperty(nameof(CommonTaikojukuResponse.Packs), nameof(FinalWire.TaikojukuResponse.AryJukupackDatas))]
    public static partial FinalWire.TaikojukuResponse Map(CommonTaikojukuResponse common);

    [MapProperty(nameof(CommonTaikojukuResponse.Pack.Songs), nameof(FinalWire.TaikojukuResponse.JukupackData.AryJukusongDatas))]
    [MapperIgnoreSource(nameof(CommonTaikojukuResponse.Pack.VerupNo))]
    private static partial FinalWire.TaikojukuResponse.JukupackData MapPack(
        CommonTaikojukuResponse.Pack pack);

    [MapProperty(nameof(CommonTaikojukuResponse.Song.Level), nameof(FinalWire.TaikojukuResponse.JukupackData.JukusongData.Level), Use = nameof(MapDifficulty))]
    private static partial FinalWire.TaikojukuResponse.JukupackData.JukusongData MapSong(
        CommonTaikojukuResponse.Song song);

    private static uint MapDifficulty(Difficulty value) => Ac15Difficulty.ToProtocol(value);
}
