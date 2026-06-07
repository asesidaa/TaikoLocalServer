namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowEvidenceTests
{
    [Theory]
    [InlineData("initialdatacheck.php")]
    [InlineData("playresult.php")]
    [InlineData("/v01r00/chassis/startupauth.php")]
    [InlineData("/v01r00/chassis/verupauth.php")]
    [InlineData("/v01r00/chassis/verupcomplete.php")]
    [InlineData("direct protobuf")]
    [InlineData("Exact Yellow game route prefix")]
    [InlineData("/v09r00")]
    [InlineData("explicit user approval on 2026-06-07")]
    [InlineData("runtime HTTP framing")]
    [InlineData("getreitai.php")]
    [InlineData("battleuserdata.php")]
    [InlineData("no Yellow battle")]
    [InlineData(".tools/yellow/EBOOT.ELF.i64")]
    [InlineData("later IDA research")]
    public void EvidenceRecord_CapturesYellowRouteTransportAndNoBattleBoundaries(string expected)
    {
        var evidence = File.ReadAllText(FindRepoFile(
            ".planning",
            "phases",
            "12-yellow-evidence-and-era-foundation",
            "12-YELLOW-EVIDENCE.md"));

        Assert.Contains(expected, evidence, StringComparison.OrdinalIgnoreCase);
    }

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
