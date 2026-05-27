using TaikoLocalServer.Application.Catalog.Blue;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetTaikojukuQueryHandler
{
    private const int MaxBlueDanSlots = 25;
    private const int MaxBlueRequestedSlotsPerRequest = 11;
    private const int MaxBlueSongsPerPack = 10;
    private const uint MaxBlueCourseLevel = 4;

    private partial ValueTask<CommonTaikojukuResponse> HandleBlue(
        GetTaikojukuQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Reading Blue Taikojuku packs for {Count} requested dans", request.RequestedDans.Count);
        var blue = gameDataService.Blue();
        var requestedSlots = GetBlueRequestedSlots(request.RequestedDans);
        var validPacksBySlot = blue.TaikojukuFileOrder
            .Where(pack => IsValidBlueDanSlot(pack.ChallengeLevel))
            .GroupBy(pack => pack.ChallengeLevel)
            .ToDictionary(group => group.Key, group => group.First());

        var packs = new List<BlueTaikojukuEntry>();
        foreach (var slot in requestedSlots)
        {
            if (validPacksBySlot.TryGetValue(slot, out var pack))
            {
                packs.Add(pack);
                continue;
            }

            var fallback = CreateBlueFallbackPack(blue, slot, packs.Count);
            if (fallback is not null)
            {
                packs.Add(fallback);
            }
        }

        return ValueTask.FromResult(new CommonTaikojukuResponse
        {
            Result = 1,
            Packs = packs
                .Select(pack => ToCommonBluePack(pack, blue.BlueMusicInfos))
                .Where(pack => pack.Songs.Count > 0)
                .ToList()
        });
    }

    private static bool IsValidBlueDanSlot(uint getDan)
        => getDan is >= 1 and <= MaxBlueDanSlots;

    private static IReadOnlyList<uint> GetBlueRequestedSlots(IReadOnlyList<uint> requestedDans)
    {
        var requestedSlots = requestedDans
            .Where(IsValidBlueDanSlot)
            .Distinct()
            .ToArray();

        if (requestedSlots.Length > 0 || requestedDans.Count == 0)
        {
            return requestedSlots;
        }

        return Enumerable.Range(1, Math.Min(requestedDans.Count, MaxBlueRequestedSlotsPerRequest))
            .Select(slot => (uint)slot)
            .ToArray();
    }

    private static BlueTaikojukuEntry? CreateBlueFallbackPack(
        IBlueCatalog blue,
        uint slot,
        int index)
    {
        var songs = blue.MusicInfoFileOrder
            .Skip(index * 3)
            .Take(3)
            .ToArray();
        if (songs.Length == 0)
        {
            songs = blue.MusicInfoFileOrder.Take(3).ToArray();
        }

        if (songs.Length == 0)
        {
            return null;
        }

        return new BlueTaikojukuEntry
        {
            UniqueId = slot,
            ChallengeLevel = slot,
            Songs = songs.Select(song => new BlueTaikojukuSong
            {
                SongNo = song.SongNo,
                Level = (uint)Math.Min(index, 4),
                MusicId = song.MusicId
            }).ToArray()
        };
    }

    private static CommonTaikojukuResponse.Pack ToCommonBluePack(
        BlueTaikojukuEntry entry,
        IReadOnlyDictionary<uint, BlueMusicInfoEntry> validSongs)
    {
        return new CommonTaikojukuResponse.Pack
        {
            GetDan = entry.ChallengeLevel,
            VerupNo = entry.VerupNo,
            Songs = entry.Songs
                .Where(song => validSongs.ContainsKey(song.SongNo))
                .Where(song => song.Level <= MaxBlueCourseLevel)
                .Take(MaxBlueSongsPerPack)
                .Select(song => new CommonTaikojukuResponse.Song
                {
                    SongNo = song.SongNo,
                    Level = song.Level
                })
                .ToList()
        };
    }
}
