namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueHostProgramSourceTests
{
    [Fact]
    public void Program_RegistersAndFiltersBlueAdapter()
    {
        var source = File.ReadAllText(FindRepoFile("Host", "Program.cs"));

        Assert.Contains("using TaikoLocalServer.Adapters.GameProtocol.Blue;", source, StringComparison.Ordinal);
        Assert.Contains("enabledEras.Contains(GameEra.Blue)", source, StringComparison.Ordinal);
        Assert.Contains("builder.Services.AddGameProtocolBlue();", source, StringComparison.Ordinal);
        Assert.Contains("RemoveApplicationPart(apm, \"TaikoLocalServer.Adapters.GameProtocol.Blue\");", source, StringComparison.Ordinal);
    }

    [Fact]
    public void Program_AssumesProtobufForBlueGameRoutes()
    {
        var source = File.ReadAllText(FindRepoFile("Host", "Program.cs"));

        Assert.Contains("path.StartsWithSegments(\"/v10r03/chassis\", StringComparison.OrdinalIgnoreCase)", source, StringComparison.Ordinal);
        Assert.DoesNotContain("path.StartsWithSegments(\"/v10r03\", StringComparison.OrdinalIgnoreCase)", source, StringComparison.Ordinal);
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
