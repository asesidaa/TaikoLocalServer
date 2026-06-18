using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.AdminApi.Controllers;
using TaikoLocalServer.Application;
using TaikoLocalServer.Application.Ac15.DonChallenge;
using TaikoLocalServer.Application.Catalog.Ac15;
using TaikoLocalServer.Contracts.AdminApi.Responses;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Tests.White;

public sealed class WhiteDonChallengeAdminApiTests
{
    [Fact]
    public async Task Availability_White_ReportsActiveConfiguredBundle()
    {
        await using var fixture = await WhiteHandlerFixture.CreateAsync(CreateCatalog());
        var controller = CreateController(fixture);

        var result = await controller.GetAvailability("White");

        var response = AssertOk<DonChallengeAvailabilityResponse>(result);
        Assert.Equal("White", response.Era);
        Assert.True(response.IsAvailable);
        Assert.Equal("white-2016-06", response.ActiveBundleId);
        Assert.Equal(DateTimeOffset.Parse("2016-06-01T00:00:00Z"), response.StartsAt);
        Assert.Equal(DateTimeOffset.Parse("2016-07-01T00:00:00Z"), response.EndsAt);
    }

    [Fact]
    public async Task GetDonChallenge_White_ReturnsWhiteProgressAndRewardsWithoutRedFallback()
    {
        await using var fixture = await WhiteHandlerFixture.CreateAsync(CreateCatalog(
            rewards: [new Ac15DonChallengeReward(1, [104], [10])]));
        AddUser(fixture);
        fixture.Context.WhiteDonChallengeProgress.Add(new WhiteDonChallengeProgress
        {
            Baid = 1,
            BundleId = "white-2016-06",
            TaskId = 1001,
            Slot = 1,
            CompeId = 1001,
            TrackNo = 0,
            SongNo = 101,
            Level = 1,
            OptionFlg = [1, 2, 3],
            StageMode = 0,
            HighScore = 765432,
            ProgressValue = 1,
            Completed = true,
            UpdatedAt = new DateTime(2016, 6, 20, 12, 0, 0),
            CompletedAt = new DateTime(2016, 6, 20, 12, 0, 0)
        });
        fixture.Context.RedDonChallengeProgress.Add(new RedDonChallengeProgress
        {
            Baid = 1,
            BundleId = "red-2016-07",
            TaskId = 9999,
            Slot = 9,
            CompeId = 9999,
            TrackNo = 0,
            SongNo = 999,
            Level = 4,
            StageMode = 0,
            ProgressValue = 1,
            Completed = true,
            UpdatedAt = new DateTime(2016, 7, 20, 12, 0, 0),
            CompletedAt = new DateTime(2016, 7, 20, 12, 0, 0)
        });
        await fixture.Context.SaveChangesAsync();
        var controller = CreateController(fixture);

        var result = await controller.GetDonChallenge("White", 1);

        var response = AssertOk<DonChallengeResponse>(result);
        Assert.Equal("White", response.Era);
        Assert.True(response.IsAvailable);
        Assert.Equal("white-2016-06", response.BundleId);
        Assert.Equal(1u, response.CompletedTaskCount);
        var completedTask = Assert.Single(response.Tasks);
        Assert.Equal(1001u, completedTask.TaskId);
        Assert.Equal("White Task", completedTask.Name);
        Assert.True(completedTask.Completed);
        Assert.Equal([1u, 2u, 3u, 4u, 5u], completedTask.Tracks.Select(track => track.Level).ToArray());
        Assert.All(completedTask.Tracks, track => Assert.Equal(101u, track.SongNumber));
        Assert.Equal(DonChallengeRewardStatus.Earned, Assert.Single(response.Rewards).Status);
        Assert.Equal(1, await fixture.Context.RedDonChallengeProgress.CountAsync());
    }

    [Fact]
    public async Task GetDonChallenge_White_NoActiveBundleReturnsReadableUnavailableResponse()
    {
        await using var fixture = await WhiteHandlerFixture.CreateAsync(CreateCatalog(activeBundleId: null));
        AddUser(fixture);
        var controller = CreateController(fixture);

        var availability = AssertOk<DonChallengeAvailabilityResponse>(await controller.GetAvailability("White"));
        var readback = AssertOk<DonChallengeResponse>(await controller.GetDonChallenge("White", 1));

        Assert.False(availability.IsAvailable);
        Assert.Equal("No active Don Challenge is configured for White.", availability.Message);
        Assert.False(readback.IsAvailable);
        Assert.Equal("No active Don Challenge is configured for White.", readback.Message);
        Assert.Empty(readback.Tasks);
        Assert.Empty(readback.Rewards);
    }

    private static void AddUser(WhiteHandlerFixture fixture)
    {
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataWhite.Add(UserSaveDataWhiteExtensions.CreateDefaultWhiteSaveData(1));
        fixture.Context.SaveChanges();
    }

    private static WhiteHandlerFixture.TestWhiteCatalog CreateCatalog(
        string? activeBundleId = "white-2016-06",
        IReadOnlyList<Ac15DonChallengeReward>? rewards = null)
        => new(
            musicInfoFileOrder:
            [
                new Ac15MusicInfoEntry { SongNo = 101, MusicId = "task_song", Title = "Task Song", FileOrder = 0 },
                new Ac15MusicInfoEntry { SongNo = 104, MusicId = "reward_song", Title = "Reward Song", FileOrder = 1 }
            ])
        {
            DonChallenge = new Ac15DonChallengeCatalog(
                enabled: true,
                activeBundleId,
                [
                    new Ac15DonChallengeMonthlyBundle(
                        "white-2016-06",
                        DateTimeOffset.Parse("2016-06-01T00:00:00Z"),
                        DateTimeOffset.Parse("2016-07-01T00:00:00Z"),
                        [
                            new Ac15DonChallengeTask(
                                1001,
                                1,
                                "White Task",
                                new Ac15DonChallengeRule(
                                    Ac15DonChallengeRuleKind.Clear,
                                    RequiredSongCount: 1,
                                    EligibleSongNoes: [101]))
                        ],
                        null,
                        rewards ?? [])
                ])
        };

    private static DonChallengeController CreateController(WhiteHandlerFixture fixture)
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddApplication()
            .AddSingleton(Options.Create(new AuthSettings { AuthenticationRequired = false }))
            .AddScoped<ITaikoDbContext>(_ => fixture.Context)
            .AddScoped<IGameDataCatalog>(_ => fixture.Catalog)
            .BuildServiceProvider();

        return new DonChallengeController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = services }
            }
        };
    }

    private static T AssertOk<T>(ActionResult<T> result)
    {
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        return Assert.IsType<T>(ok.Value);
    }
}
