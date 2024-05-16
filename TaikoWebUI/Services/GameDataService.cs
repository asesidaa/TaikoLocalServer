using System.Collections.Immutable;
using Swan.Mapping;
using TaikoWebUI.Shared.Models;

namespace TaikoWebUI.Services;

public class GameDataService : IGameDataService
{
    private readonly HttpClient client;
    private readonly Dictionary<uint, MusicDetail> musicMap = new();
    private ImmutableDictionary<uint, DanData> danMap = ImmutableDictionary<uint, DanData>.Empty;
    private ImmutableHashSet<Title> titles = ImmutableHashSet<Title>.Empty;

    private string[] bodyTitles = { };
    private string[] faceTitles = { };
    private string[] headTitles = { };
    private string[] kigurumiTitles = { };
    private string[] puchiTitles = { };
    
    private List<uint> kigurumiUniqueIdList = new();
    private List<uint> headUniqueIdList = new();
    private List<uint> bodyUniqueIdList = new();
    private List<uint> faceUniqueIdList = new();
    private List<uint> puchiUniqueIdList = new();

    private List<uint> titleUniqueIdList = new();
    private List<uint> titlePlateIdList = new();
    
    private List<uint> lockedKigurumiUniqueIdList = new();
    private List<uint> lockedHeadUniqueIdList = new();
    private List<uint> lockedBodyUniqueIdList = new();
    private List<uint> lockedFaceUniqueIdList = new();
    private List<uint> lockedPuchiUniqueIdList = new();
    private List<uint> lockedTitleUniqueIdList = new();
    private List<uint> lockedTitlePlateIdList = new();

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
        var wordlistDict = wordList.WordListEntries.GroupBy(entry => entry.Key)
            .ToImmutableDictionary(group => group.Key, group => group.First());
        await Task.Run(() => InitializeMusicMap(musicInfo, wordlistDict, musicOrder));

        await Task.Run(() => InitializeCostumeIdLists(donCosRewardData));
        await Task.Run(() => InitializeTitleIdList(shougouData));

        await Task.Run(() => InitializeHeadTitles(wordlistDict));
        await Task.Run(() => InitializeFaceTitles(wordlistDict));
        await Task.Run(() => InitializeBodyTitles(wordlistDict));
        await Task.Run(() => InitializePuchiTitles(wordlistDict));
        await Task.Run(() => InitializeKigurumiTitles(wordlistDict));
        await Task.Run(() => InitializeTitles(wordlistDict, shougouData));
        
        var lockedCostumeDataDictionary = await client.GetFromJsonAsync<Dictionary<string, List<uint>>>($"{dataBaseUrl}/data/locked_costume_data.json") ?? throw new InvalidOperationException();
        lockedKigurumiUniqueIdList = lockedCostumeDataDictionary.GetValueOrDefault("Kigurumi") ?? new List<uint>();
        lockedHeadUniqueIdList = lockedCostumeDataDictionary.GetValueOrDefault("Head") ?? new List<uint>();
        lockedBodyUniqueIdList = lockedCostumeDataDictionary.GetValueOrDefault("Body") ?? new List<uint>();
        lockedFaceUniqueIdList = lockedCostumeDataDictionary.GetValueOrDefault("Face") ?? new List<uint>();
        lockedPuchiUniqueIdList = lockedCostumeDataDictionary.GetValueOrDefault("Puchi") ?? new List<uint>();
        
