using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Application.Ac15;

[Mapper]
public static partial class Ac15NormalPlayMapper
{
    public static partial SongPlayDatumBlue ToBlueSongPlayDatum(Ac15PlayRow row);

    public static partial SongPlayDatumGreen ToGreenSongPlayDatum(Ac15PlayRow row);

    public static partial SongPlayDatumYellow ToYellowSongPlayDatum(Ac15PlayRow row);

    public static SongBestDatumBlue ToBlueSongBestDatum(uint baid, Ac15BestRow row, bool allowCrownUpdate)
    {
        var best = ToBlueSongBestDatum(row);
        best.Baid = baid;
        if (!allowCrownUpdate)
        {
            best.BestCrown = CrownType.None;
        }

        return best;
    }

    public static SongBestDatumGreen ToGreenSongBestDatum(uint baid, Ac15BestRow row, bool allowCrownUpdate)
    {
        var best = ToGreenSongBestDatum(row);
        best.Baid = baid;
        if (!allowCrownUpdate)
        {
            best.BestCrown = CrownType.None;
        }

        return best;
    }

    public static SongBestDatumYellow ToYellowSongBestDatum(uint baid, Ac15BestRow row, bool allowCrownUpdate)
    {
        var best = ToYellowSongBestDatum(row);
        best.Baid = baid;
        if (!allowCrownUpdate)
        {
            best.BestCrown = CrownType.None;
        }

        return best;
    }

    private static partial SongBestDatumBlue ToBlueSongBestDatum(Ac15BestRow row);

    private static partial SongBestDatumGreen ToGreenSongBestDatum(Ac15BestRow row);

    private static partial SongBestDatumYellow ToYellowSongBestDatum(Ac15BestRow row);
}
