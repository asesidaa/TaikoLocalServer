namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15ProfileSettingsServiceTests
{
    [Fact]
    public async Task GetAsync_ReturnsFullCurrentGroupsAndSlotBasedCustomization()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        var user = new UserDatum { Baid = 1, MyDonName = "DON", MyDonNameLanguage = 2 };
        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        save.Title = "Title";
        save.TitleplateId = 10;
        save.DefaultToneSetting = 4;
        save.Costume1 = 12;
        save.CostumeFlg1 = Ac15ProtocolBytes.SetBits(save.CostumeFlg1, [12, 13], Ac15EraProfiles.Blue.Limits.CostumeFlagBytes);
        save.TitleFlg = Ac15ProtocolBytes.SetBits(save.TitleFlg, [10], Ac15EraProfiles.Blue.Limits.TitleFlagBytes);
        save.ToneFlg = Ac15ProtocolBytes.SetBits(save.ToneFlg, [0, 4], Ac15EraProfiles.Blue.Limits.ToneFlagBytes);
        save.ColorBody = 2;
        save.ColorFace = 3;
        save.ColorLimb = 4;
        save.DispDanType = 1;
        save.IsTojiru = true;
        save.IsAutoCostumeOn = false;
        save.IsExplain = true;
        save.DispLevelChassis = 3;
        save.DispLevelSelf = 2;
        save.DispTaikojukuDan = 5;
        database.Context.UserData.Add(user);
        database.Context.UserSaveDataBlue.Add(save);
        database.Context.DanScoreDataBlue.Add(new DanScoreDatumBlue
        {
            Baid = 1,
            DanId = 5,
            IsExtra = false,
            ClearGrade = Ac15DanClearGrade.NormalClear
        });
        await database.Context.SaveChangesAsync();

        var result = await Ac15ProfileSettingsService.GetAsync<UserSaveDataBlue, DanScoreDatumBlue>(
            user,
            save,
            database.Context.DanScoreDataBlue,
            Ac15EraProfiles.Blue,
            CancellationToken.None);

        Assert.Equal(Ac15ProfileSettingsResultStatus.Success, result.Status);
        var setting = result.Setting!;
        Assert.Equal("Blue", setting.Era);
        Assert.Equal(1u, setting.Baid);
        Assert.Equal("DON", setting.Identity.MyDonName);
        Assert.Equal(2u, setting.Identity.MyDonNameLanguage);
        Assert.NotNull(setting.Customization);
        var kigurumi = Assert.Single(setting.Customization!.CostumeSlots, slot => slot.Slot == "kigurumi");
        Assert.Equal(12u, kigurumi.CurrentId);
        Assert.Contains(13u, kigurumi.UnlockedIds);
        Assert.Equal("Title", setting.Customization.Title!.TitleText);
        Assert.Equal(10u, setting.Customization.Title.TitleId);
        Assert.Equal(4u, setting.Customization.Tone!.ToneId);
        Assert.Equal(2u, setting.Customization.Colors!.BodyColor);
        Assert.True(setting.Options.NamePlate!.DisplayDanOnNamePlate);
        Assert.True(setting.Options.Folder!.ShowFolderCloseButton);
        Assert.False(setting.Options.CustomizationBehavior!.ApplyCostumeChangesFromPlayResults);
        Assert.True(setting.Options.Tutorials!.DisableHowToPlayTutorial);
        Assert.Equal(3u, setting.Options.SongSelect!.LocalRankingDifficulty);
        Assert.Equal(2u, setting.Options.SongSelect.DefaultSelectedAndSelfBestDifficulty);
        Assert.DoesNotContain(5u, setting.Options.Taikojuku!.SelectableFolderDans);
    }

    [Fact]
    public async Task GetAsync_OmitsUnsupportedGroupsFromOlderCapabilityProfile()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        var user = new UserDatum { Baid = 1, MyDonName = "OLDER" };
        var save = UserSaveDataRedExtensions.CreateDefaultRedSaveData(1);
        save.Title = "Older Title";
        save.TitleplateId = 10;
        database.Context.UserData.Add(user);
        database.Context.UserSaveDataRed.Add(save);
        await database.Context.SaveChangesAsync();
        var profile = Ac15EraProfiles.Red with
        {
            ProfileCapabilities = new Ac15ProfileCapabilities(
                CostumeSlots: [],
                SupportsTitle: true,
                SupportsTone: false,
                SupportsColors: false,
                SupportsDisplayDanOnNamePlate: true,
                SupportsFolderCloseButton: false,
                SupportsAutoCostume: false,
                SupportsHowToPlayTutorialFlag: false,
                SupportsLocalRankingDifficulty: false,
                SupportsDefaultSelectedSelfBestDifficulty: false,
                SupportsTaikojukuFolderDan: false)
        };

        var result = await Ac15ProfileSettingsService.GetAsync<UserSaveDataRed, DanScoreDatumRed>(
            user,
            save,
            null,
            profile,
            CancellationToken.None);

        var setting = result.Setting!;
        Assert.NotNull(setting.Customization);
        Assert.Empty(setting.Customization!.CostumeSlots);
        Assert.NotNull(setting.Customization.Title);
        Assert.Null(setting.Customization.Tone);
        Assert.Null(setting.Customization.Colors);
        Assert.NotNull(setting.Options.NamePlate);
        Assert.Null(setting.Options.Folder);
        Assert.Null(setting.Options.SongSelect);
        Assert.Null(setting.Options.Taikojuku);
        Assert.Null(setting.Options.Tutorials);
        Assert.Null(setting.Options.CustomizationBehavior);
    }

    private sealed class SchemaDatabase(SqliteConnection connection) : IAsyncDisposable
    {
        public TaikoDbContext Context { get; } = CreateContext(connection);

        public static async Task<SchemaDatabase> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var database = new SchemaDatabase(connection);
            await database.Context.Database.EnsureCreatedAsync();
            return database;
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await connection.DisposeAsync();
        }

        private static TaikoDbContext CreateContext(SqliteConnection connection)
            => new(new DbContextOptionsBuilder<TaikoDbContext>()
                .UseSqlite(connection)
                .Options);
    }
}
