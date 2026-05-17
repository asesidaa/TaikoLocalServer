using System.Collections.Immutable;
using TaikoWebUI.Utilities;

namespace TaikoWebUI.Services;

public class GameDataService : IGameDataService
{
    private readonly HttpClient client;
    private readonly Dictionary<string, ImmutableDictionary<uint, DanData>> danMaps = new();
    private readonly Dictionary<string, Dictionary<uint, MusicDetail>> musicDetailDictionaries = new();
    private readonly Dictionary<string, IReadOnlyList<Costume>> costumeLists = new();
    private readonly Dictionary<string, IReadOnlyDictionary<uint, Title>> titleDictionaries = new();
    private readonly Dictionary<string, IReadOnlyDictionary<uint, Neiro>> neiroDictionaries = new();
    
    private bool lockedCostumesInitialized;
    private bool lockedTitlesInitialized;
    
    private Dictionary<string, List<uint>>? lockedCostumeDataDictionary = new();
    private Dictionary<string, List<uint>>? lockedTitleDataDictionary = new();

    public GameDataService(HttpClient client)
    {
        this.client = client;
    }

    public async Task InitializeAsync(string dataBaseUrl)
    {
        foreach (var era in WebUiEra.Supported)
        {
            var danData = await client.GetFromJsonAsync<List<DanData>>(WebUiEra.Api(era, "GameData/DanData"))
                ?? new List<DanData>();
            danMaps[era] = danData.ToImmutableDictionary(data => data.DanId);
        }
    }
    
    public async Task<Dictionary<uint, MusicDetail>> GetMusicDetailDictionary()
        => await GetMusicDetailDictionary(WebUiEra.Default);

    public async Task<Dictionary<uint, MusicDetail>> GetMusicDetailDictionary(string? era)
    {
        var normalized = WebUiEra.Normalize(era);
        if (!musicDetailDictionaries.TryGetValue(normalized, out var value))
        {
            value = await client.GetFromJsonAsync<Dictionary<uint, MusicDetail>>(WebUiEra.Api(normalized, "GameData/MusicDetails"))
                    ?? new Dictionary<uint, MusicDetail>();
            musicDetailDictionaries[normalized] = value;
        }

        return value;
    }
    
    public async Task<IReadOnlyList<Costume>> GetCostumeList(string? era)
    {
        var normalized = WebUiEra.Normalize(era);
        if (!costumeLists.TryGetValue(normalized, out var value))
        {
            value = await client.GetFromJsonAsync<List<Costume>>(WebUiEra.Api(normalized, "customization/costumes"))
                    ?? new List<Costume>();
            costumeLists[normalized] = value;
        }

        return value;
    }
    
    public async Task<List<Costume>> GetCostumeList()
        => (await GetCostumeList(WebUiEra.Default)).ToList();
    
    public async Task<IReadOnlyDictionary<uint, Title>> GetTitleDictionary(string? era)
    {
        var normalized = WebUiEra.Normalize(era);
        if (!titleDictionaries.TryGetValue(normalized, out var value))
        {
            value = await client.GetFromJsonAsync<Dictionary<uint, Title>>(WebUiEra.Api(normalized, "customization/titles"))
                    ?? new Dictionary<uint, Title>();
            titleDictionaries[normalized] = value;
        }
        
        return value;
    }
    
    public async Task<Dictionary<uint, Title>> GetTitleDictionary()
        => (await GetTitleDictionary(WebUiEra.Default)).ToDictionary(pair => pair.Key, pair => pair.Value);

    public async Task<IReadOnlyDictionary<uint, Neiro>> GetNeiroDictionary(string? era)
    {
        var normalized = WebUiEra.Normalize(era);
        if (!neiroDictionaries.TryGetValue(normalized, out var value))
        {
            value = await client.GetFromJsonAsync<Dictionary<uint, Neiro>>(WebUiEra.Api(normalized, "customization/neiros"))
                    ?? new Dictionary<uint, Neiro>();
            neiroDictionaries[normalized] = value;
        }

        return value;
    }
    
    public async Task<Dictionary<string, List<uint>>> GetLockedCostumeDataDictionary()
    {
        if (!lockedCostumesInitialized)
        {
            await InitializeLockedCostumesAsync();
        }
        
        return lockedCostumeDataDictionary ?? new Dictionary<string, List<uint>>();
    }
    
    public async Task<Dictionary<string, List<uint>>> GetLockedTitleDataDictionary()
    {
        if (!lockedTitlesInitialized)
        {
            await InitializeLockedTitlesAsync();
        }
        
        return lockedTitleDataDictionary ?? new Dictionary<string, List<uint>>();
    }

