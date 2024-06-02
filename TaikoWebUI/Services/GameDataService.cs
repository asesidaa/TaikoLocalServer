using System.Collections.Immutable;

namespace TaikoWebUI.Services;

public class GameDataService : IGameDataService
{
    private readonly HttpClient client;
    private ImmutableDictionary<uint, DanData> danMap = ImmutableDictionary<uint, DanData>.Empty;
    private Dictionary<uint, MusicDetail>? musicDetailDictionary = new();
    private List<Costume>? costumeList;
    private Dictionary<uint,Title>? titleDictionary = new();
    
    private bool musicDetailInitialized;
    private bool costumesInitialized;
    private bool titlesInitialized;
    
    private Dictionary<string, List<uint>>? lockedCostumeDataDictionary = new();
    private Dictionary<string, List<uint>>? lockedTitleDataDictionary = new();

    public GameDataService(HttpClient client)
    {
        this.client = client;
    }

    public async Task InitializeAsync(string dataBaseUrl)
    {
        dataBaseUrl = dataBaseUrl.TrimEnd('/');
        var danData = await client.GetFromJsonAsync<List<DanData>>($"{dataBaseUrl}/data/dan_data.json");
        danData.ThrowIfNull();
        danMap = danData.ToImmutableDictionary(data => data.DanId);
    }
    
    public async Task<Dictionary<uint, MusicDetail>> GetMusicDetailDictionary()
    {
        if (!musicDetailInitialized)
        {
            await InitializeMusicDetailAsync();
        }

        return musicDetailDictionary ?? new Dictionary<uint, MusicDetail>();
    }
    
    public async Task<List<Costume>> GetCostumeList()
    {
        if (!costumesInitialized)
        {
            await InitializeCostumesAsync();
        }

        return costumeList ?? new List<Costume>();
    }
    
    public async Task<Dictionary<uint, Title>> GetTitleDictionary()
    {
        if (!titlesInitialized)
        {
            await InitializeTitlesAsync();
        }
        
        return titleDictionary ?? new Dictionary<uint, Title>();
    }
    
    public async Task<Dictionary<string, List<uint>>> GetLockedCostumeDataDictionary()
    {
        if (!costumesInitialized)
        {
            await InitializeCostumesAsync();
        }
        
        return lockedCostumeDataDictionary ?? new Dictionary<string, List<uint>>();
    }
    
    public async Task<Dictionary<string, List<uint>>> GetLockedTitleDataDictionary()
    {
        if (!titlesInitialized)
        {
            await InitializeTitlesAsync();
        }
        
        return lockedTitleDataDictionary ?? new Dictionary<string, List<uint>>();
    }

    public string GetMusicNameBySongId(Dictionary<uint, MusicDetail> musicDetails, uint songId, string? language = "ja")
    {
        return musicDetails.TryGetValue(songId, out var musicDetail) ? language switch
        {
            "ja" => musicDetail.SongName,
            "en-US" => musicDetail.SongNameEN,
            "zh-Hans" => musicDetail.SongNameCN,
            "zh-Hant" => musicDetail.SongNameCN,
            "ko" => musicDetail.SongNameKO,
            _ => musicDetail.SongName
        } : string.Empty;
    }

    public string GetMusicArtistBySongId(Dictionary<uint, MusicDetail> musicDetails, uint songId, string? language = "ja")
    {
        return musicDetails.TryGetValue(songId, out var musicDetail) ? language switch
        {
            "jp" => musicDetail.ArtistName,
            "en-US" => musicDetail.ArtistNameEN,
            "zh-Hans" => musicDetail.ArtistNameCN,
            "zh-Hant" => musicDetail.ArtistNameCN,
            "ko" => musicDetail.ArtistNameKO,
            _ => musicDetail.ArtistName
        } : string.Empty;
    }

    public SongGenre GetMusicGenreBySongId(Dictionary<uint, MusicDetail> musicDetails, uint songId)
    {
        return musicDetails.TryGetValue(songId, out var musicDetail) ? musicDetail.Genre : SongGenre.Variety;
    }

    public int GetMusicIndexBySongId(Dictionary<uint, MusicDetail> musicDetails, uint songId)
    {
        return musicDetails.TryGetValue(songId, out var musicDetail) ? musicDetail.Index : int.MaxValue;
    }

    public DanData GetDanDataById(uint danId)
    {
        return danMap.GetValueOrDefault(danId, new DanData());
    }

    public int GetMusicStarLevel(Dictionary<uint, MusicDetail> musicDetails, uint songId, Difficulty difficulty)
    {
        var success = musicDetails.TryGetValue(songId, out var musicDetail);
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

    public string GetHeadTitle(IEnumerable<Costume> costumes, uint index)
    {
        return costumes.FirstOrDefault(costume => costume.CostumeType == "head" && costume.CostumeId == index)?.CostumeName ?? string.Empty;
    }

    public string GetKigurumiTitle(IEnumerable<Costume> costumes, uint index)
    {
        return costumes.FirstOrDefault(costume => costume.CostumeType == "kigurumi" && costume.CostumeId == index)?.CostumeName ?? string.Empty;
    }

    public string GetBodyTitle(IEnumerable<Costume> costumes, uint index)
    {
        return costumes.FirstOrDefault(costume => costume.CostumeType == "body" && costume.CostumeId == index)?.CostumeName ?? string.Empty;
    }

    public string GetFaceTitle(IEnumerable<Costume> costumes, uint index)
    {
        return costumes.FirstOrDefault(costume => costume.CostumeType == "face" && costume.CostumeId == index)?.CostumeName ?? string.Empty;
    }

    public string GetPuchiTitle(IEnumerable<Costume> costumes, uint index)
    {
        return costumes.FirstOrDefault(costume => costume.CostumeType == "puchi" && costume.CostumeId == index)?.CostumeName ?? string.Empty;
    }
    
    private async Task InitializeMusicDetailAsync()
    {
        musicDetailDictionary = await client.GetFromJsonAsync<Dictionary<uint, MusicDetail>>("api/GameData/MusicDetails");
        musicDetailInitialized = true;
    }
    
    private async Task InitializeCostumesAsync()
    {
        costumeList = await client.GetFromJsonAsync<List<Costume>>("api/GameData/Costumes");
        lockedCostumeDataDictionary = await client.GetFromJsonAsync<Dictionary<string, List<uint>>>("api/GameData/LockedCostumes");
        costumesInitialized = true;
    }
    
    private async Task InitializeTitlesAsync()
    {
        titleDictionary = await client.GetFromJsonAsync<Dictionary<uint, Title>>("api/GameData/Titles");
        lockedTitleDataDictionary = await client.GetFromJsonAsync<Dictionary<string, List<uint>>>("api/GameData/LockedTitles");
        titlesInitialized = true;
    }
}