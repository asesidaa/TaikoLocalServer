namespace TaikoLocalServer.Tests.WebUi;

public sealed class GreenCustomizationWebUiTests
{
    [Fact]
    public void UserCard_OffersGreenProfileCustomizationRoute()
    {
        var markup = ReadWebUiFile("Components", "UserCard.razor");

        Assert.Contains("WebUiEra.UserRoute(User.Baid, \"Green\", \"Profile\")", markup);
    }

    [Fact]
    public void Profile_GatesNijiiroAchievementControlsOutsideGreen()
    {
        var markup = ReadWebUiFile("Pages", "Profile.razor");

        AssertLabelGuardedByIfNotGreen(markup, "Achievement Panel Difficulty");
        AssertLabelGuardedByIfNotGreen(markup, "Display Achievement Panel");
    }

    [Fact]
    public void Profile_HidesNijiiroTaikoPreviewAndColorSwatchesForGreen()
    {
        var markup = ReadWebUiFile("Pages", "Profile.razor");

        AssertLabelGuardedByIfNotGreen(markup, "@* Player Visualizer *@");
        Assert.Contains("ShowSwatches=\"@(!IsGreen)\"", markup);
    }

    private static void AssertLabelGuardedByIfNotGreen(string markup, string label)
    {
        var index = markup.IndexOf(label, StringComparison.Ordinal);
        Assert.True(index >= 0, $"Could not find '{label}' in Profile.razor.");

        var prefixStart = Math.Max(0, index - 2000);
        var prefix = markup[prefixStart..index];
        Assert.Contains("@if (!IsGreen)", prefix);
    }

    private static string ReadWebUiFile(params string[] pathParts)
        => File.ReadAllText(Path.Combine([FindRepoRoot(), "TaikoWebUI", .. pathParts]));

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "TaikoLocalServer.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not locate repository root.");
    }
}
