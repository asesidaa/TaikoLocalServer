using System.Collections.Immutable;
using Swan.Mapping;
using TaikoWebUI.Shared.Models;

namespace TaikoWebUI.Services;

public class GameDataService : IGameDataService
{
    private readonly HttpClient client;

    private readonly Dictionary<uint, MusicDetail> musicMap = new();

    private ImmutableDictionary<uint, DanData> danMap = null!;

    public GameDataService(HttpClient client)
    {
        this.client = client;
    }

    public async Task InitializeAsync(string dataBaseUrl)
    {
        var musicInfo = await client.GetFromJsonAsync<MusicInfo>($"{dataBaseUrl}/data/musicinfo.json");
        var wordList = await client.GetFromJsonAsync<WordList>($"{dataBaseUrl}/data/wordlist.json");
        var musicOrder = await client.GetFromJsonAsync<MusicOrder>($"{dataBaseUrl}/data/music_order.json");
        var danData = await client.GetFromJsonAsync<List<DanData>>($"{dataBaseUrl}/data/dan_data.json");

        musicInfo.ThrowIfNull();
        wordList.ThrowIfNull();
        musicOrder.ThrowIfNull();
        danData.ThrowIfNull();

        danMap = danData.ToImmutableDictionary(data => data.DanId);
        
        // To prevent duplicate entries in wordlist
        var dict = wordList.WordListEntries.GroupBy(entry => entry.Key)
            .ToImmutableDictionary(group => group.Key, group => group.First());
        foreach (var music in musicInfo.Items)
        {
            var songNameKey = $"song_{music.Id}";
            var songArtistKey = $"song_sub_{music.Id}";
            
            var musicName = dict.GetValueOrDefault(songNameKey, new WordListEntry());
            var musicArtist = dict.GetValueOrDefault(songArtistKey, new WordListEntry());

            var musicSongId = music.SongId;
            var musicDetail = music.CopyPropertiesToNew<MusicDetail>();
            musicDetail.SongName = musicName.JapaneseText;
            musicDetail.ArtistName = musicArtist.JapaneseText;

            musicMap.TryAdd(musicSongId, musicDetail);
        }

        for (var index = 0; index < musicOrder.Order.Count; index++)
        {
            var musicOrderEntry = musicOrder.Order[index];
            var songId = musicOrderEntry.SongId;
            if (musicMap.ContainsKey(songId))
            {
                musicMap[songId].Index = index;
            }
        }
    }

    public string GetMusicNameBySongId(uint songId)
    {
       return musicMap.TryGetValue(songId, out var musicDetail) ? musicDetail.SongName : string.Empty;
    }

    public string GetMusicArtistBySongId(uint songId)
    {
        return musicMap.TryGetValue(songId, out var musicDetail) ? musicDetail.ArtistName : string.Empty;
    }
    public SongGenre GetMusicGenreBySongId(uint songId)
    {
        return musicMap.TryGetValue(songId, out var musicDetail) ? musicDetail.Genre : SongGenre.Variety;
    }

    public int GetMusicIndexBySongId(uint songId)
    {
        return musicMap.TryGetValue(songId, out var musicDetail) ? musicDetail.Index : int.MaxValue;
    }
    public DanData GetDanDataById(uint danId)
    {
        return danMap.GetValueOrDefault(danId, new DanData());
    }

    public int GetMusicStarLevel(uint songId, Difficulty difficulty)
    {
        var success = musicMap.TryGetValue(songId, out var musicDetail);
        return difficulty switch
        {
            Difficulty.None => throw new ArgumentException("Difficulty cannot be none"),
            Difficulty.Easy => success ? musicDetail!.StarEasy : 0,
            Difficulty.Normal => success ? musicDetail!.StarNormal : 0,
            Difficulty.Hard => success ? musicDetail!.StarHard : 0,
            Difficulty.Oni => success ? musicDetail!.StarOni : 0,
            Difficulty.UraOni => success ? musicDetail!.StarUra : 0,
            _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null)
        };
    }
}