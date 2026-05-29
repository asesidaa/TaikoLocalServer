using System.Text.RegularExpressions;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Ac15;

internal static partial class Ac15CustomizationSourceParser
{
    private const string UnknownCostumeType = "unknown";

    public static IReadOnlyList<Costume> BuildCostumes(string gameDataRoot)
    {
        var scan = Ac15Don3dDirScanner.Scan(gameDataRoot);
        var typeToIds = BuildCostumeTypeIdMap(scan);
        var result = new List<Costume>();

        foreach (var pair in typeToIds)
        {
            foreach (var id in pair.Value)
            {
                result.Add(new Costume
                {
                    CostumeId = id,
                    CostumeType = pair.Key,
                    CostumeName = string.Empty,
                    CostumeNameEN = string.Empty,
                    CostumeNameCN = string.Empty,
                    CostumeNameKO = string.Empty,
                    Source = "don3d"
                });
            }
        }

        return result
            .GroupBy(costume => (costume.CostumeId, costume.CostumeType))
            .Select(group => group.First())
            .OrderBy(costume => costume.CostumeId)
            .ThenBy(costume => CostumeTypeSortKey(costume.CostumeType))
            .ThenBy(costume => costume.CostumeType, StringComparer.Ordinal)
            .ToArray();
    }

    public static IReadOnlyList<Title> BuildTitles(string gameDataRoot)
        => ScanNameIds(gameDataRoot, "title_name")
            .Select(id => new Title
            {
                TitleId = id,
                TitleName = string.Empty,
                TitleNameEN = string.Empty,
                TitleNameCN = string.Empty,
                TitleNameKO = string.Empty,
                TitleRarity = 0,
                Source = "nut"
            })
            .ToArray();

    public static IReadOnlyList<Neiro> BuildNeiros(string gameDataRoot)
        => ScanNameIds(gameDataRoot, "tone_name")
            .Select(id => new Neiro
            {
                NeiroId = id,
                NeiroName = string.Empty,
                NeiroNameEN = string.Empty,
                NeiroNameCN = string.Empty,
                NeiroNameKO = string.Empty,
                Source = "nut"
            })
            .ToArray();

    private static IReadOnlyDictionary<string, IReadOnlyList<uint>> BuildCostumeTypeIdMap(Ac15Don3dScanResult scan)
    {
        var result = DirectoryToCostumeType.Values
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                costumeType => costumeType,
                _ => new SortedSet<uint>(),
                StringComparer.OrdinalIgnoreCase);

        foreach (var pair in scan.DirectoryIds)
        {
            if (!TryResolveCostumeType(pair.Key, out var costumeType))
            {
                continue;
            }

            foreach (var id in pair.Value)
            {
                result[costumeType].Add(id);
            }
        }

        return result.ToDictionary(
            pair => pair.Key,
            pair => (IReadOnlyList<uint>)pair.Value.ToArray(),
            StringComparer.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<uint> ScanNameIds(string gameDataRoot, string catalogDirectoryName)
    {
        var nutdataRoot = Path.Combine(gameDataRoot, "nutdata");
        if (!Directory.Exists(nutdataRoot))
        {
            return [];
        }

        return Directory.EnumerateFiles(nutdataRoot, "*.nut", SearchOption.AllDirectories)
            .Where(path => string.Equals(
                Path.GetFileName(Path.GetDirectoryName(path)),
                catalogDirectoryName,
                StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => ParseIds(Path.GetFileNameWithoutExtension(path)))
            .Distinct()
            .Order()
            .ToArray();
    }

    private static IEnumerable<uint> ParseIds(string fileName)
    {
        var matches = NumberRegex()
            .Matches(fileName)
            .Select(match => uint.Parse(match.Value))
            .ToArray();

        if (matches.Length == 0)
        {
            return [];
        }

        if (matches.Length >= 2)
        {
            var start = matches[^2];
            var end = matches[^1];
            if (start <= end && end - start <= 10000)
            {
                return Enumerable.Range((int)start, checked((int)(end - start + 1)))
                    .Select(id => (uint)id);
            }
        }

        return [matches[^1]];
    }

    private static bool TryResolveCostumeType(string directory, out string costumeType)
    {
        var normalizedDirectory = directory.Replace('\\', '/');
        foreach (var pair in DirectoryToCostumeType)
        {
            if (string.Equals(normalizedDirectory, pair.Key, StringComparison.OrdinalIgnoreCase)
                || normalizedDirectory.StartsWith($"{pair.Key}/", StringComparison.OrdinalIgnoreCase))
            {
                costumeType = pair.Value;
                return true;
            }
        }

        costumeType = string.Empty;
        return false;
    }

    private static int CostumeTypeSortKey(string costumeType)
        => costumeType switch
        {
            "kigurumi" => 0,
            "head" => 1,
            "body" => 2,
            "face" => 3,
            "puchi" => 4,
            UnknownCostumeType => 5,
            _ => 6
        };

    private static readonly IReadOnlyDictionary<string, string> DirectoryToCostumeType =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["don3d/full/cos"] = "kigurumi",
            ["don3d/parts/head"] = "head",
            ["don3d/parts/body"] = "body",
            ["don3d/parts/paint"] = "face",
            ["don3d/parts/acc"] = "puchi"
        };

    [GeneratedRegex(@"\d+")]
    private static partial Regex NumberRegex();
}
