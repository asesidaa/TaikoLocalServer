namespace TaikoLocalServer.Application.Ac15;

public static class Ac15CrownService
{
    public static byte[] BuildInflatedBody(
        IEnumerable<Ac15BestRow> bestRows,
        IEnumerable<uint> validSongNoes,
        Ac15ProtocolLimits limits)
    {
        var values = new ushort[limits.CrownSongCount];
        var validSongs = validSongNoes
            .Where(songNo => songNo < limits.CrownSongCount)
            .ToHashSet();

        foreach (var group in bestRows.GroupBy(row => row.SongId))
        {
            if (!validSongs.Contains(group.Key))
            {
                continue;
            }

            var easy = Ac15CrownState.None;
            var normal = Ac15CrownState.None;
            var hard = Ac15CrownState.None;
            var oni = Ac15CrownState.None;
            var ura = Ac15CrownState.None;

            foreach (var row in group)
            {
                var state = MapCrownState(row.BestCrown);
                switch (row.Difficulty)
                {
                    case Difficulty.Easy:
                        easy = Max(easy, state);
                        break;
                    case Difficulty.Normal:
                        normal = Max(normal, state);
                        break;
                    case Difficulty.Hard:
                        hard = Max(hard, state);
                        break;
                    case Difficulty.Oni:
                        oni = Max(oni, state);
                        break;
                    case Difficulty.UraOni:
                        ura = Max(ura, state);
                        break;
                }
            }

            values[group.Key] = Ac15ProtocolBytes.BuildCrownValue(easy, normal, hard, oni, ura);
        }

        return Ac15ProtocolBytes.PackTenBitValues(values, limits.CrownPackedBytes, limits.CrownSongCount);
    }

    public static byte[] BuildCatalogOrderBody(
        IEnumerable<Ac15BestRow> bestRows,
        IEnumerable<uint> fileOrderSongNoes,
        Ac15ProtocolLimits limits)
    {
        var valuesBySongId = bestRows
            .GroupBy(row => row.SongId)
            .ToDictionary(group => group.Key, BuildCrownValue);
        var values = fileOrderSongNoes
            .Take(limits.CrownSongCount)
            .Select(songNo => valuesBySongId.GetValueOrDefault(songNo))
            .ToArray();

        return Ac15ProtocolBytes.PackTenBitValues(values, limits.CrownPackedBytes, limits.CrownSongCount);
    }

    public static byte[] BuildEightBitIndexedBody(
        IEnumerable<Ac15BestRow> bestRows,
        IEnumerable<uint> validSongNoes,
        Ac15ProtocolLimits limits)
    {
        var values = new byte[limits.CrownSongCount];
        var validSongs = validSongNoes
            .Where(songNo => songNo < limits.CrownSongCount)
            .ToHashSet();

        foreach (var group in bestRows.GroupBy(row => row.SongId))
        {
            if (!validSongs.Contains(group.Key))
            {
                continue;
            }

            values[group.Key] = BuildEightBitCrownValue(group);
        }

        return values;
    }

    public static Ac15CrownState MapCrownState(CrownType crown) => crown switch
    {
        CrownType.Clear => Ac15CrownState.Clear,
        CrownType.Gold => Ac15CrownState.FullCombo,
        CrownType.Dondaful => Ac15CrownState.FullCombo,
        _ => Ac15CrownState.None
    };

    private static Ac15CrownState Max(Ac15CrownState left, Ac15CrownState right)
        => left >= right ? left : right;

    private static ushort BuildCrownValue(IEnumerable<Ac15BestRow> rows)
    {
        var easy = Ac15CrownState.None;
        var normal = Ac15CrownState.None;
        var hard = Ac15CrownState.None;
        var oni = Ac15CrownState.None;
        var ura = Ac15CrownState.None;

        foreach (var row in rows)
        {
            var state = MapCrownState(row.BestCrown);
            switch (row.Difficulty)
            {
                case Difficulty.Easy:
                    easy = Max(easy, state);
                    break;
                case Difficulty.Normal:
                    normal = Max(normal, state);
                    break;
                case Difficulty.Hard:
                    hard = Max(hard, state);
                    break;
                case Difficulty.Oni:
                    oni = Max(oni, state);
                    break;
                case Difficulty.UraOni:
                    ura = Max(ura, state);
                    break;
            }
        }

        return Ac15ProtocolBytes.BuildCrownValue(easy, normal, hard, oni, ura);
    }

    private static byte BuildEightBitCrownValue(IEnumerable<Ac15BestRow> rows)
    {
        var easy = Ac15CrownState.None;
        var normal = Ac15CrownState.None;
        var hard = Ac15CrownState.None;
        var oni = Ac15CrownState.None;

        foreach (var row in rows)
        {
            var state = MapCrownState(row.BestCrown);
            switch (row.Difficulty)
            {
                case Difficulty.Easy:
                    easy = Max(easy, state);
                    break;
                case Difficulty.Normal:
                    normal = Max(normal, state);
                    break;
                case Difficulty.Hard:
                    hard = Max(hard, state);
                    break;
                case Difficulty.Oni:
                    oni = Max(oni, state);
                    break;
            }
        }

        return (byte)Ac15ProtocolBytes.BuildCrownValue(easy, normal, hard, oni, Ac15CrownState.None);
    }
}
