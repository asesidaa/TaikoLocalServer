namespace TaikoLocalServer.Tests.Blue;

public sealed class BlueBattlePersistenceShapeTests
{
    private static readonly (string TypeName, string FileName)[] BattleEntities =
    [
        ("BlueBattleUserState", "BlueBattleUserState.cs"),
        ("BlueBattleNpcState", "BlueBattleNpcState.cs"),
        ("BlueBattleTokenState", "BlueBattleTokenState.cs"),
        ("BlueBattleStageResult", "BlueBattleStageResult.cs")
    ];

    private static readonly (string TypeName, string DbSetName, string TableName, string KeySnippet)[] BattleMappings =
    [
        ("BlueBattleUserState", "BlueBattleUserStates", "BlueBattleUserStates", "entity.HasKey(e => e.Baid);"),
        ("BlueBattleNpcState", "BlueBattleNpcStates", "BlueBattleNpcStates", "entity.HasKey(e => new { e.Baid, e.NpcId });"),
        ("BlueBattleTokenState", "BlueBattleTokenStates", "BlueBattleTokenStates", "entity.HasKey(e => new { e.Baid, e.TokenId });"),
        ("BlueBattleStageResult", "BlueBattleStageResults", "BlueBattleStageResults", "entity.HasKey(e => e.Id);")
    ];

    private static readonly string[] ForbiddenGreenBattleReferences =
    [
        "GreenAiBattle",
        "GreenStageModeInterpreter",
        "GreenGhost",
        "GreenGhostTokens",
        "GreenGhostWinnings",
        "GhostStageSectionDatumGreen",
        "SongBestDatumGreen",
        "SongPlayDatumGreen",
        "UserSaveDataGreen",
        "FunctionIdAiBattleAvailable",
        "UpdateAiBattleData"
    ];

    private static readonly string[] ForbiddenNormalBlueStorageReferences =
    [
        "SongBestDatumBlue",
        "SongPlayDatumBlue",
        "DanScoreDatumBlue",
        "DanStageScoreDatumBlue",
        "BlueFavoriteSongs",
        "BlueRecentSongs",
        "BlueShopSeasonState",
        "BlueShopItemState"
    ];

    [Fact]
    public void BlueBattleEntityFiles_ExistWithBaidScopeAndNullableUnresolvedFields()
    {
        var root = FindRepoRoot();
        foreach (var (typeName, fileName) in BattleEntities)
        {
            var path = Path.Combine(root, "Domain", "Entities", fileName);
            Assert.True(File.Exists(path), $"{fileName} is missing.");

            var source = File.ReadAllText(path);
            Assert.Contains($"public sealed class {typeName}", source, StringComparison.Ordinal);
            Assert.Contains("public uint Baid { get; set; }", source, StringComparison.Ordinal);
            Assert.Contains("public UserDatum? Ba { get; set; }", source, StringComparison.Ordinal);
        }

        AssertEntityContains(root, "BlueBattleUserState.cs",
            "public byte[]? ReleaseInfoFlg { get; set; }",
            "public byte[]? ReleaseBattleStageFlg { get; set; }",
            "public uint? LastBattleStageId { get; set; }",
            "public uint? LastBossLife { get; set; }",
            "public uint? LastNpcId { get; set; }",
            "public uint? AssignStageId { get; set; }",
            "public uint? BattleBondsLvCap { get; set; }");

        AssertEntityContains(root, "BlueBattleNpcState.cs",
            "public uint NpcId { get; set; }",
            "public uint? TotalExp { get; set; }",
            "public uint? MaxDpn { get; set; }",
            "public uint? NpcCostumeId { get; set; }",
            "public byte[]? NpcCostumeFlg { get; set; }",
            "public uint? SelectedSpecialId1 { get; set; }",
            "public uint? SelectedSpecialId2 { get; set; }",
            "public uint? SelectedSpecialId3 { get; set; }",
            "public byte[]? ReleaseSpecialFlg { get; set; }",
            "public uint? BondsLevel { get; set; }");
        Assert.DoesNotContain(
            "public uint? SelectedSpecialId { get; set; }",
            File.ReadAllText(Path.Combine(root, "Domain", "Entities", "BlueBattleNpcState.cs")),
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "MaxDaniPower",
            File.ReadAllText(Path.Combine(root, "Domain", "Entities", "BlueBattleNpcState.cs")),
            StringComparison.Ordinal);

        AssertEntityContains(root, "BlueBattleTokenState.cs",
            "public uint TokenId { get; set; }",
            "public uint? TokenValue { get; set; }");

        AssertEntityContains(root, "BlueBattleStageResult.cs",
            "public long Id { get; set; }",
            "public uint? PlayMode { get; set; }",
            "public uint? StageMode { get; set; }",
            "public uint? NpcId { get; set; }",
            "public uint? Dpn { get; set; }");
        Assert.DoesNotContain(
            "DaniPower",
            File.ReadAllText(Path.Combine(root, "Domain", "Entities", "BlueBattleStageResult.cs")),
            StringComparison.Ordinal);

        Assert.False(
            File.Exists(Path.Combine(root, "Domain", "Entities", "BlueBattleReleaseState.cs")),
            "BlueBattleReleaseState should not exist because release deltas update the byte-array state rows directly.");
    }

