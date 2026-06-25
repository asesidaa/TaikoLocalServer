using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.GameProtocol.Momoiro.Controllers;
using TaikoLocalServer.Adapters.GameProtocol.Momoiro.Wire;
using TaikoLocalServer.Application;
using TaikoLocalServer.Application.Settings;
using TaikoLocalServer.Infrastructure;

namespace TaikoLocalServer.Tests.Momoiro;

public sealed class MomoiroMetadataRouteTests
{
    [Fact]
    public async Task RecommendController_ReturnsCatalogBackedRecommendation()
    {
        CopyMomoiroCatalogFilesToProcessRoot();
        await using var provider = BuildMomoiroProvider();

        var response = await InvokeActionAsync<RecommendController, RecommendResponse>(
            provider,
            nameof(RecommendController.Recommend),
            new RecommendRequest { ChassisId = "chassis", GenderType = 0, PlayerAge = 0 });

        Assert.Equal(1u, response.Result);
        Assert.True(response.ShouldSerializeRecommendSong());
        Assert.NotEqual(0u, response.RecommendSong.GetValueOrDefault());
        Assert.Empty(response.RecommendBestSongs ?? []);
    }

    [Fact]
    public async Task DefaultSongController_ReturnsSongHashVersionAndCompactedFlagBody()
    {
        CopyMomoiroCatalogFilesToProcessRoot();
        await using var provider = BuildMomoiroProvider();

        var response = await InvokeActionAsync<DefaultSongController, DefaultsongResponse>(
            provider,
            nameof(DefaultSongController.DefaultSong),
            new DefaultsongRequest { ChassisId = "chassis" });

        Assert.Equal(1u, response.Result);
        Assert.True(response.ShouldSerializeSongHashVer());
        Assert.Equal(538_116_869u, response.SongHashVer);
        Assert.True(response.ShouldSerializeHashDefaultSongFlg());
        Assert.Equal(48, response.HashDefaultSongFlg.Length);
    }

    [Fact]
    public async Task SongHashController_ReturnsSongHashVersionAndEncodedTable()
    {
        CopyMomoiroCatalogFilesToProcessRoot();
        await using var provider = BuildMomoiroProvider();

        var response = await InvokeActionAsync<SongHashController, SonghashResponse>(
            provider,
            nameof(SongHashController.SongHash),
            new SonghashRequest { ChassisId = "chassis" });

        Assert.Equal(1u, response.Result);
        Assert.True(response.ShouldSerializeSongHashVer());
        Assert.Equal(538_116_869u, response.SongHashVer);
        Assert.True(response.ShouldSerializeSongHashTbl());
        Assert.Equal(760, response.SongHashTbl.Length);
    }

    [Fact]
    public async Task TelopCheckController_EmptyMomoiroTelopCatalogReturnsSuccessWithNoIds()
    {
        CopyMomoiroCatalogFilesToProcessRoot();
        await using var provider = BuildMomoiroProvider();

        var response = await InvokeActionAsync<TelopCheckController, TelopCheckResponse>(
            provider,
            nameof(TelopCheckController.TelopCheck),
            new TelopCheckRequest { ChassisId = "chassis" });

        Assert.Equal(1u, response.Result);
        Assert.NotNull(response.TelopIds);
        Assert.Empty(response.TelopIds);
    }

    [Fact]
    public async Task GetTelopQuery_MomoiroMissingTelopReturnsSuccessWithOmittedFields()
    {
        CopyMomoiroCatalogFilesToProcessRoot();
        await using var provider = BuildMomoiroProvider();
        var catalog = provider.GetRequiredService<IGameDataCatalog>();
        await catalog.InitializeAsync(CancellationToken.None);
        var handler = new GetTelopQueryHandler(catalog);

        var response = await handler.Handle(new GetTelopQuery(GameEra.Momoiro, 99), CancellationToken.None);

        Assert.Equal(1u, response.Result);
        Assert.Null(response.StartDatetime);
        Assert.Null(response.EndDatetime);
        Assert.Null(response.Telop);
    }

