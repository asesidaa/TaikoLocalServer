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
    private CostumeNames? costumeNames;

    // wwwroot/i18n/costume-names.json: the game ships costume names in Japanese only, so the WebUI keeps its own
    // translations, keyed by the Japanese name, one column per UI language.
    private sealed record CostumeNames(string[] Languages, Dictionary<string, string[]> Names);

    public async Task<Dictionary<uint, MusicDetail>> GetMusicDetailDictionary() =>
        musicDetails ??= await client.GetFromJsonAsync<Dictionary<uint, MusicDetail>>("api/GameData/MusicDetails") ?? [];

    private readonly Dictionary<string, Dictionary<uint, MusicDetail>> eraMusic = [];

    /// <summary>One game's songs, from its own catalogue (Nijiiro's is <see cref="GetMusicDetailDictionary()"/>).</summary>
    public async Task<Dictionary<uint, MusicDetail>> GetMusicDetailDictionary(string game)
    {
        if (game == "Nijiiro") return await GetMusicDetailDictionary();
        if (!eraMusic.TryGetValue(game, out var songs))
            eraMusic[game] = songs = await client.GetFromJsonAsync<Dictionary<uint, MusicDetail>>($"api/{game}/GameData/MusicDetails") ?? [];
        return songs;
    }

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

    /// <summary>The language song titles show in for whoever is looking (their Settings › Display › Song titles, else
    /// their profile's Language), loaded by the layout; 0 (Japanese) until then.</summary>
    public uint SongLanguage { get; private set; }

    public async Task LoadSongLanguageAsync(uint baid)
    {
        if (baid == 0) return;
        try { SongLanguage = await client.GetFromJsonAsync<uint>($"api/Users/{baid}/SongLanguage"); }
        catch (Exception e) when (e is HttpRequestException or System.Text.Json.JsonException) { }
    }

    /// <summary>The game's title plates (extract's nameplate/plates.json) into GameData.TitlePlates, once; the fixed
    /// list stays when it isn't extracted yet.</summary>
    public async Task LoadTitlePlatesAsync()
    {
        if (platesLoaded) return;
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(await client.GetStringAsync("api/Assets/nameplate/plates.json"));
            if (doc.RootElement.TryGetProperty("plates", out var plates) && plates.TryGetProperty("labels", out var labels))
                GameData.UsePlateLabels(labels.EnumerateArray().Select(l => l.GetString() ?? "").ToList());
            platesLoaded = true;
        }
        catch (Exception e) when (e is HttpRequestException or System.Text.Json.JsonException) { }
    }

    private bool platesLoaded;

    public string SongTitle(MusicDetail song) => song.Title(SongLanguage);

    public string SongSubtitle(MusicDetail song) => song.Subtitle(SongLanguage);

    public string GetMusicNameBySongId(Dictionary<uint, MusicDetail> musicDetails, uint songId) =>
        musicDetails.TryGetValue(songId, out var m) ? SongTitle(m) : string.Empty;

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

    /// <summary>
    /// A costume part's name (<paramref name="type"/>: kigurumi, head, body, face or puchi): the game's name in <paramref name="language"/> when it has one, else the WebUI dictionary's, else Japanese
    /// (a costume newer than the dictionary).
    /// </summary>
    public string GetCostumeName(IEnumerable<Costume> costumes, string type, uint id, string? language)
    {
        if (costumes.FirstOrDefault(c => c.CostumeType == type && c.CostumeId == id) is not { } c) return string.Empty;
        var game = Pick(language, c.CostumeName, c.CostumeNameEN, c.CostumeNameCN, c.CostumeNameKO);
        if (game != c.CostumeName || costumeNames is null) return game;
        var column = Array.IndexOf(costumeNames.Languages, language);
        return column >= 0 && costumeNames.Names.TryGetValue(c.CostumeName, out var row) && column < row.Length && row[column].Length > 0
            ? row[column]
            : game;
    }

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
        try { costumeNames = await client.GetFromJsonAsync<CostumeNames>("i18n/costume-names.json"); }
        catch (Exception e) when (e is HttpRequestException or System.Text.Json.JsonException) { /* Japanese names, as the game has them */ }
        costumes = await client.GetFromJsonAsync<List<Costume>>("api/GameData/Costumes") ?? [];
    }

    private async Task LoadTitles()
    {
        if (titles is not null) return;
        lockedTitles = await client.GetFromJsonAsync<Dictionary<string, List<uint>>>("api/GameData/LockedTitles") ?? [];
        titles = await client.GetFromJsonAsync<Dictionary<uint, Title>>("api/GameData/Titles") ?? [];
    }
}