    [Fact]
    public void BlueBattleDbContextSources_ExposeBlueOwnedSetsAndBaidMappings()
    {
        var root = FindRepoRoot();
        var interfaceSource = File.ReadAllText(Path.Combine(root, "Application", "Abstractions", "ITaikoDbContext.Blue.cs"));
        var dbContextSource = File.ReadAllText(Path.Combine(root, "Infrastructure", "Persistence", "TaikoDbContext.Blue.cs"));

        foreach (var (typeName, dbSetName, tableName, keySnippet) in BattleMappings)
        {
            Assert.Contains($"DbSet<{typeName}> {dbSetName} {{ get; }}", interfaceSource, StringComparison.Ordinal);
            Assert.Contains($"public virtual DbSet<{typeName}> {dbSetName} {{ get; set; }} = null!;", dbContextSource, StringComparison.Ordinal);

            var mapping = ExtractEntityMapping(dbContextSource, typeName);
            Assert.Contains($"entity.ToTable(\"{tableName}\");", mapping, StringComparison.Ordinal);
            Assert.Contains(keySnippet, mapping, StringComparison.Ordinal);
            Assert.Contains(".HasPrincipalKey(p => p.Baid)", mapping, StringComparison.Ordinal);
            Assert.Contains(".HasForeignKey(d => d.Baid)", mapping, StringComparison.Ordinal);
            Assert.Contains(".OnDelete(DeleteBehavior.Cascade)", mapping, StringComparison.Ordinal);
        }

        Assert.DoesNotContain("BlueBattleReleaseState", interfaceSource, StringComparison.Ordinal);
        Assert.DoesNotContain("BlueBattleReleaseStates", interfaceSource, StringComparison.Ordinal);
        Assert.DoesNotContain("BlueBattleReleaseState", dbContextSource, StringComparison.Ordinal);
        Assert.DoesNotContain("BlueBattleReleaseStates", dbContextSource, StringComparison.Ordinal);
    }

    [Fact]
    public void BlueBattlePersistenceShape_DoesNotUseGreenOrNormalBlueStorageAsBattleStorage()
    {
        var root = FindRepoRoot();
        var sources = BattleEntities
            .Select(entity => File.ReadAllText(Path.Combine(root, "Domain", "Entities", entity.FileName)))
            .ToList();

        var dbContextSource = File.ReadAllText(Path.Combine(root, "Infrastructure", "Persistence", "TaikoDbContext.Blue.cs"));
        sources.AddRange(BattleMappings.Select(mapping => ExtractEntityMapping(dbContextSource, mapping.TypeName)));

        foreach (var source in sources)
        {
            foreach (var forbidden in ForbiddenGreenBattleReferences)
            {
                Assert.DoesNotContain(forbidden, source, StringComparison.Ordinal);
            }

            foreach (var forbidden in ForbiddenNormalBlueStorageReferences)
            {
                Assert.DoesNotContain(forbidden, source, StringComparison.Ordinal);
            }
        }
    }

    private static void AssertEntityContains(string root, string fileName, params string[] snippets)
    {
        var source = File.ReadAllText(Path.Combine(root, "Domain", "Entities", fileName));
        foreach (var snippet in snippets)
        {
            Assert.Contains(snippet, source, StringComparison.Ordinal);
        }
    }

    private static string ExtractEntityMapping(string source, string typeName)
    {
        var startMarker = $"modelBuilder.Entity<{typeName}>(entity =>";
        var start = source.IndexOf(startMarker, StringComparison.Ordinal);
        Assert.True(start >= 0, $"{typeName} mapping is missing.");

        const string endMarker = "\n        });";
        var end = source.IndexOf(endMarker, start, StringComparison.Ordinal);
        Assert.True(end >= 0, $"{typeName} mapping is unterminated.");

        return source[start..(end + endMarker.Length)];
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
