using System.Collections.Immutable;
using System.Net.Http.Json;
using TaikoWebUI.Shared.Models;
using Throw;

namespace TaikoWebUI.Services;

public class GameDataService : IGameDataService
{
    private readonly HttpClient client;

    private readonly Dictionary<uint, string> musicNameMap = new();

    private readonly Dictionary<uint, string> musicArtistMap = new();

    public GameDataService(HttpClient client)
    {
        this.client = client;
    }

    public async Task InitializeAsync(string dataBaseUrl)
    {
        var musicInfo = await client.GetFromJsonAsync<MusicInfo>($"{dataBaseUrl}/data/musicinfo");
        var wordList = await client.GetFromJsonAsync<WordList>($"{dataBaseUrl}/data/wordlist");

        musicInfo.ThrowIfNull();
        wordList.ThrowIfNull();
        
        var dict = wordList.WordListEntries.GroupBy(entry => entry.Key)
            .ToImmutableDictionary(group => group.Key, group => group.First());
        foreach (var music in musicInfo.Items)
        {
            var songNameKey = $"song_{music.Id}";
            var songArtistKey = $"song_sub_{music.Id}";
            
            var musicName = dict.GetValueOrDefault(songNameKey, new WordListEntry());
            var musicArtist = dict.GetValueOrDefault(songArtistKey, new WordListEntry());
            
            musicNameMap.TryAdd(music.SongId, musicName.JapaneseText);
            musicArtistMap.TryAdd(music.SongId, musicArtist.JapaneseText);
        }
    }

    public string GetMusicNameBySongId(uint songId)
    {
        return musicNameMap.GetValueOrDefault(songId, string.Empty);
    }

    public string GetMusicArtistBySongId(uint songId)
    {
        return musicArtistMap.GetValueOrDefault(songId, string.Empty);
    }
}