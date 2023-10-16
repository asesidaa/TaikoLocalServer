using System.Collections.Immutable;
using Swan.Mapping;
using TaikoWebUI.Shared.Models;

namespace TaikoWebUI.Services;

public class GameDataService : IGameDataService
{
    private string[] bodyTitles;
    private readonly HttpClient client;
    private string[] faceTitles;

    private string[] headTitles;
    private string[] kigurumiTitles;

    private readonly Dictionary<uint, MusicDetail> musicMap = new();
    private string[] puchiTitles;
    
    private List<int> costumeFlagArraySizes = new();
	
    private int titleFlagArraySize;

    private ImmutableDictionary<uint, DanData> danMap = ImmutableDictionary<uint, DanData>.Empty;

    private ImmutableHashSet<Title> titles = ImmutableHashSet<Title>.Empty;

    public GameDataService(HttpClient client)
    {
        this.client = client;
    }

    public async Task InitializeAsync(string dataBaseUrl)
    {
        dataBaseUrl = dataBaseUrl.TrimEnd('/');
        var musicInfo = await GetData<MusicInfo>(dataBaseUrl, Constants.MUSIC_INFO_BASE_NAME);
        var wordList = await GetData<WordList>(dataBaseUrl, Constants.WORDLIST_BASE_NAME);
        var musicOrder = await GetData<MusicOrder>(dataBaseUrl, Constants.MUSIC_ORDER_BASE_NAME);
        var donCosRewardData = await GetData<DonCosRewards>(dataBaseUrl, Constants.DON_COS_REWARD_BASE_NAME);
        var shougouData = await GetData<Shougous>(dataBaseUrl, Constants.SHOUGOU_BASE_NAME);
        var danData = await client.GetFromJsonAsync<List<DanData>>($"{dataBaseUrl}/data/dan_data.json");

        danData.ThrowIfNull();
        danMap = danData.ToImmutableDictionary(data => data.DanId);

        // To prevent duplicate entries in wordlist
        var dict = wordList.WordListEntries.GroupBy(entry => entry.Key)
            .ToImmutableDictionary(group => group.Key, group => group.First());
        await Task.Run(() => InitializeMusicMap(musicInfo, dict, musicOrder));
        
        InitializeCostumeFlagArraySizes(donCosRewardData);
        InitializeTitleFlagArraySize(shougouData);

        await Task.Run(() => InitializeHeadTitles(dict));
        await Task.Run(() => InitializeFaceTitles(dict));
        await Task.Run(() => InitializeBodyTitles(dict));
        await Task.Run(() => InitializePuchiTitles(dict));
        await Task.Run(() => InitializeKigurumiTitles(dict));
        await Task.Run(() => InitializeTitles(dict));
    }

