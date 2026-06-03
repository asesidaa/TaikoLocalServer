namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueA4SourceGuardTests
{
    [Fact]
    public void BlueA4ApplicationCode_DoesNotDependOnGreenProtocolState()
    {
        var root = FindRepoRoot();
        var files = new[]
        {
            Path.Combine(root, "Application", "Handlers", "UpdatePlayResultCommand.Blue.cs"),
            Path.Combine(root, "Application", "Handlers", "GetSelfBestQuery.Blue.cs"),
            Path.Combine(root, "Application", "Handlers", "UserDataQuery.Blue.cs"),
            Path.Combine(root, "Application", "Common", "BlueProtocolBytes.cs"),
            Path.Combine(root, "Application", "Common", "BluePlayResultMapping.cs"),
            Path.Combine(root, "Application", "Common", "BlueProfileCounters.cs"),
            Path.Combine(root, "Application", "Common", "BlueCrownResponseBuilder.cs")
        };

        foreach (var file in files)
        {
            var source = File.ReadAllText(file);
            Assert.DoesNotContain("GreenProtocolBytes", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Adapters.GameProtocol.Green", source, StringComparison.Ordinal);
            Assert.DoesNotContain("SongBestDatumGreen", source, StringComparison.Ordinal);
            Assert.DoesNotContain("SongPlayDatumGreen", source, StringComparison.Ordinal);
            Assert.DoesNotContain("UserSaveDataGreen", source, StringComparison.Ordinal);
            Assert.DoesNotContain("GreenShop", source, StringComparison.Ordinal);
            Assert.DoesNotContain("GreenGhost", source, StringComparison.Ordinal);
            Assert.DoesNotContain("GreenDan", source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void BlueA4AdapterMappers_DoNotDependOnGreenAdapterTypes()
    {
        var root = FindRepoRoot();
        var files = new[]
        {
            Path.Combine(root, "Adapters.GameProtocol.Blue", "Mappers", "PlayResultMappers.cs"),
            Path.Combine(root, "Adapters.GameProtocol.Blue", "Mappers", "SelfBestMappers.cs")
        };

        foreach (var file in files)
        {
            var source = File.ReadAllText(file);
            Assert.DoesNotContain("Adapters.GameProtocol.Green", source, StringComparison.Ordinal);
            Assert.DoesNotContain("GreenProtocolBytes", source, StringComparison.Ordinal);
        }
    }

    private static string FindRepoRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "TaikoLocalServer.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("Could not find TaikoLocalServer.slnx.");
    }
}
