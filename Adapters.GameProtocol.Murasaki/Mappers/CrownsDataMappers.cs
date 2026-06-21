using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Domain.Entities;

namespace TaikoLocalServer.Adapters.GameProtocol.Murasaki.Mappers;

public static class CrownsDataMappers
{
    public static byte[] BuildRawInflatedBody(IEnumerable<SongBestDatumMurasaki> bestRows, IMurasakiCatalog murasaki)
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
            murasaki.MurasakiMusicInfos.Keys,
            Ac15EraProfiles.Murasaki.Limits);
    }
}
