using System.IO.Compression;

namespace TaikoLocalServer.Infrastructure.Extract;

/// <summary>
/// An older (PS3) game's <c>USRDIR/data</c> folder, read from disk or straight out of a <c>.zip</c> of the RPCS3 install
/// (ZipArchive reads zip64 and checks each member's CRC). Paths are relative to <c>data</c>, <c>/</c>-separated.
/// </summary>
public sealed class GameSource : IDisposable
{
    private readonly SortedDictionary<string, Entry> files;
    private readonly string? root;
    private readonly ZipArchive? zip;
    private readonly DateTime zipModified;
    private readonly Lock gate = new();

    /// <summary>What the game path resolved to, for the report.</summary>
    public string Label { get; }

    private readonly record struct Entry(long Size, ZipArchiveEntry? Member);

    private GameSource(SortedDictionary<string, Entry> files, string label, string? root, ZipArchive? zip, DateTime zipModified)
    {
        this.files = files;
        Label = label;
        this.root = root;
        this.zip = zip;
        this.zipModified = zipModified;
    }

    /// <summary>
    /// <paramref name="path"/> is a <c>.zip</c>, or a folder: <c>USRDIR/data</c> itself, or one above it (the RPCS3 root,
    /// <c>USRDIR</c>), possibly one folder further up (Blue's <c>[ARCADE SYSTEM 357] … .7z/dev_hdd0/…</c>).
    /// </summary>
    public static GameSource Open(string path)
    {
        if (File.Exists(path))
            return OpenZip(path);
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException($"{path} doesn't exist");
        string[] tops = [path, .. Directory.EnumerateDirectories(path)];
        foreach (var top in tops)
        foreach (var rel in new[] { "", "data", "USRDIR/data", "dev_hdd0/game/SCEEXE000/USRDIR/data" })
        {
            var data = rel.Length == 0 ? top : Path.Combine(top, rel);
            if (!Directory.Exists(Path.Combine(data, "fumen")))
                continue;
            var files = new SortedDictionary<string, Entry>(StringComparer.Ordinal);
            foreach (var file in Directory.EnumerateFiles(data, "*", SearchOption.AllDirectories))
                files[Path.GetRelativePath(data, file).Replace('\\', '/')] = new Entry(new FileInfo(file).Length, null);
            return new GameSource(files, data, data, null, default);
        }
        throw new InvalidDataException($"{path} doesn't look like an older Taiko game: no USRDIR/data folder with fumen in it");
    }

    /// <summary>The data root is the first <c>…/USRDIR/data/</c> in the member names.</summary>
    private static GameSource OpenZip(string path)
    {
        const string marker = "USRDIR/data/";
        var zip = ZipFile.OpenRead(path);
        var names = zip.Entries.Select(e => e.FullName.Replace('\\', '/')).ToList();
        var prefix = names.Select(n => n.IndexOf(marker, StringComparison.Ordinal) is var i and >= 0 ? n[..(i + marker.Length)] : null)
            .FirstOrDefault(p => p is not null);
        if (prefix is null)
        {
            zip.Dispose();
            throw new InvalidDataException($"{path} has no USRDIR/data folder");
        }
        var files = new SortedDictionary<string, Entry>(StringComparer.Ordinal);
        foreach (var e in zip.Entries)
        {
            var name = e.FullName.Replace('\\', '/');
            if (!name.EndsWith('/') && name.StartsWith(prefix, StringComparison.Ordinal))
                files[name[prefix.Length..]] = new Entry(e.Length, e);
        }
        return new GameSource(files, $"{path} ({prefix})", null, zip, File.GetLastWriteTimeUtc(path));
    }

    /// <summary>Files matching <paramref name="keep"/>, in path order (ordinal).</summary>
    public IEnumerable<string> Files(Func<string, bool> keep) => files.Keys.Where(keep);

    public bool Exists(string rel) => files.ContainsKey(rel);

    public long? Size(string rel) => files.TryGetValue(rel, out var e) ? e.Size : null;

    /// <summary>When the file last changed. A zip's members all count as the archive's own time.</summary>
    public DateTime Modified(string rel) => zip is null ? File.GetLastWriteTimeUtc(Path.Combine(root!, rel)) : zipModified;

    public byte[] Read(string rel)
    {
        if (!files.TryGetValue(rel, out var e))
            throw new FileNotFoundException($"{rel} isn't in the game");
        if (e.Member is null)
            return File.ReadAllBytes(Path.Combine(root!, rel));
        lock (gate)
        {
            using var stream = e.Member.Open();
            var data = new byte[e.Size];
            stream.ReadExactly(data);
            // Reading to the end lets ZipArchive check the CRC.
            if (stream.ReadByte() != -1)
                throw new InvalidDataException($"{rel}: longer than the zip says");
            return data;
        }
    }

    /// <summary>True when <paramref name="dst"/> exists and is no older than the game file <paramref name="rel"/>.</summary>
    public bool IsFresh(string dst, string rel) => File.Exists(dst) && File.GetLastWriteTimeUtc(dst) >= Modified(rel);

    public void Dispose() => zip?.Dispose();
}
