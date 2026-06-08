using System.Reflection;
using TaikoWebUI.Utilities;

namespace TaikoLocalServer.Tests.WebUi;

public sealed class YellowWebUiTests
{
    [Fact]
    public void WebUiEra_DefinesYellowAsSupportedAc15Era()
    {
        var yellowField = typeof(WebUiEra).GetField("Yellow", BindingFlags.Public | BindingFlags.Static);

        Assert.NotNull(yellowField);
        Assert.Equal("Yellow", yellowField.GetRawConstantValue());
        Assert.Contains("Yellow", WebUiEra.Supported);
        Assert.True(WebUiEra.IsSupported("yellow"));
        Assert.True(WebUiEra.IsSupported("YELLOW"));
        Assert.True(WebUiEra.IsAc15("Yellow"));
        Assert.False(WebUiEra.IsAc15(WebUiEra.Default));
    }

    [Fact]
    public void WebUiEra_NormalizesYellowAndBuildsYellowRoutes()
    {
        var defaultEnabledEras = WebUiEra.NormalizeEnabled(null);

        Assert.Equal("Yellow", WebUiEra.Normalize("yellow"));
        Assert.Contains("Yellow", defaultEnabledEras);
        Assert.Equal("Users/123/Yellow/Profile", WebUiEra.UserRoute(123u, "yellow", "Profile"));
        Assert.Equal("api/Yellow/GameData/DanData", WebUiEra.Api("YELLOW", "GameData/DanData"));
    }

    [Fact]
    public void ExistingUserPages_UseGenericEraRoutesForYellow()
    {
        var routeAssertions = new (string FileName, string[] Snippets)[]
        {
            ("Profile.razor.cs",
            [
                "WebUiEra.Api(CurrentEra, $\"UserSettings/{Baid}\")",
                "WebUiEra.Api(CurrentEra, $\"PlayData/{Baid}\")",
                "WebUiEra.UserRoute(Baid, CurrentEra, \"Profile\")"
            ]),
            ("HighScores.razor.cs",
            [
                "WebUiEra.Api(CurrentEra, $\"PlayData/{Baid}\")",
                "WebUiEra.Api(CurrentEra, $\"UserSettings/{Baid}\")",
                "WebUiEra.Api(CurrentEra, \"FavoriteSongs\")",
                "WebUiEra.UserRoute(Baid, CurrentEra, \"HighScores\")"
            ]),
            ("PlayHistory.razor.cs",
            [
                "WebUiEra.Api(CurrentEra, $\"PlayHistory/{Baid}\")",
                "WebUiEra.Api(CurrentEra, $\"UserSettings/{Baid}\")",
                "WebUiEra.Api(CurrentEra, \"FavoriteSongs\")",
                "WebUiEra.UserRoute(Baid, CurrentEra, \"PlayHistory\")"
            ]),
            ("SongList.razor.cs",
            [
                "WebUiEra.Api(CurrentEra, $\"PlayData/{Baid}\")",
                "WebUiEra.Api(CurrentEra, $\"UserSettings/{Baid}\")",
                "WebUiEra.Api(CurrentEra, \"FavoriteSongs\")",
                "WebUiEra.UserRoute(Baid, CurrentEra, \"Songs\")"
            ]),
            ("Song.razor.cs",
            [
                "WebUiEra.Api(CurrentEra, $\"PlayHistory/{Baid}\")",
                "WebUiEra.Api(CurrentEra, $\"UserSettings/{Baid}\")",
                "WebUiEra.Api(CurrentEra, \"FavoriteSongs\")",
                "WebUiEra.UserRoute(Baid, CurrentEra, \"Songs\")",
                "WebUiEra.UserRoute(Baid, CurrentEra, $\"Songs/{SongId}\")"
            ]),
            ("DaniDojo.razor.cs",
            [
                "WebUiEra.Api(CurrentEra, $\"DanBestData/{Baid}\")",
                "WebUiEra.Api(CurrentEra, $\"UserSettings/{Baid}\")",
                "WebUiEra.UserRoute(Baid, CurrentEra, \"DaniDojo\")"
            ])
        };

        foreach (var (fileName, snippets) in routeAssertions)
        {
            var code = ReadWebUiFile("Pages", fileName);

            foreach (var snippet in snippets)
            {
                Assert.Contains(snippet, code);
            }

            Assert.DoesNotContain("WebUiEra.Yellow", code);
            Assert.DoesNotContain("\"api/Yellow/", code);
            Assert.DoesNotContain("\"Users/{baid}/Yellow", code);
        }
    }

    [Fact]
    public void WebUi_DoesNotAddYellowOnlyShopTokkunOrBanacoinSurfaces()
    {
        var sourceFiles = Directory.EnumerateFiles(Path.Combine(FindRepoRoot(), "TaikoWebUI"), "*.*", SearchOption.AllDirectories)
            .Where(path => path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".razor.cs", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.EndsWith(Path.Combine("Utilities", "WebUiEra.cs"), StringComparison.OrdinalIgnoreCase));

        foreach (var file in sourceFiles)
        {
            var code = File.ReadAllText(file);

            Assert.DoesNotContain("Yellow", code);
            Assert.DoesNotContain("Tokkun", code);
            Assert.DoesNotContain("Banacoin", code);
        }
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
