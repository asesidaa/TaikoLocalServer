using TaikoLocalServer.Application.Catalog.Green;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetTaikojukuQueryHandler
{
    private const int MaxDanSlots = 25;
    private const int MaxRequestedSlotsPerRequest = 11;
    private const int MaxSongsPerPack = 10;
    private const uint MaxGreenCourseLevel = 4;

    public partial ValueTask<CommonTaikojukuResponse> Handle(GetTaikojukuQuery request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Reading Green Taikojuku packs for {Count} requested dans", request.RequestedDans.Count);
        var green = gameDataService.Green();
        var requestedSlots = GetRequestedSlots(request.RequestedDans);
        var validPacksBySlot = green.TaikojukuFileOrder
            .Where(pack => IsValidDanSlot(pack.ChallengeLevel))
            .GroupBy(pack => pack.ChallengeLevel)
            .ToDictionary(group => group.Key, group => group.First());

        var packs = new List<GreenTaikojukuEntry>();
        foreach (var slot in requestedSlots)
        {
            if (validPacksBySlot.TryGetValue(slot, out var pack))
            {
                packs.Add(pack);
                continue;
            }

            var fallback = CreateFallbackPack(green, slot, packs.Count);
            if (fallback is not null)
            {
                packs.Add(fallback);
            }
        }

        return ValueTask.FromResult(new CommonTaikojukuResponse
        {
            Result = 1,
            Packs = packs
                .Select(pack => ToCommonPack(pack, green.GreenMusicInfos))
                .Where(pack => pack.Songs.Count > 0)
                .ToList()
        });
    }

    private static bool IsValidDanSlot(uint getDan)
        => getDan is >= 1 and <= MaxDanSlots;

    private static IReadOnlyList<uint> GetRequestedSlots(IReadOnlyList<uint> requestedDans)
    {
        var requestedSlots = requestedDans
            .Where(IsValidDanSlot)
            .Distinct()
            .ToArray();

        if (requestedSlots.Length > 0 || requestedDans.Count == 0)
        {
            return requestedSlots;
        }

        return Enumerable.Range(1, Math.Min(requestedDans.Count, MaxRequestedSlotsPerRequest))
            .Select(slot => (uint)slot)
            .ToArray();
    }

    private static GreenTaikojukuEntry? CreateFallbackPack(
        IGreenCatalog green,
        uint slot,
        int index)
    {
        var songs = green.MusicInfoFileOrder
            .Skip(index * 3)
            .Take(3)
            .ToArray();
        if (songs.Length == 0)
        {
            songs = green.MusicInfoFileOrder.Take(3).ToArray();
        }

        if (songs.Length == 0)
        {
            return null;
        }

        return new GreenTaikojukuEntry
        {
            UniqueId = slot,
            ChallengeLevel = slot,
            Songs = songs.Select(song => new GreenTaikojukuSong
            {
                SongNo = song.SongNo,
                Level = (uint)Math.Min(index, 4),
                MusicId = song.MusicId
            }).ToArray()
        };
    }

    private static CommonTaikojukuResponse.Pack ToCommonPack(
        GreenTaikojukuEntry entry,
        IReadOnlyDictionary<uint, GreenMusicInfoEntry> validSongs)
    {
        return new CommonTaikojukuResponse.Pack
        {
            GetDan = entry.ChallengeLevel,
            VerupNo = entry.VerupNo,
            Songs = entry.Songs
                .Where(song => validSongs.ContainsKey(song.SongNo))
                .Where(song => song.Level <= MaxGreenCourseLevel)
                .Take(MaxSongsPerPack)
                .Select(song => new CommonTaikojukuResponse.Song
                {
                    SongNo = song.SongNo,
                    Level = song.Level
                })
                .ToList()
        };
    }
}
