using System.Reflection;
using TaikoWebUI.Pages;

namespace TaikoLocalServer.Tests.WebUi;

public sealed class DaniDojoTitleTests
{
    [Fact]
    public void GetDanTitle_FallsBackToCatalogTitle()
    {
        const string catalogTitle = "\u521d\u7d1a";
        var title = InvokeGetDanTitle(catalogTitle);

        Assert.Equal(catalogTitle, title);
    }

    [Fact]
    public void DaniTabCss_AllowsContentSizedTitles()
    {
        var css = File.ReadAllText(Path.Combine(FindRepoRoot(), "TaikoWebUI", "wwwroot", "css", "app.css"));

        Assert.Contains("flex: 0 0 max-content;", css);
        Assert.Contains("min-width: max-content;", css);
        Assert.DoesNotContain("text-overflow: ellipsis;", css);
    }

    private static string InvokeGetDanTitle(string title)
    {
        var method = typeof(DaniDojo).GetMethod(
            "GetDanTitle",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);

        return Assert.IsType<string>(method.Invoke(new DaniDojo(), [title]));
    }

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