        var lockedTitleDataDictionary = await client.GetFromJsonAsync<Dictionary<string, List<uint>>>($"{dataBaseUrl}/data/locked_title_data.json") ?? throw new InvalidOperationException();
        lockedTitleUniqueIdList = lockedTitleDataDictionary.GetValueOrDefault("TitleNo") ?? new List<uint>();
        lockedTitlePlateIdList = lockedTitleDataDictionary.GetValueOrDefault("TitlePlateNo") ?? new List<uint>();
    }

    private async Task<T> GetData<T>(string dataBaseUrl, string fileBaseName) where T : notnull
    {
        var data = await client.GetFromJsonAsync<T>($"{dataBaseUrl}/data/datatable/{fileBaseName}.json");
        data.ThrowIfNull();
        return data;
    }

    public List<MusicDetail> GetMusicList()
    {
        return musicMap.Values.Where(musicDetail => musicDetail.SongId != 0).ToList();
    }

    public string GetMusicNameBySongId(uint songId, string? language = "ja")
    {
        return musicMap.TryGetValue(songId, out var musicDetail) ? language switch
        {
            "ja" => musicDetail.SongName,
            "en-US" => musicDetail.SongNameEN,
            "zh-Hans" => musicDetail.SongNameCN,
            "zh-Hant" => musicDetail.SongNameCN,
            "ko" => musicDetail.SongNameKO,
            _ => musicDetail.SongName
        } : string.Empty;
    }

    public string GetMusicArtistBySongId(uint songId, string? language = "ja")
    {
        return musicMap.TryGetValue(songId, out var musicDetail) ? language switch
        {
            "jp" => musicDetail.ArtistName,
            "en-US" => musicDetail.ArtistNameEN,
            "zh-Hans" => musicDetail.ArtistNameCN,
            "zh-Hant" => musicDetail.ArtistNameCN,
            "ko" => musicDetail.ArtistNameKO,
            _ => musicDetail.ArtistName
        } : string.Empty;
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
    
    public List<uint> GetKigurumiUniqueIdList()
    {
        return kigurumiUniqueIdList;
    }
    
    public List<uint> GetHeadUniqueIdList()
    {
        return headUniqueIdList;
    }
    
    public List<uint> GetBodyUniqueIdList()
    {
        return bodyUniqueIdList;
    }
    
    public List<uint> GetFaceUniqueIdList()
    {
        return faceUniqueIdList;
    }
    
    public List<uint> GetPuchiUniqueIdList()
    {
        return puchiUniqueIdList;
    }
    
    public List<uint> GetTitleUniqueIdList()
    {
        return titleUniqueIdList;
    }
    
    public List<uint> GetTitlePlateIdList()
    {
        return titlePlateIdList;
    }
    
    private void InitializeTitleIdList(Shougous? shougouData)
    { 
        shougouData.ThrowIfNull("Shouldn't happen!"); 
        titleUniqueIdList = shougouData.ShougouEntries.Select(entry => entry.UniqueId).ToList();
    }

    private void InitializeTitles(ImmutableDictionary<string, WordListEntry> dict, Shougous? shougouData)
    {
        shougouData.ThrowIfNull("Shouldn't happen!");

        var set = ImmutableHashSet.CreateBuilder<Title>();
        foreach (var i in titleUniqueIdList)
        {
            var key = $"syougou_{i}";

            var titleWordlistItem = dict.GetValueOrDefault(key, new WordListEntry());

            var titleRarity = shougouData.ShougouEntries
                .Where(entry => entry.UniqueId == i)
                .Select(entry => entry.Rarity)
                .FirstOrDefault();

            if (!titlePlateIdList.Contains(titleRarity))
            {
                titlePlateIdList.Add(titleRarity);
            }
            
            set.Add(new Title
            {
                TitleName = titleWordlistItem.JapaneseText,
                TitleId = i,
                TitleRarity = titleRarity
            });
        }

        titles = set.ToImmutable();
    }
    
    private void InitializeCostumeIdLists(DonCosRewards? donCosRewardData)
    {
        donCosRewardData.ThrowIfNull("Shouldn't happen!");
        kigurumiUniqueIdList = donCosRewardData.DonCosRewardEntries
            .Where(entry => entry.CosType == "kigurumi")
            .Select(entry => entry.UniqueId).ToList();
        headUniqueIdList = donCosRewardData.DonCosRewardEntries
            .Where(entry => entry.CosType == "head")
            .Select(entry => entry.UniqueId).ToList();
        bodyUniqueIdList = donCosRewardData.DonCosRewardEntries
            .Where(entry => entry.CosType == "body")
            .Select(entry => entry.UniqueId).ToList();
        faceUniqueIdList = donCosRewardData.DonCosRewardEntries
            .Where(entry => entry.CosType == "face")
            .Select(entry => entry.UniqueId).ToList();
        puchiUniqueIdList = donCosRewardData.DonCosRewardEntries
            .Where(entry => entry.CosType == "puchi")
            .Select(entry => entry.UniqueId).ToList();
    }

    private void InitializeKigurumiTitles(ImmutableDictionary<string, WordListEntry> dict)
    {
        kigurumiTitles = new string[kigurumiUniqueIdList.Max() + 1];
        foreach (var i in kigurumiUniqueIdList)
        {
            var key = $"costume_kigurumi_{i}";

            var costumeWordlistItem = dict.GetValueOrDefault(key, new WordListEntry());
            kigurumiTitles[i] = costumeWordlistItem.JapaneseText;
        }
    }

    private void InitializeHeadTitles(ImmutableDictionary<string, WordListEntry> dict)
    {
        headTitles = new string[headUniqueIdList.Max() + 1];
        foreach (var i in headUniqueIdList)
        {
            var key = $"costume_head_{i}";

            var costumeWordlistItem = dict.GetValueOrDefault(key, new WordListEntry());
            headTitles[i] = costumeWordlistItem.JapaneseText;
        }
    }

    private void InitializeBodyTitles(ImmutableDictionary<string, WordListEntry> dict)
    {
        bodyTitles = new string[bodyUniqueIdList.Max() + 1];
        foreach (var i in bodyUniqueIdList)
        {
            var key = $"costume_body_{i}";

            var costumeWordlistItem = dict.GetValueOrDefault(key, new WordListEntry());
            bodyTitles[i] = costumeWordlistItem.JapaneseText;
        }
    }

    private void InitializeFaceTitles(ImmutableDictionary<string, WordListEntry> dict)
    {
        faceTitles = new string[faceUniqueIdList.Max() + 1];
        foreach (var i in faceUniqueIdList)
        {
            var key = $"costume_face_{i}";

            var costumeWordlistItem = dict.GetValueOrDefault(key, new WordListEntry());
            faceTitles[i] = costumeWordlistItem.JapaneseText;
        }
    }

    private void InitializePuchiTitles(ImmutableDictionary<string, WordListEntry> dict)
    {
        puchiTitles = new string[puchiUniqueIdList.Max() + 1];
        foreach (var i in puchiUniqueIdList)
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

            // Add localized names
            musicDetail.SongNameEN = musicName.EnglishUsText;
            musicDetail.ArtistNameEN = musicArtist.EnglishUsText;

            musicDetail.SongNameCN = musicName.ChineseTText;
            musicDetail.ArtistNameCN = musicArtist.ChineseTText;

            musicDetail.SongNameKO = musicName.KoreanText;
            musicDetail.ArtistNameKO = musicArtist.KoreanText;

            musicMap.TryAdd(musicSongId, musicDetail);
        }

        for (var index = 0; index < musicOrder.Order.Count; index++)
        {
            var musicOrderEntry = musicOrder.Order[index];
            var songId = musicOrderEntry.SongId;
            if (musicMap.TryGetValue(songId, out var value))
            {
                value.Index = index;
            }
        }
    }
    
    public List<uint> GetLockedKigurumiUniqueIdList()
    {
        return lockedKigurumiUniqueIdList;
    }
    
    public List<uint> GetLockedHeadUniqueIdList()
    {
        return lockedHeadUniqueIdList;
    }
    
    public List<uint> GetLockedBodyUniqueIdList()
    {
        return lockedBodyUniqueIdList;
    }
    
    public List<uint> GetLockedFaceUniqueIdList()
    {
        return lockedFaceUniqueIdList;
    }
    
    public List<uint> GetLockedPuchiUniqueIdList()
    {
        return lockedPuchiUniqueIdList;
    }
    
    public List<uint> GetLockedTitleUniqueIdList()
    {
        return lockedTitleUniqueIdList;
    }
    
    public List<uint> GetLockedTitlePlateIdList()
    {
        return lockedTitlePlateIdList;
    }
}