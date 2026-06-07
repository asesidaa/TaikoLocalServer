namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowHostProgramSourceTests
{
    [Fact]
    public void Program_RegistersAndFiltersYellowAdapter()
    {
        var source = File.ReadAllText(FindRepoFile("Host", "Program.cs"));

        Assert.Contains("using TaikoLocalServer.Adapters.GameProtocol.Yellow;", source, StringComparison.Ordinal);
        Assert.Contains("enabledEras.Contains(GameEra.Yellow)", source, StringComparison.Ordinal);
        Assert.Contains("builder.Services.AddGameProtocolYellow();", source, StringComparison.Ordinal);
        Assert.Contains("RemoveApplicationPart(apm, \"TaikoLocalServer.Adapters.GameProtocol.Yellow\");", source, StringComparison.Ordinal);
    }

    [Fact]
    public void Program_AssumesProtobufForYellowGameRoutes()
    {
        var source = File.ReadAllText(FindRepoFile("Host", "Program.cs"));

        Assert.Contains("path.StartsWithSegments(\"/v09r00/chassis\", StringComparison.OrdinalIgnoreCase)", source, StringComparison.Ordinal);
        Assert.DoesNotContain("path.StartsWithSegments(\"/v09r00\", StringComparison.OrdinalIgnoreCase)", source, StringComparison.Ordinal);
    }

    [Fact]
    public void HostProject_ReferencesYellowAdapterAndExcludesRawYellowData()
    {
        var source = File.ReadAllText(FindRepoFile("Host", "Host.csproj"));

        Assert.Contains(@"..\Adapters.GameProtocol.Yellow\Adapters.GameProtocol.Yellow.csproj", source, StringComparison.Ordinal);
        Assert.Contains(@"wwwroot\data\yellow\data\**", source, StringComparison.Ordinal);
        Assert.Contains("CreateYellowGameDataSymlinkForDebug", source, StringComparison.Ordinal);
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
