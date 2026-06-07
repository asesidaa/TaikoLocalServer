namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;

public static class TaikojukuMappers
{
    private const int MaxSongsPerPack = 10;
    private const uint MaxYellowSongNo = BlueProtocolBytes.SongFlagBytes * 8 - 1;
    private const uint MaxYellowCourseLevel = 4;

    public static TaikojukuResponse Map(CommonTaikojukuResponse common)
    {
        var response = new TaikojukuResponse { Result = common.Result };

        foreach (var pack in common.Packs)
        {
            if (pack.GetDan is < 1 or > 25)
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

    private static bool IsValidJukusong(CommonTaikojukuResponse.Song song)
        => song.SongNo is > 0
            && song.SongNo <= MaxYellowSongNo
            && song.Level <= MaxYellowCourseLevel;
}
