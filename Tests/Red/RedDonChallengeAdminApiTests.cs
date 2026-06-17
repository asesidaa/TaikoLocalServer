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

namespace TaikoLocalServer.Tests.Red;

public sealed class RedDonChallengeAdminApiTests
{
    [Fact]
    public async Task Availability_Red_ReportsActiveConfiguredBundle()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog());
        var controller = CreateController(fixture);

        var result = await controller.GetAvailability("Red");

        var response = AssertOk<DonChallengeAvailabilityResponse>(result);
        Assert.Equal("Red", response.Era);
        Assert.True(response.IsAvailable);
        Assert.Equal("red-2016-07", response.ActiveBundleId);
        Assert.Equal(DateTimeOffset.Parse("2016-07-01T00:00:00Z"), response.StartsAt);
        Assert.Equal(DateTimeOffset.Parse("2016-08-01T00:00:00Z"), response.EndsAt);
    }

    [Fact]
    public async Task GetDonChallenge_Red_ReturnsConfiguredTasksProgressAndRewardsWithoutOptIn()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog(
            rewards:
            [
                new Ac15DonChallengeReward(1, [102], [10]),
                new Ac15DonChallengeReward(2, [103], [])
            ]));
        AddUser(fixture, donChallengeVisible: false);
        fixture.Context.RedDonChallengeProgress.Add(new RedDonChallengeProgress
        {
            Baid = 1,
            BundleId = "red-2016-07",
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
            UpdatedAt = new DateTime(2016, 7, 20, 12, 0, 0),
            CompletedAt = new DateTime(2016, 7, 20, 12, 0, 0)
        });
        SeedOtherEraRows(fixture);
        await fixture.Context.SaveChangesAsync();
        var controller = CreateController(fixture);

        var result = await controller.GetDonChallenge("Red", 1);

        var response = AssertOk<DonChallengeResponse>(result);
        Assert.Equal("Red", response.Era);
        Assert.True(response.IsAvailable);
        Assert.Equal("red-2016-07", response.BundleId);
        Assert.Equal(1u, response.CompletedTaskCount);
        Assert.Equal(2u, response.PersonalTaskCount);
        Assert.Equal(2, response.Tasks.Count);

        var completedTask = response.Tasks.Single(task => task.TaskId == 1001);
        Assert.Equal(1u, completedTask.Slot);
        Assert.Equal("Task 1", completedTask.Name);
        Assert.Equal("Clear 1 song", completedTask.RuleLabel);
        Assert.Equal(1u, completedTask.ProgressValue);
        Assert.Equal(1u, completedTask.TargetValue);
        Assert.True(completedTask.Completed);
        Assert.Equal(new DateTime(2016, 7, 20, 12, 0, 0), completedTask.CompletedAt);
        Assert.Equal([1u, 2u, 3u, 4u, 5u], completedTask.Tracks.Select(track => track.Level).ToArray());
        Assert.All(completedTask.Tracks, track => Assert.Equal(101u, track.SongNumber));

        var emptyTask = response.Tasks.Single(task => task.TaskId == 1002);
        Assert.Equal(0u, emptyTask.ProgressValue);
        Assert.False(emptyTask.Completed);

        Assert.Equal(DonChallengeRewardStatus.Earned, response.Rewards.Single(reward => reward.RequiredCompletedTasks == 1).Status);
        Assert.Equal(DonChallengeRewardStatus.Locked, response.Rewards.Single(reward => reward.RequiredCompletedTasks == 2).Status);
        Assert.Equal(1, await fixture.Context.SongPlayDataBlue.CountAsync());
        Assert.Equal(1, await fixture.Context.SongPlayDataGreen.CountAsync());
        Assert.Equal(1, await fixture.Context.SongPlayDataYellow.CountAsync());
        Assert.False((await fixture.Context.UserSaveDataRed.SingleAsync(row => row.Baid == 1)).IsChallengeCompe);
    }

    [Fact]
    public async Task GetDonChallenge_Red_TreatsConfiguredRewardFlagsAsEarnedWithoutProgress()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog(
            rewards: [new Ac15DonChallengeReward(2, [102], [10])]));
        AddUser(fixture, donChallengeVisible: false);
        var save = await fixture.Context.UserSaveDataRed.SingleAsync(row => row.Baid == 1);
        save.ReleaseSongFlg = Ac15ProtocolBytes.SetBits(save.ReleaseSongFlg, [102], Ac15EraProfiles.Red.Limits.SongFlagBytes);
        save.TitleFlg = Ac15ProtocolBytes.SetBits(save.TitleFlg, [10], Ac15EraProfiles.Red.Limits.TitleFlagBytes);
        await fixture.Context.SaveChangesAsync();
        var controller = CreateController(fixture);

        var result = await controller.GetDonChallenge("Red", 1);

        var response = AssertOk<DonChallengeResponse>(result);
        Assert.Equal(0u, response.CompletedTaskCount);
        Assert.Equal(DonChallengeRewardStatus.Earned, Assert.Single(response.Rewards).Status);
    }

    [Fact]
    public async Task GetDonChallenge_UnsupportedEraReturnsUnavailableWithoutRedFallback()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog(
            rewards: [new Ac15DonChallengeReward(1, [102], [])]));
        AddUser(fixture, donChallengeVisible: true);
        fixture.Context.RedDonChallengeProgress.Add(new RedDonChallengeProgress
        {
            Baid = 1,
            BundleId = "red-2016-07",
            TaskId = 1001,
            Slot = 1,
            CompeId = 1001,
            TrackNo = 0,
            SongNo = 101,
            Level = 1,
            StageMode = 0,
            ProgressValue = 1,
            Completed = true,
            UpdatedAt = new DateTime(2016, 7, 20, 12, 0, 0),
            CompletedAt = new DateTime(2016, 7, 20, 12, 0, 0)
        });
        await fixture.Context.SaveChangesAsync();
        var controller = CreateController(fixture);

        var availability = AssertOk<DonChallengeAvailabilityResponse>(await controller.GetAvailability("Blue"));
        var readback = AssertOk<DonChallengeResponse>(await controller.GetDonChallenge("Blue", 1));

        Assert.False(availability.IsAvailable);
        Assert.Equal("Blue", availability.Era);
        Assert.Contains("not available", availability.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(readback.IsAvailable);
        Assert.Equal("Blue", readback.Era);
        Assert.Empty(readback.Tasks);
        Assert.Empty(readback.Rewards);
        Assert.Equal(1, await fixture.Context.RedDonChallengeProgress.CountAsync());
    }

    [Fact]
    public async Task GetDonChallenge_Red_NoActiveBundleReturnsReadableUnavailableResponse()
    {
        await using var fixture = await RedHandlerFixture.CreateAsync(CreateCatalog(activeBundleId: null));
        AddUser(fixture, donChallengeVisible: true);
        var controller = CreateController(fixture);

        var availability = AssertOk<DonChallengeAvailabilityResponse>(await controller.GetAvailability("Red"));
        var readback = AssertOk<DonChallengeResponse>(await controller.GetDonChallenge("Red", 1));

        Assert.False(availability.IsAvailable);
        Assert.Equal("No active Don Challenge is configured for Red.", availability.Message);
        Assert.False(readback.IsAvailable);
        Assert.Equal("No active Don Challenge is configured for Red.", readback.Message);
        Assert.Empty(readback.Tasks);
        Assert.Empty(readback.Rewards);
    }

    private static void AddUser(RedHandlerFixture fixture, bool donChallengeVisible)
    {
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        var save = UserSaveDataRedExtensions.CreateDefaultRedSaveData(1);
        save.IsChallengeCompe = donChallengeVisible;
        fixture.Context.UserSaveDataRed.Add(save);
        fixture.Context.SaveChanges();
    }

    private static RedHandlerFixture.TestRedCatalog CreateCatalog(
        string? activeBundleId = "red-2016-07",
        IReadOnlyList<Ac15DonChallengeReward>? rewards = null)
        => new(
            musicInfoFileOrder:
            [
                new Ac15MusicInfoEntry { SongNo = 101, MusicId = "task_song", Title = "Task Song", FileOrder = 0 },
                new Ac15MusicInfoEntry { SongNo = 102, MusicId = "reward_song", Title = "Reward Song", FileOrder = 1 },
                new Ac15MusicInfoEntry { SongNo = 103, MusicId = "locked_song", Title = "Locked Song", FileOrder = 2 }
            ])
        {
            DonChallenge = new Ac15DonChallengeCatalog(
                enabled: true,
                activeBundleId,
                [
                    new Ac15DonChallengeMonthlyBundle(
                        "red-2016-07",
                        DateTimeOffset.Parse("2016-07-01T00:00:00Z"),
                        DateTimeOffset.Parse("2016-08-01T00:00:00Z"),
                        [
                            new Ac15DonChallengeTask(
                                1001,
                                1,
                                "Task 1",
                                new Ac15DonChallengeRule(
                                    Ac15DonChallengeRuleKind.Clear,
                                    RequiredSongCount: 1,
                                    EligibleSongNoes: [101])),
                            new Ac15DonChallengeTask(
                                1002,
                                2,
                                "Task 2",
                                new Ac15DonChallengeRule(
                                    Ac15DonChallengeRuleKind.FullCombo,
                                    RequiredSongCount: 2,
                                    EligibleSongNoes: [102, 103]))
                        ],
                        null,
                        rewards ?? [])
                ])
        };

    private static DonChallengeController CreateController(RedHandlerFixture fixture)
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

    private static void SeedOtherEraRows(RedHandlerFixture fixture)
    {
        fixture.Context.SongPlayDataBlue.Add(new SongPlayDatumBlue
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Oni,
            Score = 999999,
            PlayTime = new DateTime(2016, 7, 20, 12, 0, 0)
        });
        fixture.Context.SongPlayDataGreen.Add(new SongPlayDatumGreen
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Oni,
            Score = 888888,
            PlayTime = new DateTime(2016, 7, 20, 12, 0, 0)
        });
        fixture.Context.SongPlayDataYellow.Add(new SongPlayDatumYellow
        {
            Baid = 1,
            SongId = 101,
            Difficulty = Difficulty.Oni,
            Score = 777777,
            PlayTime = new DateTime(2016, 7, 20, 12, 0, 0)
        });
    }
}
