using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Application.Ac15;

public static class Ac15TaikojukuService
{
    public static CommonTaikojukuResponse BuildResponse(
        IReadOnlyList<uint> requestedDans,
        IReadOnlyList<Ac15TaikojukuEntry> packs,
        IReadOnlyList<Ac15MusicInfoEntry> musicFileOrder,
        IReadOnlyCollection<uint> validSongNoes,
        Ac15ProtocolLimits limits)
    {
        var requestedSlots = GetRequestedSlots(requestedDans, limits);
        var validPacksBySlot = packs
            .Where(pack => IsValidDanSlot(pack.ChallengeLevel, limits))
            .GroupBy(pack => pack.ChallengeLevel)
            .ToDictionary(group => group.Key, group => group.First());

        var selectedPacks = new List<Ac15TaikojukuEntry>();
        foreach (var slot in requestedSlots)
        {
            if (validPacksBySlot.TryGetValue(slot, out var pack))
            {
                selectedPacks.Add(pack);
                continue;
            }

            var fallback = CreateFallbackPack(musicFileOrder, slot, selectedPacks.Count, limits);
            if (fallback is not null)
            {
                selectedPacks.Add(fallback);
            }
        }

        return new CommonTaikojukuResponse
        {
            Result = 1,
            Packs = selectedPacks
                .Select(pack => ToCommonPack(pack, validSongNoes, limits))
                .Where(pack => pack.Songs.Count > 0)
                .ToList()
        };
    }

    private static IReadOnlyList<uint> GetRequestedSlots(IReadOnlyList<uint> requestedDans, Ac15ProtocolLimits limits)
    {
        var requestedSlots = requestedDans
            .Where(slot => IsValidDanSlot(slot, limits))
            .Distinct()
            .ToArray();

        if (requestedSlots.Length > 0 || requestedDans.Count == 0)
        {
            return requestedSlots;
        }

        return Enumerable.Range(1, Math.Min(requestedDans.Count, limits.MaxRequestedTaikojukuSlots))
            .Select(slot => (uint)slot)
            .ToArray();
    }

    private static bool IsValidDanSlot(uint slot, Ac15ProtocolLimits limits)
        => slot >= limits.MinNormalDanId && slot <= limits.MaxNormalDanId;

    private static Ac15TaikojukuEntry? CreateFallbackPack(
        IReadOnlyList<Ac15MusicInfoEntry> musicFileOrder,
        uint slot,
        int index,
        Ac15ProtocolLimits limits)
    {
        var songs = musicFileOrder
            .Skip(index * 3)
            .Take(3)
            .ToArray();
        if (songs.Length == 0)
        {
            songs = musicFileOrder.Take(3).ToArray();
        }

        if (songs.Length == 0)
        {
            return null;
        }

        return new Ac15TaikojukuEntry
        {
            UniqueId = slot,
            ChallengeLevel = slot,
            Songs = songs.Select(song => new Ac15TaikojukuSong
            {
                SongNo = song.SongNo,
                Level = Ac15Difficulty.FromSequentialIndex(index, limits.MinCourseLevel, limits.MaxCourseLevel),
                MusicId = song.MusicId
            }).ToArray()
        };
    }

    private static CommonTaikojukuResponse.Pack ToCommonPack(
        Ac15TaikojukuEntry entry,
        IReadOnlyCollection<uint> validSongNoes,
        Ac15ProtocolLimits limits)
    {
        return new CommonTaikojukuResponse.Pack
        {
            GetDan = entry.ChallengeLevel,
            VerupNo = entry.VerupNo,
            Songs = entry.Songs
                .Where(song => validSongNoes.Contains(song.SongNo))
                .Where(song => Ac15Difficulty.IsInRange(song.Level, limits.MinCourseLevel, limits.MaxCourseLevel))
                .Take(limits.MaxSongsPerTaikojukuPack)
                .Select(song => new CommonTaikojukuResponse.Song
                {
                    SongNo = song.SongNo,
                    Level = song.Level
                })
                .ToList()
        };
    }
}
