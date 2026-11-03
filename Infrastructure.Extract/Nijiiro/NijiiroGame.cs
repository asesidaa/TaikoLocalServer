namespace TaikoLocalServer.Infrastructure.Extract.Nijiiro;

/// <summary>
/// The game's files as the game sees them. With mods, a LayeredFS <c>Data_mods</c> folder is layered over <c>Data</c>: a
/// file in <c>Data_mods/x64/&lt;path&gt;</c> replaces <c>Data/x64/&lt;path&gt;</c>, and folders list both layers' entries.
/// Modded datatables live re-encrypted in <c>Data_mods/x64_enc/datatable</c> (the same IV-prefixed format the server
/// decrypts); <c>Data_mods/x64/datatable</c> holds the modding tools' decrypted JSON and is ignored.
/// </summary>
public sealed class NijiiroGame
{
    /// <summary>Top layer first: <c>Data_mods/x64</c> (with mods), then <c>Data/x64</c>.</summary>
    private readonly List<string> layers;
    private readonly string? modDatatables;

    /// <param name="x64"><c>&lt;game&gt;/Data/x64</c>; with <paramref name="mods"/>, <c>&lt;game&gt;/Data_mods</c> is layered on top.</param>
    public NijiiroGame(string x64, bool mods)
    {
        layers = [x64];
        if (!mods) return;
        var root = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetFullPath(x64))) is { } g ? Path.Combine(g, "Data_mods") : null;
        if (root is null || !Directory.Exists(root))
            throw new IOException($"--mods: no Data_mods folder next to {Path.GetDirectoryName(x64)}");
        layers.Insert(0, Path.Combine(root, "x64"));
        modDatatables = Path.Combine(root, "x64_enc", "datatable");
    }

    public string Base => layers[^1];
    public bool Modded => layers.Count > 1;

    /// <summary>The file at <paramref name="rel"/> from the top layer that has it; the base path when none do (so reports
    /// name it).</summary>
    public string File(string rel) =>
        layers.Select(l => Path.Combine(l, rel)).FirstOrDefault(p => System.IO.File.Exists(p) || Directory.Exists(p)) ?? Path.Combine(Base, rel);

    /// <summary>A datatable <c>.bin</c>: the modded, re-encrypted one when there is one.</summary>
    public string Datatable(string name) =>
        modDatatables is not null && System.IO.File.Exists(Path.Combine(modDatatables, name)) ? Path.Combine(modDatatables, name) : Path.Combine(Base, "datatable", name);

    /// <summary>The entries of folder <paramref name="rel"/> across layers, by name with the top layer winning, sorted by
    /// name; null when no layer has the folder.</summary>
    public List<string>? List(string rel)
    {
        var found = false;
        var entries = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var dir in layers.Select(l => Path.Combine(l, rel)).Where(Directory.Exists))
        {
            found = true;
            foreach (var e in Directory.EnumerateFileSystemEntries(dir))
                entries.TryAdd(Path.GetFileName(e), e);
        }
        return found ? [.. entries.Values] : null;
    }

    /// <summary>True when <paramref name="dst"/> exists and is no older than <paramref name="src"/>.</summary>
    public static bool IsNewer(string dst, string src) =>
        System.IO.File.Exists(dst) && System.IO.File.GetLastWriteTimeUtc(dst) >= System.IO.File.GetLastWriteTimeUtc(src);
}
