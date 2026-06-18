using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Application;
using RedChallengeCompeController = TaikoLocalServer.Adapters.GameProtocol.Red.Controllers.ChallengeCompeController;
using RedWire = TaikoLocalServer.Adapters.GameProtocol.Red.Wire;

namespace TaikoLocalServer.Tests.Red;

public sealed class RedChallengeCompeProtocolTests
{
    [Fact]
    public async Task GetChallengeCompeQuery_Red_ReturnsEmptyBucketsWithoutReadingDonChallengeState()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync();
        AddUser(fixture, protocolChallengeVisible: true);
        var save = await fixture.Context.UserSaveDataRed.SingleAsync(row => row.Baid == 1);
        var releaseBefore = save.ReleaseSongFlg.ToArray();
        var titleBefore = save.TitleFlg.ToArray();
        AddDonChallengeProgress(fixture);
        AddDonChallengeRawFact(fixture);
        await fixture.Context.SaveChangesAsync();
        var handler = new GetChallengeCompeQueryHandler(NullLogger<GetChallengeCompeQueryHandler>.Instance);

        var response = await handler.Handle(new GetChallengeCompeQuery(GameEra.Red, 1), CancellationToken.None);

        Assert.Empty(response.AryChallengeStat);
        Assert.Empty(response.AryUserCompeStat);
        Assert.Empty(response.AryBngCompeStat);
        Assert.Equal(1, await fixture.Context.RedDonChallengeProgress.CountAsync());
        Assert.Equal(1, await fixture.Context.RedDonChallengeRawFacts.CountAsync());
        var after = await fixture.Context.UserSaveDataRed.AsNoTracking().SingleAsync(row => row.Baid == 1);
        Assert.True(after.IsChallengeCompe);
        Assert.Equal(releaseBefore, after.ReleaseSongFlg);
        Assert.Equal(titleBefore, after.TitleFlg);
    }

    [Fact]
    public async Task ChallengeCompeController_Red_ReturnsEmptyBucketsWhenDonChallengeProgressExists()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync();
        AddUser(fixture, protocolChallengeVisible: true);
        AddDonChallengeProgress(fixture);
        await fixture.Context.SaveChangesAsync();
        var controller = new RedChallengeCompeController
        {
            ControllerContext = new ControllerContext { HttpContext = CreateHttpContext(CreateServices(fixture)) }
        };

        var result = await controller.ChallengeCompe(new RedWire.ChallengeCompeRequest
        {
            Baid = 1,
            ChassisId = "268410000000",
            ShopId = "JPN0JPN0123"
        });

        var response = Assert.IsType<RedWire.ChallengeCompeResponse>(Assert.IsType<OkObjectResult>(result).Value);
        Assert.Equal(1u, response.Result);
        Assert.Empty(response.AryChallengeStats);
        Assert.Empty(response.AryUserCompeStats);
        Assert.Empty(response.AryBngCompeStats);
    }

    private static void AddUser(RedHandlerFixture fixture, bool protocolChallengeVisible)
    {
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataRedExtensions.CreateDefaultRedSaveData(1);
        save.IsChallengeCompe = protocolChallengeVisible;
        fixture.Context.UserSaveDataRed.Add(save);
        fixture.Context.SaveChanges();
    }

    private static void AddDonChallengeProgress(RedHandlerFixture fixture)
        => fixture.Context.RedDonChallengeProgress.Add(new RedDonChallengeProgress
        {
            Baid = 1,
            BundleId = "red-2016-07",
            TaskId = 1001,
            Slot = 1,
            CompeId = 1001,
            TrackNo = 1,
            SongNo = 101,
            Level = 1,
            OptionFlg = [1, 2, 3],
            StageMode = 0,
            HighScore = 765432,
            ProgressValue = 1,
            Completed = true,
            UpdatedAt = new DateTime(2016, 7, 20, 12, 0, 0),
            CompletedAt = new DateTime(2016, 7, 20, 12, 0, 0)
        });

    private static void AddDonChallengeRawFact(RedHandlerFixture fixture)
        => fixture.Context.RedDonChallengeRawFacts.Add(new RedDonChallengeRawFact
        {
            Baid = 1,
            BundleId = "red-2016-07",
            TaskId = 1001,
            Slot = 1,
            CompeId = 1001,
            TrackNo = 1,
            SongNo = 101,
            Level = 1,
            OptionFlg = [9],
            StageMode = 0,
            HighScore = 999999,
            PlayResult = 2,
            ProgressValue = 1,
            Completed = true,
            PlayTime = new DateTime(2016, 7, 20, 12, 0, 0),
            CreatedAt = new DateTime(2016, 7, 20, 12, 0, 0)
        });

    private static ServiceProvider CreateServices(RedHandlerFixture fixture)
        => new ServiceCollection()
            .AddLogging()
            .AddApplication()
            .AddScoped<ITaikoDbContext>(_ => fixture.Context)
            .AddScoped<IGameDataCatalog>(_ => fixture.Catalog)
            .BuildServiceProvider();

    private static DefaultHttpContext CreateHttpContext(IServiceProvider services)
        => new() { RequestServices = services };
}
