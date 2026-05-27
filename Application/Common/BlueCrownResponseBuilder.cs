namespace TaikoLocalServer.Application.Common;

public static class BlueCrownResponseBuilder
{
    public static byte[] BuildInflatedBody(IEnumerable<SongBestDatumBlue> bestRows, IBlueCatalog blue)
    {
        var values = new ushort[1024];
        var validSongNoes = blue.BlueMusicInfos
            .Where(pair => pair.Key < 1024)
            .Select(pair => pair.Key)
            .ToHashSet();

        foreach (var group in bestRows.GroupBy(row => row.SongId))
        {
            if (!validSongNoes.Contains(group.Key))
            {
                continue;
            }

            var easy = BlueCrownState.None;
            var normal = BlueCrownState.None;
            var hard = BlueCrownState.None;
            var oni = BlueCrownState.None;
            var ura = BlueCrownState.None;

            foreach (var row in group)
            {
                var state = BluePlayResultMapping.MapBlueCrownState(row.BestCrown);
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

            values[group.Key] = BlueProtocolBytes.BuildBlueCrownValue(easy, normal, hard, oni, ura);
        }

        return BlueProtocolBytes.PackBlueCrowns(values);
    }

    private static BlueCrownState Max(BlueCrownState left, BlueCrownState right)
        => left >= right ? left : right;
}
