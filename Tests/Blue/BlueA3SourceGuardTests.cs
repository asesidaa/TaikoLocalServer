namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueA3SourceGuardTests
{
    [Fact]
    public void BlueA3ApplicationCode_DoesNotDependOnGreenProtocolState()
    {
        var root = FindRepoRoot();
        var files = new[]
        {
            Path.Combine(root, "Application", "Handlers", "BaidQuery.Blue.cs"),
            Path.Combine(root, "Application", "Handlers", "AddMyDonEntryCommand.Blue.cs"),
            Path.Combine(root, "Application", "Handlers", "GetInitialDataQuery.Blue.cs"),
            Path.Combine(root, "Application", "Handlers", "UserDataQuery.Blue.cs"),
            Path.Combine(root, "Application", "Common", "UserSaveDataBlueExtensions.cs"),
            Path.Combine(root, "Application", "Common", "BlueProtocolBytes.cs")
        };

        foreach (var file in files)
        {
            var source = File.ReadAllText(file);
            Assert.DoesNotContain("GreenProtocolBytes", source, StringComparison.Ordinal);
            Assert.DoesNotContain("IGreenCatalog", source, StringComparison.Ordinal);
            Assert.DoesNotContain("UserSaveDataGreen", source, StringComparison.Ordinal);
            Assert.DoesNotContain("GreenShop", source, StringComparison.Ordinal);
            Assert.DoesNotContain("GreenGhost", source, StringComparison.Ordinal);
            Assert.DoesNotContain("GreenDan", source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void BlueA3AdapterMappers_DoNotDependOnGreenAdapterTypes()
    {
        var root = FindRepoRoot();
        var mapperRoot = Path.Combine(root, "Adapters.GameProtocol.Blue", "Mappers");

        foreach (var file in Directory.EnumerateFiles(mapperRoot, "*.cs", SearchOption.AllDirectories))
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
