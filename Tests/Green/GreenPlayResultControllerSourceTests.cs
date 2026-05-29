namespace TaikoLocalServer.Tests.Green;

public sealed class GreenPlayResultControllerSourceTests
{
    [Fact]
    public void GreenPlayResultController_LogsOuterPayloadSummary()
    {
        var source = File.ReadAllText(Path.Combine(
            FindRepoRoot(),
            "Adapters.GameProtocol.Green",
            "Controllers",
            "PlayResultController.cs"));

        Assert.Contains("payload_bytes={PayloadBytes}", source, StringComparison.Ordinal);
        Assert.Contains("payload_hex={PayloadHex}", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Green PlayResult request: {Request}", source, StringComparison.Ordinal);
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
