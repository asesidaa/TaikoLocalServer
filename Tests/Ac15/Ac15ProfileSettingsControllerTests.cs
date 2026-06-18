using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TaikoLocalServer.Adapters.AdminApi.Controllers;
using TaikoLocalServer.Infrastructure.Identity.Settings;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15ProfileSettingsControllerTests
{
    [Fact]
    public async Task Get_RejectsNijiiroEra()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        var controller = CreateController(database.Context);

        var result = await controller.Get("Nijiiro", 1);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Unsupported AC15 profile settings era 'Nijiiro'.", badRequest.Value);
    }

    [Fact]
    public async Task Get_BlueReadsBlueProfileOnly()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        database.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        database.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        var blueSave = UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1);
        blueSave.Costume1 = 12;
        database.Context.UserSaveDataBlue.Add(blueSave);
        await database.Context.SaveChangesAsync();
        var controller = CreateController(database.Context);

        var result = await controller.Get("Blue", 1);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var setting = Assert.IsType<Ac15ProfileSettingsDto>(ok.Value);
        Assert.Equal("Blue", setting.Era);
        Assert.Equal(12u, Assert.Single(setting.Customization!.CostumeSlots, slot => slot.Slot == "kigurumi").CurrentId);
    }

    [Fact]
    public async Task Put_RedSavesRedRowsAndDoesNotMutateOtherEras()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        database.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        database.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        database.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        database.Context.UserSaveDataYellow.Add(UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(1));
        var redSave = UserSaveDataRedExtensions.CreateDefaultRedSaveData(1);
        database.Context.UserSaveDataRed.Add(redSave);
        await database.Context.SaveChangesAsync();
        var controller = CreateController(database.Context);

        var result = await controller.Put("Red", 1, new Ac15ProfileSettingsUpdateDto(
            new Ac15ProfileIdentityDto("RED", 1),
            new Ac15CustomizationUpdateDto(
                CostumeSlots:
                [
                    new Ac15CostumeSlotUpdateDto("kigurumi", 7, [0, 7])
                ],
                Title: new Ac15TitleSelectionUpdateDto("Red Title", 10, [10]),
                Tone: new Ac15ToneSelectionUpdateDto(4, [0, 4]),
                Colors: new Ac15CostumeColorsDto(2, 3, 4)),
            new Ac15ProfileOptionGroupsUpdateDto(
                NamePlate: new Ac15NamePlateOptionsDto(false),
                Folder: new Ac15FolderOptionsDto(false),
                SongSelect: new Ac15SongSelectOptionsDto(4, 3),
                Taikojuku: new Ac15TaikojukuFolderDanUpdateDto(1),
                Tutorials: new Ac15TutorialOptionsDto(false),
                CustomizationBehavior: new Ac15CustomizationBehaviorOptionsDto(false))));

        Assert.IsType<NoContentResult>(result);
        Assert.Equal("RED", (await database.Context.UserData.FindAsync(1u))!.MyDonName);
        Assert.Equal(7u, redSave.Costume1);
        Assert.Equal(0u, redSave.DispDanType);
        Assert.Equal(0u, (await database.Context.UserSaveDataBlue.FindAsync(1u))!.Costume1);
        Assert.Equal(0u, (await database.Context.UserSaveDataGreen.FindAsync(1u))!.Costume1);
        Assert.Equal(0u, (await database.Context.UserSaveDataYellow.FindAsync(1u))!.Costume1);
    }

    [Fact]
    public async Task Put_ReturnsBadRequestForUnsupportedSubmittedGroup()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        database.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        database.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await database.Context.SaveChangesAsync();
        var controller = CreateController(database.Context);

        var result = await controller.Put("Blue", 1, new Ac15ProfileSettingsUpdateDto(
            new Ac15ProfileIdentityDto("DON", 0),
            Customization: null,
            Options: new Ac15ProfileOptionGroupsUpdateDto(
                NamePlate: null,
                Folder: null,
                SongSelect: new Ac15SongSelectOptionsDto(5, null),
                Taikojuku: null,
                Tutorials: null,
                CustomizationBehavior: null)));

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Local ranking difficulty must be between 0 and 4.", badRequest.Value);
    }

    [Fact]
    public async Task UserSettingsController_RejectsAc15AfterMigrationAndKeepsNijiiro()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        database.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        database.Context.UserSaveDataNijiiro.Add(UserSaveDataNijiiroExtensions.CreateDefaultNijiiroSaveData(1));
        database.Context.UserSaveDataBlue.Add(UserSaveDataBlueExtensions.CreateDefaultBlueSaveData(1));
        await database.Context.SaveChangesAsync();
        var authSettings = new AuthSettings { AuthenticationRequired = false };
        var services = new ServiceCollection()
            .AddSingleton(Options.Create(authSettings))
            .BuildServiceProvider();
        var controller = new UserSettingsController(database.Context, Options.Create(authSettings))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = services }
            }
        };

        var nijiiro = await controller.GetUserSetting("Nijiiro", 1);
        var blue = await controller.GetUserSetting("Blue", 1);

        Assert.IsType<OkObjectResult>(nijiiro.Result);
        var badRequest = Assert.IsType<BadRequestObjectResult>(blue.Result);
        Assert.Equal("Unsupported game era 'Blue'.", badRequest.Value);
    }

    private static Ac15ProfileSettingsController CreateController(ITaikoDbContext context)
    {
        var authSettings = new AuthSettings { AuthenticationRequired = false, AllowFreeProfileEditing = true };
        var services = new ServiceCollection()
            .AddSingleton(Options.Create(authSettings))
            .BuildServiceProvider();

        return new Ac15ProfileSettingsController(context, Options.Create(authSettings))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = services }
            }
        };
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
