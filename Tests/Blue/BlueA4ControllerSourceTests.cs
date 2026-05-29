namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueA4ControllerSourceTests
{
    [Fact]
    public void BluePlayResultController_MapsAndSendsBlueUpdateCommand()
    {
        var source = File.ReadAllText(Path.Combine(
            FindRepoRoot(),
            "Adapters.GameProtocol.Blue",
            "Controllers",
            "PlayResultController.cs"));

        Assert.Contains("PlayResultMappers.Map(request)", source, StringComparison.Ordinal);
        Assert.Contains("new UpdatePlayResultCommand(request.Baid, GameEra.Blue, common)", source, StringComparison.Ordinal);
        Assert.Contains("PlayResultMappers.Map(result)", source, StringComparison.Ordinal);
    }

    [Fact]
    public void BluePlayResultController_LogsFullRequestDump()
    {
        var source = File.ReadAllText(Path.Combine(
            FindRepoRoot(),
            "Adapters.GameProtocol.Blue",
            "Controllers",
            "PlayResultController.cs"));

        Assert.Contains("request.Stringify()", source, StringComparison.Ordinal);
        Assert.DoesNotContain("play_datetime={PlayDatetime}", source, StringComparison.Ordinal);
    }

    [Fact]
    public void BlueRewardCardCheckController_UsesSharedCardLookup()
    {
        var source = File.ReadAllText(Path.Combine(
            FindRepoRoot(),
            "Adapters.GameProtocol.Blue",
            "Controllers",
            "RewardCardCheckController.cs"));

        Assert.Contains("new RewardCardCheckQuery(request.AccessCode)", source, StringComparison.Ordinal);
        Assert.Contains("Baid = common.Baid", source, StringComparison.Ordinal);
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
