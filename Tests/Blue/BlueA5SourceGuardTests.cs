namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueA5SourceGuardTests
{
    [Fact]
    public void BlueA5ApplicationCode_DoesNotDependOnGreenDanState()
    {
        var root = FindRepoRoot();
        var files = new[]
        {
            Path.Combine(root, "Application", "Common", "BlueDanHelpers.cs"),
            Path.Combine(root, "Application", "Handlers", "UpdatePlayResultCommand.Blue.cs"),
            Path.Combine(root, "Application", "Handlers", "GetDanScoreQuery.Blue.cs"),
            Path.Combine(root, "Application", "Handlers", "GetTaikojukuQuery.Blue.cs"),
            Path.Combine(root, "Application", "Handlers", "UserDataQuery.Blue.cs"),
            Path.Combine(root, "Application", "Handlers", "BaidQuery.Blue.cs")
        };

        foreach (var file in files)
        {
            var source = File.ReadAllText(file);
            Assert.DoesNotContain("GreenDanHelpers", source, StringComparison.Ordinal);
            Assert.DoesNotContain("GreenProtocolBytes", source, StringComparison.Ordinal);
            Assert.DoesNotContain("DanScoreDatumGreen", source, StringComparison.Ordinal);
            Assert.DoesNotContain("DanStageScoreDatumGreen", source, StringComparison.Ordinal);
            Assert.DoesNotContain("UserSaveDataGreen", source, StringComparison.Ordinal);
            Assert.DoesNotContain("Adapters.GameProtocol.Green", source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void BlueA5AdapterCode_DoesNotDependOnGreenAdapterTypes()
    {
        var root = FindRepoRoot();
        var files = new[]
        {
            Path.Combine(root, "Adapters.GameProtocol.Blue", "Controllers", "TaikojukuController.cs"),
            Path.Combine(root, "Adapters.GameProtocol.Blue", "Mappers", "TaikojukuMappers.cs")
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
