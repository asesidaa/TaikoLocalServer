using System.Net;
using TaikoWebUI.Services;
using TaikoWebUI.Utilities;

namespace TaikoLocalServer.Tests.WebUi;

public sealed class GameDataServiceTests
{
    [Fact]
    public async Task InitializeAsync_LoadsDanDataOnlyForEnabledEras()
    {
        var handler = new RecordingHandler();
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        var service = new GameDataService(client);

        await service.InitializeAsync("http://localhost/", ["Green"]);

        Assert.Equal(["api/Green/GameData/DanData"], handler.RequestPaths);
        Assert.Empty(service.GetDanMap("Nijiiro"));
    }

    [Fact]
    public async Task InitializeAsync_LoadsBlueDanDataWhenEnabled()
    {
        var handler = new RecordingHandler();
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        var service = new GameDataService(client);

        await service.InitializeAsync("http://localhost/", ["Blue"]);

        Assert.Equal(["api/Blue/GameData/DanData"], handler.RequestPaths);
        Assert.Empty(service.GetDanMap("Green"));
    }

    [Fact]
    public async Task InitializeAsync_LoadsYellowDanDataWhenEnabled()
    {
        var handler = new RecordingHandler();
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        var service = new GameDataService(client);

        await service.InitializeAsync("http://localhost/", ["Yellow"]);

        Assert.Equal(["api/Yellow/GameData/DanData"], handler.RequestPaths);
        Assert.Empty(service.GetDanMap("Blue"));
    }

    [Fact]
    public async Task InitializeAsync_LoadsRedDanDataWhenEnabled()
    {
        var handler = new RecordingHandler();
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        var service = new GameDataService(client);

        await service.InitializeAsync("http://localhost/", ["Red"]);

        Assert.Equal(["api/Red/GameData/DanData"], handler.RequestPaths);
        Assert.Empty(service.GetDanMap("Yellow"));
    }

    [Fact]
    public async Task InitializeAsync_LoadsWhiteDanDataWhenEnabled()
    {
        var handler = new RecordingHandler();
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        var service = new GameDataService(client);

        await service.InitializeAsync("http://localhost/", ["White"]);

        Assert.Equal(["api/White/GameData/DanData"], handler.RequestPaths);
        Assert.Empty(service.GetDanMap("Red"));
    }

    [Fact]
    public async Task CatalogLookups_RequestYellowAdminApiRoutes()
    {
        var handler = new RecordingHandler();
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        var service = new GameDataService(client);

        await service.InitializeAsync("http://localhost/", ["Yellow"]);
        await service.GetMusicDetailDictionary("Yellow");
        await service.GetCostumeList("Yellow");
        await service.GetTitleDictionary("Yellow");
        await service.GetNeiroDictionary("Yellow");

        Assert.Equal(
            [
                "api/Yellow/GameData/DanData",
                "api/Yellow/GameData/MusicDetails",
                "api/Yellow/customization/costumes",
                "api/Yellow/customization/titles",
                "api/Yellow/customization/neiros"
            ],
            handler.RequestPaths);
    }

    [Fact]
    public async Task CatalogLookups_RequestRedAdminApiRoutes()
    {
        var handler = new RecordingHandler();
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        var service = new GameDataService(client);

        await service.InitializeAsync("http://localhost/", ["Red"]);
        await service.GetMusicDetailDictionary("Red");
        await service.GetCostumeList("Red");
        await service.GetTitleDictionary("Red");
        await service.GetNeiroDictionary("Red");

        Assert.Equal(
            [
                "api/Red/GameData/DanData",
                "api/Red/GameData/MusicDetails",
                "api/Red/customization/costumes",
                "api/Red/customization/titles",
                "api/Red/customization/neiros"
            ],
            handler.RequestPaths);
    }

