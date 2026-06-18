using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Domain.Entities;

namespace TaikoLocalServer.Adapters.GameProtocol.White.Mappers;

public static class CrownsDataMappers
{
    public static byte[] BuildRawInflatedBody(IEnumerable<SongBestDatumWhite> bestRows, IWhiteCatalog white)
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
            white.WhiteMusicInfos.Keys,
            Ac15EraProfiles.White.Limits);
    }
}
