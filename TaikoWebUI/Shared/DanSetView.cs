namespace TaikoWebUI.Shared;

/// <summary>
/// Dani Dojo on a player's pages: the server's dan sets (the default first, then the admin's order) and the player's
/// results in each. Every cabinet runs one set, and a run is kept under it, so each set is shown on its own.
/// </summary>
public sealed record DanSetView(IReadOnlyList<DanSetInfo> Sets, IReadOnlyDictionary<string, List<DanBestData>> Results)
{
    public DanSetInfo Default => Sets[0];

    /// <summary>The picker shows with more than one set; the default's addresses stay the plain ones.</summary>
    public bool Several => Sets.Count > 1;

    /// <summary>A set by its id; none (or one that's gone) is the default.</summary>
    public DanSetInfo Find(string? id) => Sets.FirstOrDefault(s => s.Id == id) ?? Default;

    public List<DanBestData> ResultsOf(DanSetInfo set) => Results.GetValueOrDefault(set.Id) ?? [];

    /// <summary>The set's Dani Dojo page, or one of its ranks.</summary>
    public string Path(long baid, DanSetInfo set, int? danId = null) =>
        PlayDataPaths.Of(baid, PlayDataPaths.DaniDojo) + (set.IsDefault ? "" : $"/{set.Id}") + (danId is { } d ? $"/{d}" : "");

    public static async Task<DanSetView> LoadAsync(HttpClient client, long baid)
    {
        var sets = await client.GetFromJsonAsync<List<DanSetInfo>>("api/GameData/DanSets") ?? [];
        if (sets.Count == 0) sets = [new DanSetInfo("dan_data", "Default", "", true, 0)];
        var ordered = sets.Where(s => s.IsDefault).Concat(sets.Where(s => !s.IsDefault)).ToList();
        var results = await Task.WhenAll(ordered.Select(async s =>
            (s.Id, (await client.GetFromJsonAsync<DanBestDataResponse>($"api/DanBestData/{baid}?set={Uri.EscapeDataString(s.Id)}"))?.DanBestDataList ?? [])));
        return new DanSetView(ordered, results.ToDictionary(r => r.Id, r => r.Item2));
    }
}
