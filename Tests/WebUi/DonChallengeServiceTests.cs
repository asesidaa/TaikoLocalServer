using System.Net;
using TaikoLocalServer.Contracts.AdminApi.Responses;
using TaikoWebUI.Services;

namespace TaikoLocalServer.Tests.WebUi;

public sealed class DonChallengeServiceTests
{
    [Fact]
    public async Task GetAvailabilityAsync_RedRequestsRedRoute()
    {
        var handler = new RecordingHandler(path => path switch
        {
            "api/Red/DonChallenge/availability" => Json("""{"era":"Red","isAvailable":true,"activeBundleId":"red-1"}"""),
            _ => NotFound()
        });
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        var service = new DonChallengeService(client);

        var response = await service.GetAvailabilityAsync("Red");

        Assert.True(response.IsAvailable);
        Assert.Equal("red-1", response.ActiveBundleId);
        Assert.Equal(["api/Red/DonChallenge/availability"], handler.RequestPaths);
    }

    [Fact]
    public async Task GetDonChallengeAsync_RedRequestsRedReadbackRoute()
    {
        var handler = new RecordingHandler(path => path switch
        {
            "api/Red/DonChallenge/123" => Json("""{"era":"Red","isAvailable":true,"bundleId":"red-1","completedTaskCount":1,"personalTaskCount":3}"""),
            _ => NotFound()
        });
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        var service = new DonChallengeService(client);

        var response = await service.GetDonChallengeAsync("Red", 123);

        Assert.True(response.IsAvailable);
        Assert.Equal("red-1", response.BundleId);
        Assert.Equal(1u, response.CompletedTaskCount);
        Assert.Equal(["api/Red/DonChallenge/123"], handler.RequestPaths);
    }

    [Fact]
    public async Task GetAvailabilityAsync_UnavailableKnownEraDoesNotFallback()
    {
        var handler = new RecordingHandler(path => path switch
        {
            "api/Blue/DonChallenge/availability" => Json("""{"era":"Blue","isAvailable":false,"message":"Don Challenge is not available for Blue."}"""),
            _ => NotFound()
        });
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        var service = new DonChallengeService(client);

        var response = await service.GetAvailabilityAsync("Blue");

        Assert.False(response.IsAvailable);
        Assert.Equal("Blue", response.Era);
        Assert.Equal("Don Challenge is not available for Blue.", response.Message);
        Assert.Equal(["api/Blue/DonChallenge/availability"], handler.RequestPaths);
    }

    [Fact]
    public async Task GetDonChallengeAsync_NotFoundReturnsUnavailableForRequestedKnownEra()
    {
        var handler = new RecordingHandler(_ => NotFound());
        using var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };
        var service = new DonChallengeService(client);

        DonChallengeResponse response = await service.GetDonChallengeAsync("Blue", 123);

        Assert.False(response.IsAvailable);
        Assert.Equal("Blue", response.Era);
        Assert.Equal("Don Challenge is not available for Blue.", response.Message);
        Assert.Equal(["api/Blue/DonChallenge/123"], handler.RequestPaths);
    }

    private static HttpResponseMessage Json(string json)
        => new(HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        };

    private static HttpResponseMessage NotFound()
        => new(HttpStatusCode.NotFound);

    private sealed class RecordingHandler(Func<string, HttpResponseMessage> responder) : HttpMessageHandler
    {
        public List<string> RequestPaths { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.PathAndQuery.TrimStart('/') ?? string.Empty;
            RequestPaths.Add(path);
            return Task.FromResult(responder(path));
        }
    }
}
