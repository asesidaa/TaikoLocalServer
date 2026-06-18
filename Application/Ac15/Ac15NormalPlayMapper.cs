using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Application.Ac15;

[Mapper]
public static partial class Ac15NormalPlayMapper
{
    [MapperIgnoreTarget(nameof(SongPlayDatumBlue.Id))]
    [MapperIgnoreTarget(nameof(SongPlayDatumBlue.Ba))]
    public static partial SongPlayDatumBlue ToBlueSongPlayDatum(Ac15PlayRow row);

    [MapperIgnoreTarget(nameof(SongPlayDatumGreen.Id))]
    [MapperIgnoreTarget(nameof(SongPlayDatumGreen.Ba))]
    public static partial SongPlayDatumGreen ToGreenSongPlayDatum(Ac15PlayRow row);

    [MapperIgnoreTarget(nameof(SongPlayDatumYellow.Id))]
    [MapperIgnoreTarget(nameof(SongPlayDatumYellow.Ba))]
    public static partial SongPlayDatumYellow ToYellowSongPlayDatum(Ac15PlayRow row);

    [MapperIgnoreTarget(nameof(SongPlayDatumRed.Id))]
    [MapperIgnoreTarget(nameof(SongPlayDatumRed.Ba))]
    public static partial SongPlayDatumRed ToRedSongPlayDatum(Ac15PlayRow row);

    [MapperIgnoreTarget(nameof(SongPlayDatumWhite.Id))]
    [MapperIgnoreTarget(nameof(SongPlayDatumWhite.Ba))]
    public static partial SongPlayDatumWhite ToWhiteSongPlayDatum(Ac15PlayRow row);

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

    public static SongBestDatumRed ToRedSongBestDatum(uint baid, Ac15BestRow row, bool allowCrownUpdate)
    {
        var best = ToRedSongBestDatum(row);
        best.Baid = baid;
        if (!allowCrownUpdate)
        {
            best.BestCrown = CrownType.None;
        }

        return best;
    }

    public static SongBestDatumWhite ToWhiteSongBestDatum(uint baid, Ac15BestRow row, bool allowCrownUpdate)
    {
        var best = ToWhiteSongBestDatum(row);
        best.Baid = baid;
        if (!allowCrownUpdate)
        {
            best.BestCrown = CrownType.None;
        }

        return best;
    }

    [MapperIgnoreTarget(nameof(SongBestDatumBlue.Baid))]
    [MapperIgnoreTarget(nameof(SongBestDatumBlue.Ba))]
    private static partial SongBestDatumBlue ToBlueSongBestDatum(Ac15BestRow row);

    [MapperIgnoreTarget(nameof(SongBestDatumGreen.Baid))]
    [MapperIgnoreTarget(nameof(SongBestDatumGreen.Ba))]
    private static partial SongBestDatumGreen ToGreenSongBestDatum(Ac15BestRow row);

    [MapperIgnoreTarget(nameof(SongBestDatumYellow.Baid))]
    [MapperIgnoreTarget(nameof(SongBestDatumYellow.Ba))]
    private static partial SongBestDatumYellow ToYellowSongBestDatum(Ac15BestRow row);

    [MapperIgnoreTarget(nameof(SongBestDatumRed.Baid))]
    [MapperIgnoreTarget(nameof(SongBestDatumRed.Ba))]
    private static partial SongBestDatumRed ToRedSongBestDatum(Ac15BestRow row);

    [MapperIgnoreTarget(nameof(SongBestDatumWhite.Baid))]
    [MapperIgnoreTarget(nameof(SongBestDatumWhite.Ba))]
    private static partial SongBestDatumWhite ToWhiteSongBestDatum(Ac15BestRow row);
}
