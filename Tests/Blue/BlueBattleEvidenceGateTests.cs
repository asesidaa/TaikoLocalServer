namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueBattleEvidenceGateTests
{
    private static readonly string[] BlockingStatusMarkers =
    [
        "STILL_MISSING",
        "NEEDS_USER_APPROVAL",
        "DEFER_RUNTIME_USE",
    ];

    [Fact]
    public void ResolutionMatrix_CopiesAllPhase4MissingEvidenceRows()
    {
        var phase4Rows = ReadTable(
            FindRepoFile(".planning", "phases", "04-blue-battle-evidence-and-design", "04-03-BLUE-BATTLE-DESIGN-GATE.md"),
            "Missing Evidence Item");
        var resolutionRows = ReadResolutionRows();

        var phase4Items = phase4Rows.Select(row => row["Missing Evidence Item"]).ToArray();
        var resolutionItems = resolutionRows.Select(row => row["Phase 4 Missing-Evidence Row"]).ToArray();

        Assert.Equal(26, resolutionRows.Count);
        Assert.Equal(phase4Items, resolutionItems);
    }

    [Fact]
    public void ResolutionMatrix_HasPhase5GateColumns()
    {
        var rows = ReadResolutionRows();

        foreach (var row in rows)
        {
            Assert.Contains("Phase 5 Status", row.Keys);
            Assert.Contains("Evidence Source", row.Keys);
            Assert.Contains("Runtime Use", row.Keys);
            Assert.Contains("Next Gate", row.Keys);
            Assert.False(string.IsNullOrWhiteSpace(row["Phase 5 Status"]));
            Assert.False(string.IsNullOrWhiteSpace(row["Evidence Source"]));
            Assert.False(string.IsNullOrWhiteSpace(row["Runtime Use"]));
            Assert.False(string.IsNullOrWhiteSpace(row["Next Gate"]));
        }
    }

    [Fact]
    public void ResolutionMatrix_BlocksUnresolvedRowsFromRuntimeUse()
    {
        var rows = ReadResolutionRows();

        foreach (var row in rows)
        {
            var status = row["Phase 5 Status"];
            if (BlockingStatusMarkers.Any(marker => status.Contains(marker, StringComparison.Ordinal)))
            {
                Assert.Equal("blocked", row["Runtime Use"]);
            }
        }
    }

    [Fact]
    public void ResolutionMatrix_DoesNotContainBlanketApprovals()
    {
        var source = File.ReadAllText(FindRepoFile(".planning", "phases", "05-blue-battle-runtime-support", "05-RESOLUTION.md"));
        var forbiddenApprovalPatterns = new[]
        {
            "APPROVED_ALL",
            "ALL_APPROVED",
            "APPROVED_BY_DEFAULT",
            "RUNTIME_ALLOWED_ALL",
        };

        foreach (var pattern in forbiddenApprovalPatterns)
        {
            Assert.DoesNotContain(pattern, source, StringComparison.OrdinalIgnoreCase);
        }
    }

    private static IReadOnlyList<Dictionary<string, string>> ReadResolutionRows() =>
        ReadTable(
            FindRepoFile(".planning", "phases", "05-blue-battle-runtime-support", "05-RESOLUTION.md"),
            "Phase 4 Missing-Evidence Row");

    private static IReadOnlyList<Dictionary<string, string>> ReadTable(string path, string markerHeader)
    {
        var lines = File.ReadAllLines(path);
        for (var i = 0; i < lines.Length - 1; i++)
        {
            if (!lines[i].Contains(markerHeader, StringComparison.Ordinal))
            {
                continue;
            }

            var headers = SplitCells(lines[i]);
            var rows = new List<Dictionary<string, string>>();
            for (var rowIndex = i + 2; rowIndex < lines.Length; rowIndex++)
            {
                var line = lines[rowIndex];
                if (!line.StartsWith('|') || !line.EndsWith('|'))
                {
                    break;
                }

                var cells = SplitCells(line);
                Assert.Equal(headers.Length, cells.Length);
                rows.Add(headers.Zip(cells).ToDictionary(pair => pair.First, pair => pair.Second, StringComparer.Ordinal));
            }

            return rows;
        }

        throw new InvalidOperationException($"Could not find markdown table with header '{markerHeader}' in {path}.");
    }

    private static string[] SplitCells(string line) =>
        line.Trim().Trim('|').Split('|', StringSplitOptions.TrimEntries);

    private static string FindRepoFile(params string[] pathParts)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            var candidate = Path.Combine(new[] { directory.FullName }.Concat(pathParts).ToArray());
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException($"Could not find {Path.Combine(pathParts)}.");
    }
}