    [Fact]
    public async Task CatalogLookups_RequestWhiteAdminApiRoutes()
    {
        var handler = new RecordingHandler();
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        var service = new GameDataService(client);

        await service.InitializeAsync("http://localhost/", ["White"]);
        await service.GetMusicDetailDictionary("White");
        await service.GetCostumeList("White");
        await service.GetTitleDictionary("White");
        await service.GetNeiroDictionary("White");

        Assert.Equal(
            [
                "api/White/GameData/DanData",
                "api/White/GameData/MusicDetails",
                "api/White/customization/costumes",
                "api/White/customization/titles",
                "api/White/customization/neiros"
            ],
            handler.RequestPaths);
    }

    [Fact]
    public async Task LegacyCatalogLookups_UseFirstEnabledEra()
    {
        var handler = new RecordingHandler();
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        var service = new GameDataService(client);

        await service.InitializeAsync("http://localhost/", ["Yellow"]);
        await service.GetMusicDetailDictionary();
        await service.GetCostumeList();
        await service.GetTitleDictionary();

        Assert.Equal(
            [
                "api/Yellow/GameData/DanData",
                "api/Yellow/GameData/MusicDetails",
                "api/Yellow/customization/costumes",
                "api/Yellow/customization/titles"
            ],
            handler.RequestPaths);
        Assert.Contains(900u, service.GetDanMap().Keys);
    }

    [Fact]
    public void NormalizeEnabled_IgnoresUnsupportedEras()
    {
        var enabled = WebUiEra.NormalizeEnabled(["Yellow", "Red", "White", "Unknown"]);

        Assert.Equal(["Yellow", "Red", "White"], enabled);
    }

    [Fact]
    public void Red_IsAc15()
    {
        Assert.True(WebUiEra.IsAc15("Red"));
    }

    [Fact]
    public void White_IsAc15()
    {
        Assert.True(WebUiEra.IsAc15("White"));
    }

    [Fact]
    public void FavoriteSongLimit_UsesServerSuppliedEraLimit()
    {
        var favoriteSongLimits = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Red"] = 5,
            ["Green"] = 10
        };

        Assert.Equal(5, WebUiEra.GetFavoriteSongLimit("red", favoriteSongLimits));
        Assert.Equal(10, WebUiEra.GetFavoriteSongLimit("Green", favoriteSongLimits));
        Assert.Null(WebUiEra.GetFavoriteSongLimit("Nijiiro", favoriteSongLimits));
    }

    [Fact]
    public void Red_RouteHelpersPreserveEra()
    {
        Assert.Equal("Users/123/Red/Songs", WebUiEra.UserRoute(123u, "Red", "Songs"));
        Assert.Equal("api/Red/PlayData/123", WebUiEra.Api("Red", "PlayData/123"));
    }

    [Fact]
    public void White_RouteHelpersPreserveEra()
    {
        Assert.Equal("Users/123/White/Songs", WebUiEra.UserRoute(123u, "White", "Songs"));
        Assert.Equal("api/White/PlayData/123", WebUiEra.Api("White", "PlayData/123"));
    }

    [Fact]
    public void OlderAc15DonChallengeCapability_IsRedAndWhiteOnly()
    {
        Assert.True(WebUiEra.SupportsOlderAc15DonChallenge("Red"));
        Assert.True(WebUiEra.SupportsOlderAc15DonChallenge("White"));
        Assert.False(WebUiEra.SupportsOlderAc15DonChallenge("Blue"));
        Assert.False(WebUiEra.SupportsOlderAc15DonChallenge("Nijiiro"));
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public List<string> RequestPaths { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.PathAndQuery.TrimStart('/') ?? string.Empty;
            RequestPaths.Add(path);
            var content = path switch
            {
                "api/Yellow/GameData/DanData" => """[{"danId":900,"title":"yellow"}]""",
                "api/Red/GameData/DanData" => """[{"danId":800,"title":"red"}]""",
                "api/White/GameData/DanData" => """[{"danId":700,"title":"white"}]""",
                _ when path.Contains("MusicDetails", StringComparison.Ordinal)
                    || path.Contains("customization/titles", StringComparison.Ordinal)
                    || path.Contains("customization/neiros", StringComparison.Ordinal) => "{}",
                _ => "[]"
            };

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content)
            });
        }
    }
}
