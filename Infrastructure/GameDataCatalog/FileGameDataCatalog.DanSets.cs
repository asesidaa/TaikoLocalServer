using System.Collections.Immutable;
using System.Text.Json;
using TaikoLocalServer.Contracts.AdminApi.Responses;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog;

// Dani Dojo course sets (one per year, say). dan_data.json is the built-in first set; the others live in
// data/dan/<id>.json, and data/dan/sets.json names them and says which is the default.
public partial class FileGameDataCatalog
{
    public const string BuiltInDanSet = "dan_data";
    private const string DanSetIndexFile = "dan/sets.json";
    private static readonly JsonSerializerOptions IndexJson = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    private record DanSetName(string Id, string Name);
    private record DanSetIndex(string DefaultSet, List<DanSetName> Sets);

    private DanSetIndex danIndex = new(BuiltInDanSet, [new(BuiltInDanSet, "Default")]);
    private ImmutableDictionary<string, ImmutableDictionary<uint, DanData>> danSets =
        ImmutableDictionary<string, ImmutableDictionary<uint, DanData>>.Empty;

    private static string DanSetFile(string id) => id == BuiltInDanSet ? DataFiles.Dan : $"dan/{id}.json";

    private static string? DanSetId(string fileName) =>
        fileName.StartsWith("dan/") && fileName.EndsWith(".json") && fileName != DanSetIndexFile ? fileName[4..^5] : null;

    private Func<string, Action> DanSetParser(string id) => json =>
    {
        var courses = Parse<List<DanData>>(json);
        var dupe = courses.GroupBy(c => c.DanId).FirstOrDefault(g => g.Count() > 1);
        if (dupe is not null) throw new InvalidDataException($"Dan ID {dupe.Key} is listed twice.");
        var d = courses.ToImmutableDictionary(c => c.DanId);
        return () => danSets = danSets.SetItem(id, d);
    };

    public IReadOnlyList<DanSetInfo> GetDanSets() =>
        danIndex.Sets.Select(x => new DanSetInfo(x.Id, x.Name, DanSetFile(x.Id), x.Id == danIndex.DefaultSet,
            danSets.GetValueOrDefault(x.Id)?.Count ?? 0)).ToList();

    private async Task LoadDanSetsAsync(List<GameDataFileStatus> status)
    {
        var dataPath = PathHelper.GetDataPath();
        var indexPath = Path.Combine(dataPath, DanSetIndexFile);
        var index = new DanSetIndex(BuiltInDanSet, [new(BuiltInDanSet, "Default")]);
        if (File.Exists(indexPath))
        {
            try
            {
                var read = JsonSerializer.Deserialize<DanSetIndex>(await File.ReadAllTextAsync(indexPath), IndexJson)
                           ?? throw new InvalidDataException("The file is empty (null).");
                // The built-in set is always there; ids are file names, so nothing that could leave data/dan.
                var sets = read.Sets.Where(x => x.Id == BuiltInDanSet || IsSafeId(x.Id)).DistinctBy(x => x.Id).ToList();
                if (sets.All(x => x.Id != BuiltInDanSet)) sets.Insert(0, new(BuiltInDanSet, "Default"));
                index = new(sets.Any(x => x.Id == read.DefaultSet) ? read.DefaultSet : BuiltInDanSet, sets);
                status.Add(new GameDataFileStatus(DanSetIndexFile, false, false, null, File.GetLastWriteTimeUtc(indexPath)));
            }
            catch (Exception e) when (e is JsonException or InvalidDataException or IOException)
            {
                status.Add(new GameDataFileStatus(DanSetIndexFile, false, false, e.Message, File.GetLastWriteTimeUtc(indexPath)));
                index = danIndex;
            }
        }
        danIndex = index;

        danSets = danSets.Where(p => index.Sets.Any(x => x.Id == p.Key)).ToImmutableDictionary()
            .SetItem(BuiltInDanSet, commonDanDataDictionary);
        foreach (var set in index.Sets.Where(x => x.Id != BuiltInDanSet))
        {
            var file = DanSetFile(set.Id);
            var path = Path.Combine(dataPath, file);
            if (!File.Exists(path))
            {
                status.Add(new GameDataFileStatus(file, true, true, null, null));
                continue;
            }
            try
            {
                DanSetParser(set.Id)(await File.ReadAllTextAsync(path))();
                status.Add(new GameDataFileStatus(file, true, false, null, File.GetLastWriteTimeUtc(path)));
            }
            catch (Exception e) when (e is JsonException or InvalidDataException or IOException)
            {
                status.Add(new GameDataFileStatus(file, true, false, e.Message, File.GetLastWriteTimeUtc(path)));
            }
        }
    }

    private static bool IsSafeId(string id) => id.Length is > 0 and <= 40 && id.All(c => char.IsAsciiLetterOrDigit(c) || c == '-');

    private async Task WriteDanIndexAsync(DanSetIndex index)
    {
        await WriteAtomicAsync(Path.Combine(PathHelper.GetDataPath(), DanSetIndexFile), JsonSerializer.Serialize(index, IndexJson));
        await ReloadAsync();
    }

    public async Task<string> CreateDanSetAsync(string name, string copyFrom)
    {
        var source = danIndex.Sets.Any(x => x.Id == copyFrom) ? copyFrom : danIndex.DefaultSet;
        var n = 1;
        while (danIndex.Sets.Any(x => x.Id == $"set-{n}") || File.Exists(Path.Combine(PathHelper.GetDataPath(), DanSetFile($"set-{n}")))) n++;
        var id = $"set-{n}";
        var json = await File.ReadAllTextAsync(Path.Combine(PathHelper.GetDataPath(), DanSetFile(source)));
        await WriteAtomicAsync(Path.Combine(PathHelper.GetDataPath(), DanSetFile(id)), json);
        await WriteDanIndexAsync(danIndex with { Sets = [.. danIndex.Sets, new(id, name.Trim())] });
        return id;
    }

    public Task<bool> RenameDanSetAsync(string id, string name) => UpdateIndex(id,
        i => i with { Sets = i.Sets.Select(x => x.Id == id ? x with { Name = name.Trim() } : x).ToList() });

    public Task<bool> SetDefaultDanSetAsync(string id) => UpdateIndex(id, i => i with { DefaultSet = id });

    /// <summary>Not the built-in set or the default one. The file is kept as .bak.</summary>
    public async Task<bool> DeleteDanSetAsync(string id)
    {
        if (id == BuiltInDanSet || id == danIndex.DefaultSet || danIndex.Sets.All(x => x.Id != id)) return false;
        var path = Path.Combine(PathHelper.GetDataPath(), DanSetFile(id));
        if (File.Exists(path)) File.Move(path, path + ".bak", overwrite: true);
        await WriteDanIndexAsync(danIndex with { Sets = danIndex.Sets.Where(x => x.Id != id).ToList() });
        return true;
    }

    private async Task<bool> UpdateIndex(string id, Func<DanSetIndex, DanSetIndex> change)
    {
        if (danIndex.Sets.All(x => x.Id != id)) return false;
        await WriteDanIndexAsync(change(danIndex));
        return true;
    }
}
