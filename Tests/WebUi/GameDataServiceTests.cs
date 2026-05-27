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

    private sealed class RecordingHandler : HttpMessageHandler
    {
        public List<string> RequestPaths { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestPaths.Add(request.RequestUri?.PathAndQuery.TrimStart('/') ?? string.Empty);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]")
            });
        }
    }
}