    private async Task<T> GetData<T>(string dataBaseUrl, string fileBaseName) where T : notnull
    {
        var data = await client.GetFromJsonAsync<T>($"{dataBaseUrl}/data/{fileBaseName}.json");
        data.ThrowIfNull();
        return data;
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

    public string GetHeadTitle(uint index)
    {
        return index < headTitles.Length ? headTitles[index] : string.Empty;
    }

    public string GetKigurumiTitle(uint index)
    {
        return index < kigurumiTitles.Length ? kigurumiTitles[index] : string.Empty;
    }

    public string GetBodyTitle(uint index)
    {
        return index < bodyTitles.Length ? bodyTitles[index] : string.Empty;
    }

    public string GetFaceTitle(uint index)
    {
        return index < faceTitles.Length ? faceTitles[index] : string.Empty;
    }

    public string GetPuchiTitle(uint index)
    {
        return index < puchiTitles.Length ? puchiTitles[index] : string.Empty;
    }

    public ImmutableHashSet<Title> GetTitles()
    {
        return titles;
    }
    
    public List<int> GetCostumeFlagArraySizes()
    {
        return costumeFlagArraySizes;
    }
    
    private void InitializeTitleFlagArraySize(Shougous? shougouData)
    {
        shougouData.ThrowIfNull("Shouldn't happen!");
        titleFlagArraySize = (int)shougouData.ShougouEntries.Max(entry => entry.uniqueId) + 1;
    }

    private void InitializeTitles(ImmutableDictionary<string, WordListEntry> dict)
    {
        var set = ImmutableHashSet.CreateBuilder<Title>();
        for (var i = 1; i < titleFlagArraySize; i++)
        {
            var key = $"syougou_{i}";

            var titleWordlistItem = dict.GetValueOrDefault(key, new WordListEntry());

            set.Add(new Title{
                TitleName = titleWordlistItem.JapaneseText,
                TitleId = i
            });
        }

        titles = set.ToImmutable();
    }
    
    private void InitializeCostumeFlagArraySizes(DonCosRewards? donCosRewardData)
    {
        donCosRewardData.ThrowIfNull("Shouldn't happen!");
        var kigurumiUniqueIdList = donCosRewardData.DonCosRewardEntries
            .Where(entry => entry.cosType == "kigurumi")
            .Select(entry => entry.uniqueId);
        var headUniqueIdList = donCosRewardData.DonCosRewardEntries
            .Where(entry => entry.cosType == "head")
            .Select(entry => entry.uniqueId);
        var bodyUniqueIdList = donCosRewardData.DonCosRewardEntries
            .Where(entry => entry.cosType == "body")
            .Select(entry => entry.uniqueId);
        var faceUniqueIdList = donCosRewardData.DonCosRewardEntries
            .Where(entry => entry.cosType == "face")
            .Select(entry => entry.uniqueId);
        var puchiUniqueIdList = donCosRewardData.DonCosRewardEntries
            .Where(entry => entry.cosType == "puchi")
            .Select(entry => entry.uniqueId);
		
        costumeFlagArraySizes = new List<int>
        {
            (int)kigurumiUniqueIdList.Max() + 1,
            (int)headUniqueIdList.Max() + 1,
            (int)bodyUniqueIdList.Max() + 1,
            (int)faceUniqueIdList.Max() + 1,
            (int)puchiUniqueIdList.Max() + 1
        };
    }
    
    private void InitializeKigurumiTitles(ImmutableDictionary<string, WordListEntry> dict)
    {
        var costumeKigurumiMax = costumeFlagArraySizes[0];
        kigurumiTitles = new string[costumeKigurumiMax];
        for (var i = 0; i < costumeKigurumiMax; i++)
        {
            var key = $"costume_kigurumi_{i}";

            var costumeWordlistItem = dict.GetValueOrDefault(key, new WordListEntry());
            kigurumiTitles[i] = costumeWordlistItem.JapaneseText;
        }
    }
    
    private void InitializeHeadTitles(ImmutableDictionary<string, WordListEntry> dict)
    {
        var costumeHeadMax = costumeFlagArraySizes[1];
        headTitles = new string[costumeHeadMax];
        for (var i = 0; i < costumeHeadMax; i++)
        {
            var key = $"costume_head_{i}";

            var costumeWordlistItem = dict.GetValueOrDefault(key, new WordListEntry());
            headTitles[i] = costumeWordlistItem.JapaneseText;
        }
    }
    
    private void InitializeBodyTitles(ImmutableDictionary<string, WordListEntry> dict)
    {
        var costumeBodyMax = costumeFlagArraySizes[2];
        bodyTitles = new string[costumeBodyMax];
        for (var i = 0; i < costumeBodyMax; i++)
        {
            var key = $"costume_body_{i}";

            var costumeWordlistItem = dict.GetValueOrDefault(key, new WordListEntry());
            bodyTitles[i] = costumeWordlistItem.JapaneseText;
        }
    }
    
    private void InitializeFaceTitles(ImmutableDictionary<string, WordListEntry> dict)
    {
        var costumeFaceMax = costumeFlagArraySizes[3];
        faceTitles = new string[costumeFaceMax];
        for (var i = 0; i < costumeFaceMax; i++)
        {
            var key = $"costume_face_{i}";

            var costumeWordlistItem = dict.GetValueOrDefault(key, new WordListEntry());
            faceTitles[i] = costumeWordlistItem.JapaneseText;
        }
    }

    private void InitializePuchiTitles(ImmutableDictionary<string, WordListEntry> dict)
    {
        var costumePuchiMax = costumeFlagArraySizes[4];
        puchiTitles = new string[costumePuchiMax];
        for (var i = 0; i < costumePuchiMax; i++)
        {
            var key = $"costume_puchi_{i}";

            var costumeWordlistItem = dict.GetValueOrDefault(key, new WordListEntry());
            puchiTitles[i] = costumeWordlistItem.JapaneseText;
        }
    }

    private void InitializeMusicMap(MusicInfo musicInfo, ImmutableDictionary<string, WordListEntry> dict,
        MusicOrder musicOrder)
    {
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
}