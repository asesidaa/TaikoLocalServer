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
}