    public string GetMusicNameBySongId(Dictionary<uint, MusicDetail> musicDetails, uint songId, string? language = "ja")
    {
        return musicDetails.TryGetValue(songId, out var musicDetail) ? language switch
        {
            "ja" => musicDetail.SongName,
            "en-US" => musicDetail.SongNameEN,
            "fr-FR" => musicDetail.SongNameEN,
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
            "fr-FR" => musicDetail.ArtistNameEN,
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

    public ImmutableDictionary<uint, DanData> GetDanMap()
        => GetDanMap(WebUiEra.Default);

    public ImmutableDictionary<uint, DanData> GetDanMap(string? era)
    {
        var normalized = WebUiEra.Normalize(era);
        return danMaps.TryGetValue(normalized, out var value) ? value : ImmutableDictionary<uint, DanData>.Empty;
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

    public string GetHeadTitle(IEnumerable<Costume> costumes, uint index, string? language = "ja")
    {
        return language switch
        {
            "jp" => costumes.FirstOrDefault(costume => costume.CostumeType == "head" && costume.CostumeId == index)?.CostumeName ?? string.Empty,
            "en-US" => costumes.FirstOrDefault(costume => costume.CostumeType == "head" && costume.CostumeId == index)?.CostumeNameEN ?? string.Empty,
            "fr-FR" => costumes.FirstOrDefault(costume => costume.CostumeType == "head" && costume.CostumeId == index)?.CostumeNameEN ?? string.Empty,
            "zh-Hans" => costumes.FirstOrDefault(costume => costume.CostumeType == "head" && costume.CostumeId == index)?.CostumeNameCN ?? string.Empty,
            "zh-Hant" => costumes.FirstOrDefault(costume => costume.CostumeType == "head" && costume.CostumeId == index)?.CostumeNameCN ?? string.Empty,
            "ko" => costumes.FirstOrDefault(costume => costume.CostumeType == "head" && costume.CostumeId == index)?.CostumeNameKO ?? string.Empty,
            _ => costumes.FirstOrDefault(costume => costume.CostumeType == "head" && costume.CostumeId == index)?.CostumeName ?? string.Empty
        };
    }

    public string GetKigurumiTitle(IEnumerable<Costume> costumes, uint index, string? language = "ja")
    {
        return language switch
        {
            "jp" => costumes.FirstOrDefault(costume => costume.CostumeType == "kigurumi" && costume.CostumeId == index)?.CostumeName ?? string.Empty,
            "en-US" => costumes.FirstOrDefault(costume => costume.CostumeType == "kigurumi" && costume.CostumeId == index)?.CostumeNameEN ?? string.Empty,
            "fr-FR" => costumes.FirstOrDefault(costume => costume.CostumeType == "kigurumi" && costume.CostumeId == index)?.CostumeNameEN ?? string.Empty,
            "zh-Hans" => costumes.FirstOrDefault(costume => costume.CostumeType == "kigurumi" && costume.CostumeId == index)?.CostumeNameCN ?? string.Empty,
            "zh-Hant" => costumes.FirstOrDefault(costume => costume.CostumeType == "kigurumi" && costume.CostumeId == index)?.CostumeNameCN ?? string.Empty,
            "ko" => costumes.FirstOrDefault(costume => costume.CostumeType == "kigurumi" && costume.CostumeId == index)?.CostumeNameKO ?? string.Empty,
            _ => costumes.FirstOrDefault(costume => costume.CostumeType == "kigurumi" && costume.CostumeId == index)?.CostumeName ?? string.Empty
        };
    }

    public string GetBodyTitle(IEnumerable<Costume> costumes, uint index, string? language = "ja")
    {
        return language switch
        {
            "jp" => costumes.FirstOrDefault(costume => costume.CostumeType == "body" && costume.CostumeId == index)?.CostumeName ?? string.Empty,
            "en-US" => costumes.FirstOrDefault(costume => costume.CostumeType == "body" && costume.CostumeId == index)?.CostumeNameEN ?? string.Empty,
            "fr-FR" => costumes.FirstOrDefault(costume => costume.CostumeType == "body" && costume.CostumeId == index)?.CostumeNameEN ?? string.Empty,
            "zh-Hans" => costumes.FirstOrDefault(costume => costume.CostumeType == "body" && costume.CostumeId == index)?.CostumeNameCN ?? string.Empty,
            "zh-Hant" => costumes.FirstOrDefault(costume => costume.CostumeType == "body" && costume.CostumeId == index)?.CostumeNameCN ?? string.Empty,
            "ko" => costumes.FirstOrDefault(costume => costume.CostumeType == "body" && costume.CostumeId == index)?.CostumeNameKO ?? string.Empty,
            _ => costumes.FirstOrDefault(costume => costume.CostumeType == "body" && costume.CostumeId == index)?.CostumeName ?? string.Empty
        };
    }

    public string GetFaceTitle(IEnumerable<Costume> costumes, uint index, string? language = "ja")
    {
        return language switch
        {
            "jp" => costumes.FirstOrDefault(costume => costume.CostumeType == "face" && costume.CostumeId == index)?.CostumeName ?? string.Empty,
            "en-US" => costumes.FirstOrDefault(costume => costume.CostumeType == "face" && costume.CostumeId == index)?.CostumeNameEN ?? string.Empty,
            "fr-FR" => costumes.FirstOrDefault(costume => costume.CostumeType == "face" && costume.CostumeId == index)?.CostumeNameEN ?? string.Empty,
            "zh-Hans" => costumes.FirstOrDefault(costume => costume.CostumeType == "face" && costume.CostumeId == index)?.CostumeNameCN ?? string.Empty,
            "zh-Hant" => costumes.FirstOrDefault(costume => costume.CostumeType == "face" && costume.CostumeId == index)?.CostumeNameCN ?? string.Empty,
            "ko" => costumes.FirstOrDefault(costume => costume.CostumeType == "face" && costume.CostumeId == index)?.CostumeNameKO ?? string.Empty,
            _ => costumes.FirstOrDefault(costume => costume.CostumeType == "face" && costume.CostumeId == index)?.CostumeName ?? string.Empty
        };
    }

    public string GetPuchiTitle(IEnumerable<Costume> costumes, uint index, string? language = "ja")
    {
        return language switch
        {
            "jp" => costumes.FirstOrDefault(costume => costume.CostumeType == "puchi" && costume.CostumeId == index)?.CostumeName ?? string.Empty,
            "en-US" => costumes.FirstOrDefault(costume => costume.CostumeType == "puchi" && costume.CostumeId == index)?.CostumeNameEN ?? string.Empty,
            "fr-FR" => costumes.FirstOrDefault(costume => costume.CostumeType == "puchi" && costume.CostumeId == index)?.CostumeNameEN ?? string.Empty,
            "zh-Hans" => costumes.FirstOrDefault(costume => costume.CostumeType == "puchi" && costume.CostumeId == index)?.CostumeNameCN ?? string.Empty,
            "zh-Hant" => costumes.FirstOrDefault(costume => costume.CostumeType == "puchi" && costume.CostumeId == index)?.CostumeNameCN ?? string.Empty,
            "ko" => costumes.FirstOrDefault(costume => costume.CostumeType == "puchi" && costume.CostumeId == index)?.CostumeNameKO ?? string.Empty,
            _ => costumes.FirstOrDefault(costume => costume.CostumeType == "puchi" && costume.CostumeId == index)?.CostumeName ?? string.Empty
        };
    }

    public string GetTitle(IEnumerable<Title> titles, uint index, string? language = "ja")
    {
        return language switch
        {
            "jp" => titles.FirstOrDefault(title => title.TitleId == index)?.TitleName ?? string.Empty,
            "en-US" => titles.FirstOrDefault(title => title.TitleId == index)?.TitleNameEN ?? string.Empty,
            "fr-FR" => titles.FirstOrDefault(title => title.TitleId == index)?.TitleNameEN ?? string.Empty,
            "zh-Hans" => titles.FirstOrDefault(title => title.TitleId == index)?.TitleNameCN ?? string.Empty,
            "zh-Hant" => titles.FirstOrDefault(title => title.TitleId == index)?.TitleNameCN ?? string.Empty,
            "ko" => titles.FirstOrDefault(title => title.TitleId == index)?.TitleNameKO ?? string.Empty,
            _ => titles.FirstOrDefault(title => title.TitleId == index)?.TitleName ?? string.Empty,
        };
    }

    private async Task InitializeLockedCostumesAsync()
    {
        lockedCostumeDataDictionary = await client.GetFromJsonAsync<Dictionary<string, List<uint>>>("api/GameData/LockedCostumes")
                                      ?? new Dictionary<string, List<uint>>();
        lockedCostumesInitialized = true;
    }
    
    private async Task InitializeLockedTitlesAsync()
    {
        lockedTitleDataDictionary = await client.GetFromJsonAsync<Dictionary<string, List<uint>>>("api/GameData/LockedTitles")
                                    ?? new Dictionary<string, List<uint>>();
        lockedTitlesInitialized = true;
    }
}
