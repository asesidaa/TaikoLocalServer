using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Domain.Entities;

namespace TaikoLocalServer.Adapters.GameProtocol.Red.Mappers;

public static class CrownsDataMappers
{
    public static byte[] BuildRawInflatedBody(IEnumerable<SongBestDatumRed> bestRows, IRedCatalog red)
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
            red.RedMusicInfos.Keys,
            Ac15EraProfiles.Red.Limits);
    }
}
