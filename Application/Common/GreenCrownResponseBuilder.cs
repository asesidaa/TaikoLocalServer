using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Common;

public static class GreenCrownResponseBuilder
{
    public static byte[] BuildInflatedBody(IEnumerable<SongBestDatumGreen> bestRows, IGreenCatalog green)
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
            green.GreenMusicInfos.Keys,
            Ac15EraProfiles.Green.Limits);
    }
}
