using TaikoLocalServer.Application.Catalog.Green;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetTaikojukuQueryHandler
{
    public partial ValueTask<CommonTaikojukuResponse> Handle(GetTaikojukuQuery request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Reading Green Taikojuku packs for {Count} requested dans", request.RequestedDans.Count);
        var green = gameDataService.Green();
        var requested = request.RequestedDans.ToHashSet();

        var packs = green.TaikojukuFileOrder
            .Where(pack => requested.Count == 0 || requested.Contains(pack.UniqueId))
            .ToList();

        if (packs.Count == 0)
        {
            packs = green.TaikojukuFileOrder
                .Where(pack => requested.Contains(pack.ChallengeLevel))
                .ToList();
        }

        if (packs.Count == 0)
        {
            packs = CreateFallbackPacks(green);
        }

        return ValueTask.FromResult(new CommonTaikojukuResponse
        {
            Result = 1,
            Packs = packs.Select(ToCommonPack).ToList()
        });
    }

    private static List<GreenTaikojukuEntry> CreateFallbackPacks(IGreenCatalog green)
    {
        var songs = green.MusicInfoFileOrder.Take(6).ToArray();
        if (songs.Length == 0)
        {
            return [];
        }

        return
        [
            new GreenTaikojukuEntry
            {
                UniqueId = 1,
                ChallengeLevel = 1,
                Songs = songs.Take(3).Select(song => new GreenTaikojukuSong
                {
                    SongNo = song.SongNo,
                    Level = 0,
                    MusicId = song.MusicId
                }).ToArray()
            },
            new GreenTaikojukuEntry
            {
                UniqueId = 2,
                ChallengeLevel = 2,
                Songs = songs.Skip(3).Take(3).DefaultIfEmpty(songs[0]).Select(song => new GreenTaikojukuSong
                {
                    SongNo = song.SongNo,
                    Level = 1,
                    MusicId = song.MusicId
                }).ToArray()
            }
        ];
    }

    private static CommonTaikojukuResponse.Pack ToCommonPack(GreenTaikojukuEntry entry)
    {
        return new CommonTaikojukuResponse.Pack
        {
            GetDan = entry.UniqueId != 0 ? entry.UniqueId : entry.ChallengeLevel,
            VerupNo = entry.VerupNo,
            Songs = entry.Songs.Select(song => new CommonTaikojukuResponse.Song
            {
                SongNo = song.SongNo,
                Level = song.Level
            }).ToList()
        };
    }
}
