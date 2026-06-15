using TaikoLocalServer.Contracts.AdminApi.ViewModels;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15UserSettingsServiceTests
{
    [Fact]
    public async Task GetAsync_DecodesUnlocksAndSelectableUnclearedNormalDans()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        var user = new UserDatum { Baid = 1, MyDonName = "DON", MyDonNameLanguage = 2 };
        var save = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        save.Costume1 = 12;
        save.CostumeFlg1 = Ac15ProtocolBytes.SetBits(save.CostumeFlg1, [12, 13], BlueProtocolBytes.CostumeFlagBytes);
        save.DispTaikojukuDan = 5;
        save.DispScoreType = 1;
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

        var result = await Ac15UserSettingsService.GetAsync(
            user,
            save,
            database.Context.DanScoreDataBlue,
            Ac15UserSettingsAccess.Blue,
            Ac15EraProfiles.Blue.Limits,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Contains(12u, result.Setting!.UnlockedKigurumi);
        Assert.Contains(13u, result.Setting.UnlockedKigurumi);
        Assert.Equal(1u, result.Setting.Ac15DispScoreType);
        Assert.DoesNotContain(5u, result.Setting.GreenSelectableTaikojukuDans);
        Assert.Equal(Ac15EraProfiles.Blue.Limits.MinNormalDanId, result.Setting.GreenTaikojukuDan);
    }

    [Fact]
    public async Task SaveAsync_RejectsInvalidDisplayLevelsWithoutMutation()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        var user = new UserDatum { Baid = 1, MyDonName = "DON" };
        var save = UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(1);
        database.Context.UserData.Add(user);
        database.Context.UserSaveDataYellow.Add(save);
        await database.Context.SaveChangesAsync();

        var result = await Ac15UserSettingsService.SaveAsync(
            user,
            save,
            new UserSetting { MyDonName = "BAD", GreenDispLevelChassis = 5, GreenDispLevelSelf = 0 },
            database.Context.DanScoreDataYellow,
            Ac15UserSettingsAccess.Yellow,
            Ac15EraProfiles.Yellow.Limits,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("GreenDispLevelChassis must be between 0 and 4.", result.ErrorMessage);
        Assert.Equal("DON", user.MyDonName);
    }

    [Fact]
    public async Task SaveAsync_RejectsInvalidScoreTypeWithoutMutation()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        var user = new UserDatum { Baid = 1, MyDonName = "DON" };
        var save = UserSaveDataRedExtensions.CreateDefaultRedSaveData(1);
        save.DispScoreType = 1;
        database.Context.UserData.Add(user);
        database.Context.UserSaveDataRed.Add(save);
        await database.Context.SaveChangesAsync();

        var result = await Ac15UserSettingsService.SaveAsync(
            user,
            save,
            new UserSetting { MyDonName = "BAD", Ac15DispScoreType = 2 },
            database.Context.DanScoreDataRed,
            Ac15UserSettingsAccess.Red,
            Ac15EraProfiles.Red.Limits,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Ac15DispScoreType must be between 0 and 1.", result.ErrorMessage);
        Assert.Equal("DON", user.MyDonName);
        Assert.Equal(1u, save.DispScoreType);
    }

    [Fact]
    public async Task SaveAsync_PersistsCustomizationAndSelectsOnlyValidTaikojukuDan()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        var user = new UserDatum { Baid = 1, MyDonName = "DON" };
        var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
        database.Context.UserData.Add(user);
        database.Context.UserSaveDataGreen.Add(save);
        database.Context.DanScoreDataGreen.Add(new DanScoreDatumGreen
        {
            Baid = 1,
            DanId = 3,
            IsExtra = false,
            ClearGrade = Ac15DanClearGrade.NormalClear
        });
        await database.Context.SaveChangesAsync();

        var result = await Ac15UserSettingsService.SaveAsync(
            user,
            save,
            new UserSetting
            {
                MyDonName = "NEW",
                MyDonNameLanguage = 1,
                Title = "TITLE",
                TitlePlateId = 9,
                Kigurumi = 12,
                ToneId = 4,
                UnlockedKigurumi = [12, 13],
                UnlockedTone = [4, 5],
                GreenTaikojukuDan = 3,
                Ac15DispScoreType = 1,
                GreenDispLevelChassis = 2,
                GreenDispLevelSelf = 1
            },
            database.Context.DanScoreDataGreen,
            Ac15UserSettingsAccess.Green,
            Ac15EraProfiles.Green.Limits,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("NEW", user.MyDonName);
        Assert.Equal(12u, save.Costume1);
        Assert.True(BitIsSet(save.CostumeFlg1, 13));
        Assert.True(BitIsSet(save.ToneFlg, 5));
        Assert.Equal(1u, save.DispScoreType);
        Assert.NotEqual(3u, save.DispTaikojukuDan);
    }

    private static bool BitIsSet(byte[] source, uint id)
        => (source[id >> 3] & (1 << ((int)id & 7))) != 0;

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