    [Fact]
    public async Task HeartbeatAndBookkeepingControllers_RemainStaticOperationalSuccess()
    {
        await using var provider = BuildMomoiroProvider();

        var heartbeat = await InvokeActionAsync<HeartbeatController, HeartBeatResponse>(
            provider,
            nameof(HeartbeatController.Heartbeat),
            new HeartBeatRequest { ChassisId = "chassis" });
        var bookkeeping = await InvokeActionAsync<BookkeepingController, BookKeepingResponse>(
            provider,
            nameof(BookkeepingController.Bookkeeping),
            new BookKeepingRequest { ChassisId = "chassis", ShopId = "shop" });

        Assert.Equal(1u, heartbeat.Result);
        Assert.Equal(1u, heartbeat.ComSvrStat);
        Assert.Equal(1u, heartbeat.GameSvrStat);
        Assert.Equal(1u, bookkeeping.Result);
    }

    private static async Task<TResponse> InvokeActionAsync<TController, TResponse>(
        IServiceProvider services,
        string actionName,
        object request)
        where TController : ControllerBase
    {
        var controller = ActivatorUtilities.CreateInstance<TController>(services);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { RequestServices = services }
        };

        var method = typeof(TController).GetMethod(actionName, [request.GetType()]);
        Assert.NotNull(method);

        var result = method.Invoke(controller, [request]);
        if (result is Task<IActionResult> task)
        {
            result = await task;
        }
        else if (result is ValueTask<IActionResult> valueTask)
        {
            result = await valueTask;
        }

        var ok = Assert.IsType<OkObjectResult>(result);
        return Assert.IsType<TResponse>(ok.Value);
    }

    private static ServiceProvider BuildMomoiroProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddApplication();
        services.AddInfrastructure(BuildConfiguration(), new HashSet<GameEra> { GameEra.Momoiro });
        return services.BuildServiceProvider();
    }

    private static IConfigurationRoot BuildConfiguration()
        => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DbFileName"] = $"momoiro-metadata-test-{Guid.NewGuid():N}.db",
                ["AuthSettings:JwtIssuer"] = "test",
                ["AuthSettings:JwtAudience"] = "test",
                ["AuthSettings:JwtKey"] = "0123456789abcdef0123456789abcdef",
                ["ServerSettings:Eras:Momoiro:Enabled"] = "true",
                ["ServerSettings:Eras:Momoiro:AutoExtractCatalog"] = "false",
                ["ServerSettings:Eras:Momoiro:GameDataPath"] = ProcessMomoiroDataRoot()
            })
            .Build();

    private static void CopyMomoiroCatalogFilesToProcessRoot()
    {
        var repoRoot = FindRepoRoot();
        var targetRoot = ProcessMomoiroRoot();
        if (Directory.Exists(targetRoot))
        {
            Directory.Delete(targetRoot, recursive: true);
        }

        Copy(
            Path.Combine(repoRoot, "Host", "wwwroot", "data", "momoiro", "data", "musicinfo.xml"),
            Path.Combine(targetRoot, "data", "musicinfo.xml"));
        Copy(
            Path.Combine(repoRoot, "Host", "wwwroot", "data", "momoiro", "data", "musicmedleyinfo.xml"),
            Path.Combine(targetRoot, "data", "musicmedleyinfo.xml"));
        Copy(
            Path.Combine(repoRoot, "Host", "wwwroot", "data", "momoiro", "data", "defmusic.bin"),
            Path.Combine(targetRoot, "data", "defmusic.bin"));
        Copy(
            Path.Combine(repoRoot, "Host", "wwwroot", "data", "momoiro", "data", "fumen", "tuning.bin"),
            Path.Combine(targetRoot, "data", "fumen", "tuning.bin"));
    }

    private static string ProcessMomoiroRoot()
        => Path.Combine(
            Path.GetDirectoryName(Environment.ProcessPath)
                ?? throw new ApplicationException("Cannot resolve process directory."),
            "wwwroot",
            "data",
            "momoiro");

    private static string ProcessMomoiroDataRoot()
        => Path.Combine(ProcessMomoiroRoot(), "data");

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "TaikoLocalServer.slnx")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Could not locate repository root.");
    }

    private static void Copy(string source, string destination)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(destination)
            ?? throw new ApplicationException($"Cannot resolve directory for {destination}."));
        File.Copy(source, destination, overwrite: true);
    }
}
