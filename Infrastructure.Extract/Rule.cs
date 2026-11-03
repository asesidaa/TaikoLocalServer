namespace TaikoLocalServer.Infrastructure.Extract;

/// <summary>
/// One <c>--inputs</c> rule: a file, or (ending in <c>/</c>) everything under a folder. A <c>*</c> segment matches any one
/// folder name. <see cref="NamesOnly"/>: only the names of the folder's direct children are read.
/// </summary>
public sealed record Rule(string Path, bool NamesOnly = false)
{
    public static Rule Files(string path) => new(path);
    public static Rule Names(string path) => new(path, true);

    /// <summary>The same test as the server's <c>ExtractRunner.Match</c>.</summary>
    public bool Matches(string path)
    {
        string[] r = Path.Split('/'), p = path.Split('/');
        var folder = Path.EndsWith('/');
        if (NamesOnly || !folder ? p.Length != r.Length : p.Length < r.Length) return false;
        for (var i = 0; i < (folder ? r.Length - 1 : r.Length); i++)
            if (r[i] != "*" && r[i] != p[i]) return false;
        return true;
    }
}
