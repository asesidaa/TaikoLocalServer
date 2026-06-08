using System.Net;
using TaikoWebUI.Services;

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

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public List<string> RequestPaths { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.PathAndQuery.TrimStart('/') ?? string.Empty;
            RequestPaths.Add(path);
            var content = path.Contains("MusicDetails", StringComparison.Ordinal)
                || path.Contains("customization/titles", StringComparison.Ordinal)
                || path.Contains("customization/neiros", StringComparison.Ordinal)
                    ? "{}"
                    : "[]";

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content)
            });
        }
    }
}
