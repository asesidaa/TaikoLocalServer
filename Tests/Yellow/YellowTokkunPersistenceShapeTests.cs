namespace TaikoLocalServer.Tests.Yellow;

public sealed class YellowTokkunPersistenceShapeTests
{
    private static readonly string[] ForbiddenTokkunStorageReferences =
    [
        "Blue",
        "Green",
        "Nijiiro",
        "SongBestDatumYellow",
        "SongPlayDatumYellow",
        "DanScoreDatumYellow",
        "DanStageScoreDatumYellow",
        "YellowFavoriteSongs",
        "YellowRecentSongs",
        "YellowShop",
        "Battle",
        "Reward",
        "Unlock",
        "Payment",
        "BanacoinPayment",
        "Balance",
        "Coupon",
        "Receipt",
        "Bnid",
        "Chid",
        "UploadedAtUtc",
        "CreatedAt"
    ];

    [Fact]
    public void YellowTokkunEntityFile_ExistsWithProtocolBackedFieldsOnly()
    {
        var root = FindRepoRoot();
        var path = Path.Combine(root, "Domain", "Entities", "YellowTokkunStageResult.cs");
        Assert.True(File.Exists(path), "YellowTokkunStageResult.cs is missing.");

        var source = File.ReadAllText(path);
        Assert.Contains("public sealed class YellowTokkunStageResult", source, StringComparison.Ordinal);
        Assert.Contains("public long Id { get; set; }", source, StringComparison.Ordinal);
        Assert.Contains("public uint Baid { get; set; }", source, StringComparison.Ordinal);
        Assert.Contains("public string PlayDatetime { get; set; }", source, StringComparison.Ordinal);
        Assert.Contains("public uint PlayMode { get; set; }", source, StringComparison.Ordinal);
        Assert.Contains("public string BanacoinDatetime { get; set; }", source, StringComparison.Ordinal);
        Assert.Contains("public uint TokkunSongCnt { get; set; }", source, StringComparison.Ordinal);
        Assert.Contains("public string TookunSongnoesJson { get; set; }", source, StringComparison.Ordinal);
        Assert.Contains("public uint TokkunSpeedchangeCnt { get; set; }", source, StringComparison.Ordinal);
        Assert.Contains("public uint TokkunAutoplayCnt { get; set; }", source, StringComparison.Ordinal);
        Assert.Contains("public uint TokkunJumpCnt { get; set; }", source, StringComparison.Ordinal);
        Assert.Contains("public UserDatum? Ba { get; set; }", source, StringComparison.Ordinal);

        AssertNoForbiddenReferences(source);
    }

    [Fact]
    public void UserSaveDataYellow_TokkunTutorialFlagIsNullableAndNotDefaulted()
    {
        var root = FindRepoRoot();
        var saveSource = File.ReadAllText(Path.Combine(root, "Domain", "Entities", "UserSaveDataYellow.cs"));
        var defaultsSource = File.ReadAllText(Path.Combine(root, "Application", "Common", "UserSaveDataYellowExtensions.cs"));

        Assert.Contains("public uint? TokkunTutorialFlg { get; set; }", saveSource, StringComparison.Ordinal);
        Assert.DoesNotContain("TokkunTutorialFlg =", defaultsSource, StringComparison.Ordinal);
    }

    [Fact]
    public void YellowTokkunDbContextSources_ExposeYellowOwnedSetAndBaidMapping()
    {
        var root = FindRepoRoot();
        var interfaceSource = File.ReadAllText(Path.Combine(root, "Application", "Abstractions", "ITaikoDbContext.Yellow.cs"));
        var dbContextSource = File.ReadAllText(Path.Combine(root, "Infrastructure", "Persistence", "TaikoDbContext.Yellow.cs"));

        Assert.Contains("DbSet<YellowTokkunStageResult> YellowTokkunStageResults { get; }", interfaceSource, StringComparison.Ordinal);
        Assert.Contains(
            "public virtual DbSet<YellowTokkunStageResult> YellowTokkunStageResults { get; set; } = null!;",
            dbContextSource,
            StringComparison.Ordinal);

        var mapping = ExtractEntityMapping(dbContextSource, "YellowTokkunStageResult");
        Assert.Contains("entity.ToTable(\"YellowTokkunStageResults\");", mapping, StringComparison.Ordinal);
        Assert.Contains("entity.HasKey(e => e.Id);", mapping, StringComparison.Ordinal);
        Assert.Contains("entity.Property(e => e.Id).ValueGeneratedOnAdd();", mapping, StringComparison.Ordinal);
        Assert.Contains("entity.HasIndex(e => new { e.Baid, e.PlayDatetime });", mapping, StringComparison.Ordinal);
        Assert.Contains(".HasPrincipalKey(p => p.Baid)", mapping, StringComparison.Ordinal);
        Assert.Contains(".HasForeignKey(d => d.Baid)", mapping, StringComparison.Ordinal);
        Assert.Contains(".OnDelete(DeleteBehavior.Cascade)", mapping, StringComparison.Ordinal);

        AssertNoForbiddenReferences(mapping);
    }

    private static void AssertNoForbiddenReferences(string source)
    {
        foreach (var forbidden in ForbiddenTokkunStorageReferences)
        {
            Assert.DoesNotContain(forbidden, source, StringComparison.Ordinal);
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
