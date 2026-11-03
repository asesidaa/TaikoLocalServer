using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TaikoLocalServer.Infrastructure.Extract;

/// <summary>A note in the manifest: what the dump lacks (<c>gap</c>), what this game doesn't have (<c>absent</c>), or an extra
/// (<c>included</c>).</summary>
public sealed record ManifestNote(string Kind, string Code, string? Detail = null);

/// <summary>
/// What a run did, for the server's Extract panel: <c>&lt;out&gt;/data/extract.json</c> (Nijiiro) or
/// <c>&lt;out&gt;/data/&lt;era&gt;/extract.json</c>, replaced by every run that gets to the end, failed steps or not.
/// </summary>
public static class Manifest
{
    public const string FileName = "extract.json";

    /// <summary>Steps are the reports in printed order, labels without the era (<c>server files</c>, not <c>red server files</c>).
    /// <paramref name="keepOtherSteps"/>: a partial run; the last manifest's steps stay, these reports replacing theirs.</summary>
    public static void Write(string path, string era, string source, IEnumerable<Report> reports, IEnumerable<ManifestNote> notes, DateTime? finishedAt = null,
        bool keepOtherSteps = false)
    {
        var prefix = era + " ";
        var steps = new JsonArray(reports.Select(r => (JsonNode)new JsonObject
        {
            ["label"] = r.Label.StartsWith(prefix, StringComparison.Ordinal) ? r.Label[prefix.Length..] : r.Label,
            ["written"] = r.Written.Count,
            ["upToDate"] = r.UpToDate.Count,
            ["missing"] = r.Missing.Count,
            ["failed"] = r.Failed.Count,
        }).ToArray());
        if (keepOtherSteps && Previous(path) is { } last)
        {
            var fresh = steps.ToDictionary(s => (string)s!["label"]!, s => s!.DeepClone());
            var merged = last.Select(s => fresh.Remove((string)s!["label"]!, out var now) ? now : s!.DeepClone()).ToList();
            steps = new JsonArray([.. merged, .. fresh.Values]);
        }
        var noteArray = new JsonArray(notes.Select(n =>
        {
            var o = new JsonObject { ["kind"] = n.Kind, ["code"] = n.Code };
            if (n.Detail is not null) o["detail"] = n.Detail;
            return (JsonNode)o;
        }).ToArray());
        var manifest = new JsonObject
        {
            ["era"] = era,
            ["finishedAt"] = (finishedAt ?? DateTime.UtcNow).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture),
            ["source"] = source,
            ["steps"] = steps,
            ["notes"] = noteArray,
        };
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, manifest.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    private static JsonArray? Previous(string path)
    {
        try
        {
            return File.Exists(path) ? JsonNode.Parse(File.ReadAllText(path))?["steps"] as JsonArray : null;
        }
        catch (JsonException) { return null; }
    }
}
