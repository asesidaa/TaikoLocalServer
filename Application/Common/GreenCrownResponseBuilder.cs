namespace TaikoLocalServer.Application.Common;

public static class GreenCrownResponseBuilder
{
    public static byte[] BuildInflatedBody(IEnumerable<SongBestDatumGreen> bestRows, IGreenCatalog green)
    {
        var values = new ushort[1024];
        var validSongNoes = green.GreenMusicInfos
            .Where(pair => pair.Key < 1024)
            .Select(pair => pair.Key)
            .ToHashSet();

        foreach (var group in bestRows.GroupBy(row => row.SongId))
        {
            if (!validSongNoes.Contains(group.Key))
            {
                continue;
            }

            var easy = GreenCrownState.None;
            var normal = GreenCrownState.None;
            var hard = GreenCrownState.None;
            var oni = GreenCrownState.None;
            var ura = GreenCrownState.None;

            foreach (var row in group)
            {
                var state = GreenPlayResultMapping.MapGreenCrownState(row.BestCrown);
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

            values[group.Key] = GreenProtocolBytes.BuildGreenCrownValue(easy, normal, hard, oni, ura);
        }

        return GreenProtocolBytes.PackGreenCrowns(values);
    }

    private static GreenCrownState Max(GreenCrownState left, GreenCrownState right)
        => left >= right ? left : right;
}
