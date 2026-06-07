using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Domain.Entities;

namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;

public static class CrownsDataMappers
{
    public static byte[] BuildRawInflatedBody(IEnumerable<SongBestDatumYellow> bestRows, IYellowCatalog yellow)
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
            yellow.YellowMusicInfos.Keys,
            Ac15EraProfiles.Yellow.Limits);
    }
}
