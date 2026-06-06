using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Common;

public static class BlueCrownResponseBuilder
{
    public static byte[] BuildInflatedBody(IEnumerable<SongBestDatumBlue> bestRows, IBlueCatalog blue)
    {
        var canonicalRows = bestRows.Select(row => new Ac15BestRow(
            row.SongId,
            row.Difficulty,
            row.IsShin,
            row.BestScore,
            row.BestRate,
            row.BestCrown));

        return Ac15CrownService.BuildInflatedBody(
            canonicalRows,
            blue.BlueMusicInfos.Keys,
            Ac15EraProfiles.Blue.Limits);
    }
}
