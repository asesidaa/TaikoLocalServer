namespace TaikoWebUI.Services;

/// <summary>
/// The game data the pages show (songs, costumes, titles), fetched once on first use: api/GameData needs a
/// logged-in user, so it can't load at startup. Names come in the viewer's language.
/// </summary>
public class GameDataService(HttpClient client)
{
    private Dictionary<uint, MusicDetail>? musicDetails;
    private List<Costume>? costumes;
    private Dictionary<string, List<uint>>? lockedCostumes;
    private Dictionary<uint, Title>? titles;
    private Dictionary<string, List<uint>>? lockedTitles;

    public async Task<Dictionary<uint, MusicDetail>> GetMusicDetailDictionary() =>
        musicDetails ??= await client.GetFromJsonAsync<Dictionary<uint, MusicDetail>>("api/GameData/MusicDetails") ?? [];

    public async Task<List<Costume>> GetCostumeList()
    {
        await LoadCostumes();
        return costumes!;
    }

    public async Task<Dictionary<string, List<uint>>> GetLockedCostumeDataDictionary()
    {
        await LoadCostumes();
        return lockedCostumes!;
    }

    public async Task<Dictionary<uint, Title>> GetTitleDictionary()
    {
        await LoadTitles();
        return titles!;
    }

    public async Task<Dictionary<string, List<uint>>> GetLockedTitleDataDictionary()
    {
        await LoadTitles();
        return lockedTitles!;
    }

    public string GetMusicNameBySongId(Dictionary<uint, MusicDetail> musicDetails, uint songId, string? language) =>
        musicDetails.TryGetValue(songId, out var m) ? Pick(language, m.SongName, m.SongNameEN, m.SongNameCN, m.SongNameKO) : string.Empty;

    public SongGenre GetMusicGenreBySongId(Dictionary<uint, MusicDetail> musicDetails, uint songId) =>
        musicDetails.TryGetValue(songId, out var m) ? m.Genre : SongGenre.Variety;

    public int GetMusicStarLevel(Dictionary<uint, MusicDetail> musicDetails, uint songId, Difficulty difficulty)
    {
        if (!musicDetails.TryGetValue(songId, out var m)) return 0;
        return difficulty switch
        {
            Difficulty.Easy => m.StarEasy, Difficulty.Normal => m.StarNormal, Difficulty.Hard => m.StarHard,
            Difficulty.Oni => m.StarOni, Difficulty.UraOni => m.StarUra,
            _ => throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null),
        };
    }

    /// <summary>A costume part's name: <paramref name="type"/> is kigurumi, head, body, face or puchi.</summary>
    public string GetCostumeName(IEnumerable<Costume> costumes, string type, uint id, string? language) =>
        costumes.FirstOrDefault(c => c.CostumeType == type && c.CostumeId == id) is { } c
            ? Pick(language, c.CostumeName, c.CostumeNameEN, c.CostumeNameCN, c.CostumeNameKO)
            : string.Empty;

    public string GetTitle(IEnumerable<Title> titles, uint id, string? language) =>
        titles.FirstOrDefault(t => t.TitleId == id) is { } t ? Pick(language, t.TitleName, t.TitleNameEN, t.TitleNameCN, t.TitleNameKO) : string.Empty;

    // The WebUI's languages (appsettings SupportedLanguages); Japanese is the default and the fallback.
    private static string Pick(string? language, string ja, string en, string cn, string ko) => language switch
    {
        "en-US" or "fr-FR" => en,
        "zh-Hans" or "zh-Hant" => cn,
        "ko" => ko,
        _ => ja,
    };

    private async Task LoadCostumes()
    {
        if (costumes is not null) return;
        lockedCostumes = await client.GetFromJsonAsync<Dictionary<string, List<uint>>>("api/GameData/LockedCostumes") ?? [];
        costumes = await client.GetFromJsonAsync<List<Costume>>("api/GameData/Costumes") ?? [];
    }

    private async Task LoadTitles()
    {
        if (titles is not null) return;
        lockedTitles = await client.GetFromJsonAsync<Dictionary<string, List<uint>>>("api/GameData/LockedTitles") ?? [];
        titles = await client.GetFromJsonAsync<Dictionary<uint, Title>>("api/GameData/Titles") ?? [];
    }
}
