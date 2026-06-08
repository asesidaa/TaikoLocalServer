using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowPersistenceBoundaryTests
{
    [Fact]
    public async Task YellowSchema_CreatesOnlyPhase15DaniYellowTables()
    {
        await using var fixture = await YellowHandlerFixture.CreateAsync();

        var tables = await fixture.Context.Database.SqlQueryRaw<string>(
                "SELECT name AS Value FROM sqlite_master WHERE type = 'table' AND name LIKE '%Yellow%' ORDER BY name")
            .ToArrayAsync();

        var expectedTables = new[]
        {
                "SongBestDatum_Yellow",
                "SongPlayDatum_Yellow",
                "UserSaveData_Yellow",
                "DanScoreDatum_Yellow",
                "DanStageScoreDatum_Yellow",
                "YellowFavoriteSongs",
                "YellowRecentSongs"
        }.Order(StringComparer.Ordinal);

        Assert.Equal(expectedTables, tables);

        Assert.DoesNotContain("DanScoreDatum_Blue", tables);
        Assert.DoesNotContain("DanScoreDatum_Green", tables);
        Assert.DoesNotContain("DanStageScoreDatum_Blue", tables);
        Assert.DoesNotContain("DanStageScoreDatum_Green", tables);
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
    public void YellowDbContextContract_ExposesPhase15DaniDbSets()
    {
        var propertyNames = typeof(ITaikoDbContext).GetProperties()
            .Where(property => property.Name.Contains("Yellow", StringComparison.Ordinal))
            .Select(property => property.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

        var expectedProperties = new[]
        {
                "SongBestDataYellow",
                "SongPlayDataYellow",
                "UserSaveDataYellow",
                "DanScoreDataYellow",
                "DanStageScoreDataYellow",
                "YellowFavoriteSongs",
                "YellowRecentSongs"
        }.Order(StringComparer.Ordinal);

        Assert.Equal(expectedProperties, propertyNames);
    }

    [Fact]
    public void YellowPersistenceSources_DoNotAddDeferredShopTokkunOrBattleTables()
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

    [Fact]
    public void YellowDaniPersistenceSources_DoNotReuseBlueOrGreenDanEntities()
    {
        var root = FindRepoRoot();
        var yellowSources = new[]
            {
                Path.Combine(root, "Application", "Common", "YellowDanHelpers.cs"),
                Path.Combine(root, "Domain", "Entities", "DanScoreDatumYellow.cs"),
                Path.Combine(root, "Domain", "Entities", "DanStageScoreDatumYellow.cs"),
                Path.Combine(root, "Infrastructure", "Persistence", "TaikoDbContext.Yellow.cs"),
                Path.Combine(root, "Application", "Abstractions", "ITaikoDbContext.Yellow.cs")
            }
            .Select(File.ReadAllText)
            .Aggregate(string.Empty, string.Concat);

        Assert.Contains("DanScoreDatumYellow", yellowSources, StringComparison.Ordinal);
        Assert.Contains("DanStageScoreDatumYellow", yellowSources, StringComparison.Ordinal);
        Assert.Contains("DanScoreDatum_Yellow", yellowSources, StringComparison.Ordinal);
        Assert.Contains("DanStageScoreDatum_Yellow", yellowSources, StringComparison.Ordinal);
        Assert.DoesNotContain("DanScoreDataBlue", yellowSources, StringComparison.Ordinal);
        Assert.DoesNotContain("DanScoreDataGreen", yellowSources, StringComparison.Ordinal);
        Assert.DoesNotContain("DanStageScoreDataBlue", yellowSources, StringComparison.Ordinal);
        Assert.DoesNotContain("DanStageScoreDataGreen", yellowSources, StringComparison.Ordinal);
        Assert.DoesNotContain("BlueDanHelpers", yellowSources, StringComparison.Ordinal);
        Assert.DoesNotContain("GreenDanHelpers", yellowSources, StringComparison.Ordinal);
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
