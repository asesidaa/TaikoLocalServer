using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Application.Common;

namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;

[Mapper]
public static partial class TaikojukuMappers
{
    private const int MaxSongsPerPack = 10;
    private const uint MaxBlueSongNo = BlueProtocolBytes.SongFlagBytes * 8 - 1;
    private const uint MaxBlueCourseLevel = 4;

    public static TaikojukuResponse Map(CommonTaikojukuResponse common)
    {
        var response = new TaikojukuResponse { Result = common.Result };

        foreach (var pack in common.Packs)
        {
            if (!IsValidDanSlot(pack.GetDan))
            {
                continue;
            }

            var songs = pack.Songs
                .Where(IsValidJukusong)
                .Take(MaxSongsPerPack)
                .ToArray();

            if (songs.Length == 0)
            {
                continue;
            }

            var wirePack = new TaikojukuResponse.JukupackData
            {
                GetDan = pack.GetDan,
                VerupNo = pack.VerupNo
            };

            wirePack.AryJukusongDatas.AddRange(songs.Select(MapSong));

            response.AryJukupackDatas.Add(wirePack);
        }

        return response;
    }

    private static bool IsValidDanSlot(uint value)
        => value is >= 1 and <= 25;

    private static bool IsValidJukusong(CommonTaikojukuResponse.Song song)
        => song.SongNo is > 0
            && song.SongNo <= MaxBlueSongNo
            && song.Level <= MaxBlueCourseLevel;

    private static partial TaikojukuResponse.JukupackData.JukusongData MapSong(
        CommonTaikojukuResponse.Song song);
}
