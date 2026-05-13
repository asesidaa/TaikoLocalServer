using Riok.Mapperly.Abstractions;
using TaikoLocalServer.Application.Common;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper]
public static partial class TaikojukuMappers
{
    private const int MaxSongsPerPack = 10;
    private const uint MaxGreenSongNo = GreenProtocolBytes.SongFlagBytes * 8 - 1;
    private const uint MaxGreenCourseLevel = 4;

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

            foreach (var song in songs)
            {
                wirePack.AryJukusongDatas.Add(new TaikojukuResponse.JukupackData.JukusongData
                {
                    SongNo = song.SongNo,
                    Level = song.Level
                });
            }

            response.AryJukupackDatas.Add(wirePack);
        }

        return response;
    }

    private static bool IsValidDanSlot(uint value)
        => value is >= 1 and <= 25;

    private static bool IsValidJukusong(CommonTaikojukuResponse.Song song)
        => song.SongNo is > 0
            && song.SongNo <= MaxGreenSongNo
            && song.Level <= MaxGreenCourseLevel;
}
