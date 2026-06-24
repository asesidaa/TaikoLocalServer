using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Domain.Entities;

namespace TaikoLocalServer.Adapters.GameProtocol.Kimidori.Mappers;

public static class CrownsDataMappers
{
    public static byte[] BuildHashIndexedBody(IEnumerable<SongBestDatumKimidori> bestRows, IKimidoriCatalog kimidori)
    {
        var canonicalRows = bestRows.Select(row => new Ac15BestRow(
            row.SongId,
            row.Difficulty,
            row.IsShin,
            row.BestScore,
            row.BestRate,
            row.BestCrown));

        var inflated = Ac15CrownService.BuildInflatedBody(
            canonicalRows,
            kimidori.KimidoriMusicInfos.Keys,
            Ac15EraProfiles.Kimidori.Limits);
        return Ac15SongHashCodec.CompactTenBitValues(inflated, kimidori.SongHashTable);
    }
}
