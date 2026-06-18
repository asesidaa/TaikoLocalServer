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

    [Fact]
    public async Task SaveAsync_RejectsUnsupportedOptionGroupWithoutMutation()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        var user = new UserDatum { Baid = 1, MyDonName = "DON" };
        var save = UserSaveDataRedExtensions.CreateDefaultRedSaveData(1);
        save.DispDanType = 0;
        database.Context.UserData.Add(user);
        database.Context.UserSaveDataRed.Add(save);
        await database.Context.SaveChangesAsync();
        var profile = Ac15EraProfiles.Red with
        {
            ProfileCapabilities = Ac15EraProfiles.Red.ProfileCapabilities with
            {
                SupportsDisplayDanOnNamePlate = false
            }
        };

        var result = await Ac15ProfileSettingsService.SaveAsync<UserSaveDataRed, DanScoreDatumRed>(
            user,
            save,
            null,
            profile,
            new Ac15ProfileSettingsUpdateDto(
                new Ac15ProfileIdentityDto("BAD", 0),
                Customization: null,
                Options: new Ac15ProfileOptionGroupsUpdateDto(
                    NamePlate: new Ac15NamePlateOptionsDto(true),
                    Folder: null,
                    SongSelect: null,
                    Taikojuku: null,
                    Tutorials: null,
                    CustomizationBehavior: null)),
            new Ac15ProfileEditPolicy(AllowFreeProfileEditing: true),
            CancellationToken.None);

        Assert.Equal(Ac15ProfileSettingsResultStatus.BadRequest, result.Status);
        Assert.Equal("NamePlate options are not supported by Red.", result.ErrorMessage);
        Assert.Equal("DON", user.MyDonName);
        Assert.Equal(0u, save.DispDanType);
    }

    [Fact]
    public async Task SaveAsync_RejectsUnsupportedCustomizationSlot()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        var user = new UserDatum { Baid = 1, MyDonName = "DON" };
        var save = UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(1);
        database.Context.UserData.Add(user);
        database.Context.UserSaveDataYellow.Add(save);
        await database.Context.SaveChangesAsync();
        var profile = Ac15EraProfiles.Yellow with
        {
            ProfileCapabilities = Ac15EraProfiles.Yellow.ProfileCapabilities with
            {
                CostumeSlots = ["head"]
            }
        };

        var result = await Ac15ProfileSettingsService.SaveAsync<UserSaveDataYellow, DanScoreDatumYellow>(
            user,
            save,
            database.Context.DanScoreDataYellow,
            profile,
            new Ac15ProfileSettingsUpdateDto(
                new Ac15ProfileIdentityDto("BAD", 0),
                new Ac15CustomizationUpdateDto(
                    CostumeSlots:
                    [
                        new Ac15CostumeSlotUpdateDto("puchi", 7, [0, 7])
                    ],
                    Title: null,
                    Tone: null,
                    Colors: null),
                Options: new Ac15ProfileOptionGroupsUpdateDto(null, null, null, null, null, null)),
            new Ac15ProfileEditPolicy(AllowFreeProfileEditing: true),
            CancellationToken.None);

        Assert.Equal(Ac15ProfileSettingsResultStatus.BadRequest, result.Status);
        Assert.Equal("Customization slot 'puchi' is not supported by Yellow.", result.ErrorMessage);
        Assert.Equal(0u, save.Costume5);
    }

    [Fact]
    public async Task SaveAsync_PersistsSupportedGroupsAndLeavesOmittedUnlocksUnchanged()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        var user = new UserDatum { Baid = 1, MyDonName = "DON" };
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        save.DispScoreType = 2;
        save.CostumeFlg1 = BitsetCodec.Encode([0, 5], Ac15EraProfiles.Green.Limits.CostumeFlagBytes);
        database.Context.UserData.Add(user);
        database.Context.UserSaveDataGreen.Add(save);
        database.Context.DanScoreDataGreen.Add(new DanScoreDatumGreen
        {
            Baid = 1,
            DanId = 3,
            IsExtra = false,
            ClearGrade = Ac15DanClearGrade.NotClear
        });
        await database.Context.SaveChangesAsync();

        var result = await Ac15ProfileSettingsService.SaveAsync<UserSaveDataGreen, DanScoreDatumGreen>(
            user,
            save,
            database.Context.DanScoreDataGreen,
            Ac15EraProfiles.Green,
            new Ac15ProfileSettingsUpdateDto(
                new Ac15ProfileIdentityDto("GREEN", 1),
                new Ac15CustomizationUpdateDto(
                    CostumeSlots:
                    [
                        new Ac15CostumeSlotUpdateDto("kigurumi", 12, null)
                    ],
                    Title: new Ac15TitleSelectionUpdateDto("Green Title", 10, [10]),
                    Tone: new Ac15ToneSelectionUpdateDto(4, [0, 4]),
                    Colors: new Ac15CostumeColorsDto(2, 3, 4)),
                Options: new Ac15ProfileOptionGroupsUpdateDto(
                    NamePlate: new Ac15NamePlateOptionsDto(false),
                    Folder: new Ac15FolderOptionsDto(false),
                    SongSelect: new Ac15SongSelectOptionsDto(4, 3),
                    Taikojuku: new Ac15TaikojukuFolderDanUpdateDto(3),
                    Tutorials: new Ac15TutorialOptionsDto(true),
                    CustomizationBehavior: new Ac15CustomizationBehaviorOptionsDto(false))),
            new Ac15ProfileEditPolicy(AllowFreeProfileEditing: false),
            CancellationToken.None);

        Assert.Equal(Ac15ProfileSettingsResultStatus.Success, result.Status);
        Assert.Equal("GREEN", user.MyDonName);
        Assert.Equal(1u, user.MyDonNameLanguage);
        Assert.Equal(12u, save.Costume1);
        Assert.Equal([0u, 5u], BitsetCodec.Decode(save.CostumeFlg1, Ac15EraProfiles.Green.Limits.CostumeFlagBytes));
        Assert.Equal("Green Title", save.Title);
        Assert.Equal(10u, save.TitleplateId);
        Assert.Contains(10u, BitsetCodec.Decode(save.TitleFlg, Ac15EraProfiles.Green.Limits.TitleFlagBytes));
        Assert.Equal(4u, save.DefaultToneSetting);
        Assert.Contains(4u, BitsetCodec.Decode(save.ToneFlg, Ac15EraProfiles.Green.Limits.ToneFlagBytes));
        Assert.Equal(2u, save.ColorBody);
        Assert.Equal(3u, save.ColorFace);
        Assert.Equal(4u, save.ColorLimb);
        Assert.Equal(0u, save.DispDanType);
        Assert.False(save.IsTojiru);
        Assert.False(save.IsAutoCostumeOn);
        Assert.True(save.IsExplain);
        Assert.Equal(4u, save.DispLevelChassis);
        Assert.Equal(3u, save.DispLevelSelf);
        Assert.Equal(3u, save.DispTaikojukuDan);
        Assert.Equal(2u, save.DispScoreType);
    }

    [Fact]
    public async Task SaveAsync_RejectsUnselectableTaikojukuFolderDan()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        var user = new UserDatum { Baid = 1, MyDonName = "DON" };
        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        save.DispTaikojukuDan = 4;
        database.Context.UserData.Add(user);
        database.Context.UserSaveDataBlue.Add(save);
        database.Context.DanScoreDataBlue.Add(new DanScoreDatumBlue
        {
            Baid = 1,
            DanId = 3,
            IsExtra = false,
            ClearGrade = Ac15DanClearGrade.NormalClear
        });
        await database.Context.SaveChangesAsync();

        var result = await Ac15ProfileSettingsService.SaveAsync<UserSaveDataBlue, DanScoreDatumBlue>(
            user,
            save,
            database.Context.DanScoreDataBlue,
            Ac15EraProfiles.Blue,
            new Ac15ProfileSettingsUpdateDto(
                new Ac15ProfileIdentityDto("DON", 0),
                Customization: null,
                Options: new Ac15ProfileOptionGroupsUpdateDto(
                    NamePlate: null,
                    Folder: null,
                    SongSelect: null,
                    Taikojuku: new Ac15TaikojukuFolderDanUpdateDto(3),
                    Tutorials: null,
                    CustomizationBehavior: null)),
            new Ac15ProfileEditPolicy(AllowFreeProfileEditing: true),
            CancellationToken.None);

        Assert.Equal(Ac15ProfileSettingsResultStatus.BadRequest, result.Status);
        Assert.Equal("Taikojuku folder Dan 3 is not selectable for Blue.", result.ErrorMessage);
        Assert.Equal(4u, save.DispTaikojukuDan);
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
