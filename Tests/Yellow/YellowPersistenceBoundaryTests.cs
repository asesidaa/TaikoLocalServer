using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowPersistenceBoundaryTests
{
    [Fact]
    public async Task YellowSchema_CreatesOnlyPhase14YellowTables()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();

        var tables = await fixture.Context.Database.SqlQueryRaw<string>(
                "SELECT name AS Value FROM sqlite_master WHERE type = 'table' AND name LIKE '%Yellow%' ORDER BY name")
            .ToArrayAsync();

        Assert.Equal(
            [
                "SongBestDatum_Yellow",
                "SongPlayDatum_Yellow",
                "UserSaveData_Yellow",
                "YellowFavoriteSongs",
                "YellowRecentSongs"
            ],
            tables);
    }

    [Fact]
    public async Task GetOrCreateYellowSaveData_CreatesDefaultYellowStateWithAc15Lengths()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "YELLOW" });
        await fixture.Context.SaveChangesAsync();

        var save = await fixture.Context.GetOrCreateYellowSaveDataAsync(1);
        await fixture.Context.SaveChangesAsync();

        var limits = Ac15EraProfiles.Yellow.Limits;
        Assert.Equal(1u, save.Baid);
        Assert.Equal(0u, save.TitleplateId);
        Assert.Equal(0u, save.ColorFace);
        Assert.Equal(1u, save.ColorBody);
        Assert.Equal(3u, save.ColorLimb);
        Assert.Equal(limits.CostumeFlagBytes, save.CostumeFlg1.Length);
        Assert.Equal(limits.CostumeFlagBytes, save.CostumeFlg2.Length);
        Assert.Equal(limits.CostumeFlagBytes, save.CostumeFlg3.Length);
        Assert.Equal(limits.CostumeFlagBytes, save.CostumeFlg4.Length);
        Assert.Equal(limits.CostumeFlagBytes, save.CostumeFlg5.Length);
        Assert.True((save.CostumeFlg1[0] & 1) != 0);
        Assert.Equal(limits.ToneFlagBytes, save.ToneFlg.Length);
        Assert.True((save.ToneFlg[0] & 1) != 0);
        Assert.Equal(limits.TitleFlagBytes, save.TitleFlg.Length);
        Assert.Equal(limits.SongFlagBytes, save.ReleaseSongFlg.Length);
        Assert.Equal(limits.DanFlagBytes, save.GotDanFlg.Length);
        Assert.Equal(limits.DanExtraFlagBytes, save.GotDanExtraFlg.Length);
        Assert.True(save.IsAutoCostumeOn);
        Assert.True(save.IsTojiru);
        Assert.Equal(DateTime.UnixEpoch, save.LastPlayDatetime);
    }

    [Fact]
    public void YellowDbContextContract_ExposesOnlyPhase14DbSets()
    {
        var propertyNames = typeof(ITaikoDbContext).GetProperties()
            .Where(property => property.Name.Contains("Yellow", StringComparison.Ordinal))
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            [
                "SongBestDataYellow",
                "SongPlayDataYellow",
                "UserSaveDataYellow",
                "YellowFavoriteSongs",
                "YellowRecentSongs"
            ],
            propertyNames);
    }

    [Fact]
    public void YellowPersistenceSources_DoNotAddDeferredPhase15Or16Tables()
    {
        var root = FindRepoRoot();
        var sources = Directory.EnumerateFiles(Path.Combine(root, "Domain", "Entities"), "*Yellow*.cs")
            .Concat(Directory.EnumerateFiles(Path.Combine(root, "Infrastructure", "Persistence"), "*Yellow*.cs"))
            .Concat(Directory.EnumerateFiles(Path.Combine(root, "Application", "Abstractions"), "*Yellow*.cs"))
            .Select(File.ReadAllText)
            .Aggregate(string.Empty, string.Concat);

        Assert.DoesNotContain("YellowBattle", sources, StringComparison.Ordinal);
        Assert.DoesNotContain("YellowTokkunStage", sources, StringComparison.Ordinal);
        Assert.DoesNotContain("YellowShopSeason", sources, StringComparison.Ordinal);
        Assert.DoesNotContain("YellowShopItem", sources, StringComparison.Ordinal);
        Assert.DoesNotContain("Banacoin", sources, StringComparison.Ordinal);
        Assert.DoesNotContain("AdminApi", sources, StringComparison.Ordinal);
    }

    private static string FindRepoRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "TaikoLocalServer.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException("Could not find TaikoLocalServer.slnx.");
    }
}
