# AC15 AdminApi User Settings Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Move AC15 user settings business behavior out of AdminApi controller partials into a switch-free Application service.

**Architecture:** AdminApi controllers keep route parsing, authorization, `NotFound`, `BadRequest`, `Ok`, `NoContent`, and cancellation token propagation. `Ac15UserSettingsService` receives a shared `UserDatum`, an era save row, the bound era Dan table, a typed access record, and protocol limits.

**Tech Stack:** C# 13, .NET 10, ASP.NET Core controllers, EF Core SQLite, Contracts.AdminApi DTOs, xUnit.

---

## File Structure

Create:

- `Application/Ac15/Ac15UserSettingsAccess.cs` - typed save-row field delegates for AC15 settings.
- `Application/Ac15/Ac15UserSettingsService.cs` - shared get/save workflow.
- `Tests/Ac15/Ac15UserSettingsServiceTests.cs` - behavior tests for settings decode, validation, and Dan selection.

Modify:

- `Adapters.AdminApi/Controllers/UserSettingsController.Blue.cs`
- `Adapters.AdminApi/Controllers/UserSettingsController.Green.cs`
- `Adapters.AdminApi/Controllers/UserSettingsController.Yellow.cs`
- Existing Blue, Green, and Yellow AdminApi user settings tests.

## Task 1: Application Service For AC15 User Settings

**Files:**
- Create: `Application/Ac15/Ac15UserSettingsAccess.cs`
- Create: `Application/Ac15/Ac15UserSettingsService.cs`
- Create: `Tests/Ac15/Ac15UserSettingsServiceTests.cs`

- [ ] **Step 1: Write service behavior tests**

Create `Tests/Ac15/Ac15UserSettingsServiceTests.cs`:

```csharp
using TaikoLocalServer.Application.Ac15;
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
```

- [ ] **Step 2: Run service tests and verify compile failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15UserSettingsServiceTests"
```

Expected: compile failure because `Ac15UserSettingsService` and `Ac15UserSettingsAccess` do not exist.

- [ ] **Step 3: Add typed settings access**

Create `Application/Ac15/Ac15UserSettingsAccess.cs`:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15UserSettingsAccess<TSave>(
    Func<TSave, string> GetTitle,
    Action<TSave, string> SetTitle,
    Func<TSave, uint> GetTitlePlateId,
    Action<TSave, uint> SetTitlePlateId,
    Func<TSave, uint> GetToneId,
    Action<TSave, uint> SetToneId,
    Func<TSave, uint> GetCostume1,
    Action<TSave, uint> SetCostume1,
    Func<TSave, uint> GetCostume2,
    Action<TSave, uint> SetCostume2,
    Func<TSave, uint> GetCostume3,
    Action<TSave, uint> SetCostume3,
    Func<TSave, uint> GetCostume4,
    Action<TSave, uint> SetCostume4,
    Func<TSave, uint> GetCostume5,
    Action<TSave, uint> SetCostume5,
    Func<TSave, byte[]> GetCostumeFlg1,
    Action<TSave, byte[]> SetCostumeFlg1,
    Func<TSave, byte[]> GetCostumeFlg2,
    Action<TSave, byte[]> SetCostumeFlg2,
    Func<TSave, byte[]> GetCostumeFlg3,
    Action<TSave, byte[]> SetCostumeFlg3,
    Func<TSave, byte[]> GetCostumeFlg4,
    Action<TSave, byte[]> SetCostumeFlg4,
    Func<TSave, byte[]> GetCostumeFlg5,
    Action<TSave, byte[]> SetCostumeFlg5,
    Func<TSave, byte[]> GetTitleFlg,
    Action<TSave, byte[]> SetTitleFlg,
    Func<TSave, byte[]> GetToneFlg,
    Action<TSave, byte[]> SetToneFlg,
    Func<TSave, uint> GetColorBody,
    Action<TSave, uint> SetColorBody,
    Func<TSave, uint> GetColorFace,
    Action<TSave, uint> SetColorFace,
    Func<TSave, uint> GetColorLimb,
    Action<TSave, uint> SetColorLimb,
    Func<TSave, uint> GetDispDanType,
    Action<TSave, uint> SetDispDanType,
    Func<TSave, uint> GetDispTaikojukuDan,
    Action<TSave, uint> SetDispTaikojukuDan,
    Func<TSave, bool> GetIsTojiru,
    Action<TSave, bool> SetIsTojiru,
    Func<TSave, bool> GetIsAutoCostumeOn,
    Action<TSave, bool> SetIsAutoCostumeOn,
    Func<TSave, uint> GetDispLevelChassis,
    Action<TSave, uint> SetDispLevelChassis,
    Func<TSave, uint> GetDispLevelSelf,
    Action<TSave, uint> SetDispLevelSelf,
    Func<TSave, DateTime> GetLastPlayDatetime,
    int CostumeFlagBytes,
    int TitleFlagBytes,
    int ToneFlagBytes);

public static class Ac15UserSettingsAccess
{
    public static Ac15UserSettingsAccess<UserSaveDataBlue> Blue { get; } = CreateBlue();
    public static Ac15UserSettingsAccess<UserSaveDataGreen> Green { get; } = CreateGreen();
    public static Ac15UserSettingsAccess<UserSaveDataYellow> Yellow { get; } = CreateYellow();

    private static Ac15UserSettingsAccess<UserSaveDataBlue> CreateBlue()
        => new(
            save => save.Title, (save, value) => save.Title = value,
            save => save.TitleplateId, (save, value) => save.TitleplateId = value,
            save => save.DefaultToneSetting, (save, value) => save.DefaultToneSetting = value,
            save => save.Costume1, (save, value) => save.Costume1 = value,
            save => save.Costume2, (save, value) => save.Costume2 = value,
            save => save.Costume3, (save, value) => save.Costume3 = value,
            save => save.Costume4, (save, value) => save.Costume4 = value,
            save => save.Costume5, (save, value) => save.Costume5 = value,
            save => save.CostumeFlg1, (save, value) => save.CostumeFlg1 = value,
            save => save.CostumeFlg2, (save, value) => save.CostumeFlg2 = value,
            save => save.CostumeFlg3, (save, value) => save.CostumeFlg3 = value,
            save => save.CostumeFlg4, (save, value) => save.CostumeFlg4 = value,
            save => save.CostumeFlg5, (save, value) => save.CostumeFlg5 = value,
            save => save.TitleFlg, (save, value) => save.TitleFlg = value,
            save => save.ToneFlg, (save, value) => save.ToneFlg = value,
            save => save.ColorBody, (save, value) => save.ColorBody = value,
            save => save.ColorFace, (save, value) => save.ColorFace = value,
            save => save.ColorLimb, (save, value) => save.ColorLimb = value,
            save => save.DispDanType, (save, value) => save.DispDanType = value,
            save => save.DispTaikojukuDan, (save, value) => save.DispTaikojukuDan = value,
            save => save.IsTojiru, (save, value) => save.IsTojiru = value,
            save => save.IsAutoCostumeOn, (save, value) => save.IsAutoCostumeOn = value,
            save => save.DispLevelChassis, (save, value) => save.DispLevelChassis = value,
            save => save.DispLevelSelf, (save, value) => save.DispLevelSelf = value,
            save => save.LastPlayDatetime,
            BlueProtocolBytes.CostumeFlagBytes,
            BlueProtocolBytes.TitleFlagBytes,
            BlueProtocolBytes.ToneFlagBytes);

    private static Ac15UserSettingsAccess<UserSaveDataGreen> CreateGreen()
        => new(
            save => save.Title, (save, value) => save.Title = value,
            save => save.TitleplateId, (save, value) => save.TitleplateId = value,
            save => save.DefaultToneSetting, (save, value) => save.DefaultToneSetting = value,
            save => save.Costume1, (save, value) => save.Costume1 = value,
            save => save.Costume2, (save, value) => save.Costume2 = value,
            save => save.Costume3, (save, value) => save.Costume3 = value,
            save => save.Costume4, (save, value) => save.Costume4 = value,
            save => save.Costume5, (save, value) => save.Costume5 = value,
            save => save.CostumeFlg1, (save, value) => save.CostumeFlg1 = value,
            save => save.CostumeFlg2, (save, value) => save.CostumeFlg2 = value,
            save => save.CostumeFlg3, (save, value) => save.CostumeFlg3 = value,
            save => save.CostumeFlg4, (save, value) => save.CostumeFlg4 = value,
            save => save.CostumeFlg5, (save, value) => save.CostumeFlg5 = value,
            save => save.TitleFlg, (save, value) => save.TitleFlg = value,
            save => save.ToneFlg, (save, value) => save.ToneFlg = value,
            save => save.ColorBody, (save, value) => save.ColorBody = value,
            save => save.ColorFace, (save, value) => save.ColorFace = value,
            save => save.ColorLimb, (save, value) => save.ColorLimb = value,
            save => save.DispDanType, (save, value) => save.DispDanType = value,
            save => save.DispTaikojukuDan, (save, value) => save.DispTaikojukuDan = value,
            save => save.IsTojiru, (save, value) => save.IsTojiru = value,
            save => save.IsAutoCostumeOn, (save, value) => save.IsAutoCostumeOn = value,
            save => save.DispLevelChassis, (save, value) => save.DispLevelChassis = value,
            save => save.DispLevelSelf, (save, value) => save.DispLevelSelf = value,
            save => save.LastPlayDatetime,
            GreenProtocolBytes.CostumeFlagBytes,
            GreenProtocolBytes.TitleFlagBytes,
            GreenProtocolBytes.ToneFlagBytes);

    private static Ac15UserSettingsAccess<UserSaveDataYellow> CreateYellow()
        => new(
            save => save.Title, (save, value) => save.Title = value,
            save => save.TitleplateId, (save, value) => save.TitleplateId = value,
            save => save.DefaultToneSetting, (save, value) => save.DefaultToneSetting = value,
            save => save.Costume1, (save, value) => save.Costume1 = value,
            save => save.Costume2, (save, value) => save.Costume2 = value,
            save => save.Costume3, (save, value) => save.Costume3 = value,
            save => save.Costume4, (save, value) => save.Costume4 = value,
            save => save.Costume5, (save, value) => save.Costume5 = value,
            save => save.CostumeFlg1, (save, value) => save.CostumeFlg1 = value,
            save => save.CostumeFlg2, (save, value) => save.CostumeFlg2 = value,
            save => save.CostumeFlg3, (save, value) => save.CostumeFlg3 = value,
            save => save.CostumeFlg4, (save, value) => save.CostumeFlg4 = value,
            save => save.CostumeFlg5, (save, value) => save.CostumeFlg5 = value,
            save => save.TitleFlg, (save, value) => save.TitleFlg = value,
            save => save.ToneFlg, (save, value) => save.ToneFlg = value,
            save => save.ColorBody, (save, value) => save.ColorBody = value,
            save => save.ColorFace, (save, value) => save.ColorFace = value,
            save => save.ColorLimb, (save, value) => save.ColorLimb = value,
            save => save.DispDanType, (save, value) => save.DispDanType = value,
            save => save.DispTaikojukuDan, (save, value) => save.DispTaikojukuDan = value,
            save => save.IsTojiru, (save, value) => save.IsTojiru = value,
            save => save.IsAutoCostumeOn, (save, value) => save.IsAutoCostumeOn = value,
            save => save.DispLevelChassis, (save, value) => save.DispLevelChassis = value,
            save => save.DispLevelSelf, (save, value) => save.DispLevelSelf = value,
            save => save.LastPlayDatetime,
            Ac15EraProfiles.Yellow.Limits.CostumeFlagBytes,
            Ac15EraProfiles.Yellow.Limits.TitleFlagBytes,
            Ac15EraProfiles.Yellow.Limits.ToneFlagBytes);
}
```

- [ ] **Step 4: Add user settings service**

Create `Application/Ac15/Ac15UserSettingsService.cs`:

```csharp
using TaikoLocalServer.Contracts.AdminApi.ViewModels;

namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15UserSettingsResult(bool IsSuccess, UserSetting? Setting, string? ErrorMessage)
{
    public static Ac15UserSettingsResult Success(UserSetting? setting = null) => new(true, setting, null);
    public static Ac15UserSettingsResult Error(string message) => new(false, null, message);
}

public static class Ac15UserSettingsService
{
    public static async ValueTask<Ac15UserSettingsResult> GetAsync<TSave, TDanScore>(
        UserDatum user,
        TSave saveData,
        DbSet<TDanScore> danScores,
        Ac15UserSettingsAccess<TSave> access,
        Ac15ProtocolLimits limits,
        CancellationToken cancellationToken)
        where TDanScore : class, IAc15DanScoreDatum
    {
        var selectableTaikojukuDans = await GetSelectableTaikojukuFolderDans(danScores, user.Baid, limits, cancellationToken);
        var taikojukuDan = SelectTaikojukuFolderDan(selectableTaikojukuDans, access.GetDispTaikojukuDan(saveData), limits);

        return Ac15UserSettingsResult.Success(new UserSetting
        {
            Baid = user.Baid,
            ToneId = access.GetToneId(saveData),
            MyDonName = user.MyDonName,
            MyDonNameLanguage = user.MyDonNameLanguage,
            Title = access.GetTitle(saveData),
            TitlePlateId = access.GetTitlePlateId(saveData),
            Kigurumi = access.GetCostume1(saveData),
            Head = access.GetCostume2(saveData),
            Body = access.GetCostume3(saveData),
            Face = access.GetCostume4(saveData),
            Puchi = access.GetCostume5(saveData),
            UnlockedKigurumi = BitsetCodec.Decode(access.GetCostumeFlg1(saveData), access.CostumeFlagBytes),
            UnlockedHead = BitsetCodec.Decode(access.GetCostumeFlg2(saveData), access.CostumeFlagBytes),
            UnlockedBody = BitsetCodec.Decode(access.GetCostumeFlg3(saveData), access.CostumeFlagBytes),
            UnlockedFace = BitsetCodec.Decode(access.GetCostumeFlg4(saveData), access.CostumeFlagBytes),
            UnlockedPuchi = BitsetCodec.Decode(access.GetCostumeFlg5(saveData), access.CostumeFlagBytes),
            UnlockedTitle = BitsetCodec.Decode(access.GetTitleFlg(saveData), access.TitleFlagBytes),
            UnlockedTone = BitsetCodec.Decode(access.GetToneFlg(saveData), access.ToneFlagBytes),
            BodyColor = access.GetColorBody(saveData),
            FaceColor = access.GetColorFace(saveData),
            LimbColor = access.GetColorLimb(saveData),
            IsDisplayDanOnNamePlate = access.GetDispDanType(saveData) != 0,
            GreenTaikojukuDan = taikojukuDan,
            GreenSelectableTaikojukuDans = selectableTaikojukuDans,
            GreenIsTojiru = access.GetIsTojiru(saveData),
            GreenIsAutoCostumeOn = access.GetIsAutoCostumeOn(saveData),
            GreenDispLevelChassis = SafeDisplayLevel(access.GetDispLevelChassis(saveData)),
            GreenDispLevelSelf = SafeDisplayLevel(access.GetDispLevelSelf(saveData)),
            LastPlayDateTime = access.GetLastPlayDatetime(saveData)
        });
    }

    public static async ValueTask<Ac15UserSettingsResult> SaveAsync<TSave, TDanScore>(
        UserDatum user,
        TSave saveData,
        UserSetting request,
        DbSet<TDanScore> danScores,
        Ac15UserSettingsAccess<TSave> access,
        Ac15ProtocolLimits limits,
        CancellationToken cancellationToken)
        where TDanScore : class, IAc15DanScoreDatum
    {
        if (request.GreenDispLevelChassis > 4)
        {
            return Ac15UserSettingsResult.Error("GreenDispLevelChassis must be between 0 and 4.");
        }

        if (request.GreenDispLevelSelf > 4)
        {
            return Ac15UserSettingsResult.Error("GreenDispLevelSelf must be between 0 and 4.");
        }

        user.MyDonName = request.MyDonName;
        user.MyDonNameLanguage = request.MyDonNameLanguage;
        access.SetTitle(saveData, request.Title);
        access.SetTitlePlateId(saveData, request.TitlePlateId);
        access.SetColorBody(saveData, request.BodyColor);
        access.SetColorFace(saveData, request.FaceColor);
        access.SetColorLimb(saveData, request.LimbColor);
        access.SetCostume1(saveData, request.Kigurumi);
        access.SetCostume2(saveData, request.Head);
        access.SetCostume3(saveData, request.Body);
        access.SetCostume4(saveData, request.Face);
        access.SetCostume5(saveData, request.Puchi);
        access.SetToneId(saveData, request.ToneId);
        access.SetDispDanType(saveData, request.IsDisplayDanOnNamePlate ? 1u : 0u);
        access.SetIsTojiru(saveData, request.GreenIsTojiru);
        access.SetIsAutoCostumeOn(saveData, request.GreenIsAutoCostumeOn);
        access.SetDispLevelChassis(saveData, request.GreenDispLevelChassis);
        access.SetDispLevelSelf(saveData, request.GreenDispLevelSelf);

        if (request.GreenTaikojukuDan != 0)
        {
            var selectableTaikojukuDans = await GetSelectableTaikojukuFolderDans(danScores, user.Baid, limits, cancellationToken);
            access.SetDispTaikojukuDan(saveData, SelectTaikojukuFolderDan(selectableTaikojukuDans, request.GreenTaikojukuDan, limits));
        }

        access.SetCostumeFlg1(saveData, EncodeUnlocks(request.UnlockedKigurumi, access.GetCostume1(saveData), access.CostumeFlagBytes));
        access.SetCostumeFlg2(saveData, EncodeUnlocks(request.UnlockedHead, access.GetCostume2(saveData), access.CostumeFlagBytes));
        access.SetCostumeFlg3(saveData, EncodeUnlocks(request.UnlockedBody, access.GetCostume3(saveData), access.CostumeFlagBytes));
        access.SetCostumeFlg4(saveData, EncodeUnlocks(request.UnlockedFace, access.GetCostume4(saveData), access.CostumeFlagBytes));
        access.SetCostumeFlg5(saveData, EncodeUnlocks(request.UnlockedPuchi, access.GetCostume5(saveData), access.CostumeFlagBytes));
        access.SetTitleFlg(saveData, EncodeUnlocks(request.UnlockedTitle, access.GetTitlePlateId(saveData), access.TitleFlagBytes));
        access.SetToneFlg(saveData, EncodeUnlocks(request.UnlockedTone, access.GetToneId(saveData), access.ToneFlagBytes));

        return Ac15UserSettingsResult.Success();
    }

    private static async Task<List<uint>> GetSelectableTaikojukuFolderDans<TDanScore>(
        DbSet<TDanScore> danScores,
        uint baid,
        Ac15ProtocolLimits limits,
        CancellationToken cancellationToken)
        where TDanScore : class, IAc15DanScoreDatum
    {
        var clearGrades = await danScores
            .Where(row => row.Baid == baid && !row.IsExtra)
            .Select(row => new { row.DanId, row.ClearGrade })
            .ToListAsync(cancellationToken);
        var clearGradeMap = clearGrades.ToDictionary(row => row.DanId, row => row.ClearGrade);

        var selectable = new List<uint>();
        for (var danId = limits.MinNormalDanId; danId <= limits.MaxNormalDanId; danId++)
        {
            if (!clearGradeMap.TryGetValue(danId, out var grade) || !Ac15DanHelpers.IsClear(grade))
            {
                selectable.Add(danId);
            }
        }

        return selectable;
    }

    private static uint SelectTaikojukuFolderDan(IReadOnlyList<uint> selectableDans, uint requestedDan, Ac15ProtocolLimits limits)
        => selectableDans.Contains(requestedDan)
            ? requestedDan
            : selectableDans.FirstOrDefault(limits.MinNormalDanId);

    private static uint SafeDisplayLevel(uint value)
        => value <= 4 ? value : 0u;

    private static byte[] EncodeUnlocks(IEnumerable<uint> requestedUnlocks, uint currentId, int bytes)
    {
        var ids = requestedUnlocks.ToHashSet();
        ids.Add(0);
        ids.Add(currentId);
        return BitsetCodec.Encode(ids.OrderBy(id => id).ToList(), bytes);
    }
}
```

- [ ] **Step 5: Run service tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15UserSettingsServiceTests"
```

Expected: PASS.

## Task 2: Replace AdminApi Controller Business Logic

**Files:**
- Modify: `Adapters.AdminApi/Controllers/UserSettingsController.Blue.cs`
- Modify: `Adapters.AdminApi/Controllers/UserSettingsController.Green.cs`
- Modify: `Adapters.AdminApi/Controllers/UserSettingsController.Yellow.cs`

- [ ] **Step 1: Replace Blue controller partial with transport-only calls**

Replace the body of `GetBlueUserSetting` with:

```csharp
private async Task<ActionResult<UserSetting>> GetBlueUserSetting(uint baid)
{
    var user = await context.UserData.Include(d => d.Tokens).FirstOrDefaultAsync(d => d.Baid == baid);
    if (user is null)
    {
        return NotFound();
    }

    var saveData = await context.GetOrCreateBlueSaveDataAsync(baid, HttpContext.RequestAborted);
    var result = await Ac15UserSettingsService.GetAsync(
        user,
        saveData,
        context.DanScoreDataBlue,
        Ac15UserSettingsAccess.Blue,
        Ac15EraProfiles.Blue.Limits,
        HttpContext.RequestAborted);

    return Ok(result.Setting);
}
```

Replace the body of `SaveBlueUserSetting` with:

```csharp
private async Task<IActionResult> SaveBlueUserSetting(uint baid, UserSetting userSetting)
{
    var user = await context.UserData.FindAsync(baid);
    if (user is null)
    {
        return NotFound();
    }

    var saveData = await context.GetOrCreateBlueSaveDataAsync(baid, HttpContext.RequestAborted);
    var result = await Ac15UserSettingsService.SaveAsync(
        user,
        saveData,
        userSetting,
        context.DanScoreDataBlue,
        Ac15UserSettingsAccess.Blue,
        Ac15EraProfiles.Blue.Limits,
        HttpContext.RequestAborted);
    if (!result.IsSuccess)
    {
        return BadRequest(result.ErrorMessage);
    }

    await context.SaveChangesAsync(HttpContext.RequestAborted);
    return NoContent();
}
```

Delete the private Blue helper methods in that file once there are no callers:

```csharp
BuildBlueUserSetting
GetBlueTaikojukuFolderDan
GetBlueSelectableTaikojukuFolderDans
SelectBlueTaikojukuFolderDan
GetSafeBlueDispLevelChassis
GetSafeBlueDispLevelSelf
EncodeBlueCostumeUnlocks
```

- [ ] **Step 2: Replace Green and Yellow controller partials**

Apply the same structure in `UserSettingsController.Green.cs` with:

```csharp
context.GetOrCreateGreenSaveDataAsync(baid, HttpContext.RequestAborted)
context.DanScoreDataGreen
Ac15UserSettingsAccess.Green
Ac15EraProfiles.Green.Limits
```

Apply the same structure in `UserSettingsController.Yellow.cs` with:

```csharp
context.GetOrCreateYellowSaveDataAsync(baid, HttpContext.RequestAborted)
context.DanScoreDataYellow
Ac15UserSettingsAccess.Yellow
Ac15EraProfiles.Yellow.Limits
```

Delete the private Green and Yellow helper methods that duplicate the Blue helper list.

- [ ] **Step 3: Keep shared controller helpers only where Nijiiro still uses them**

In `Adapters.AdminApi/Controllers/UserSettingsController.cs`, keep `SortedDistinctWithZero` only if the Nijiiro partial still calls it. If no controller partial calls it after the AC15 extraction, delete it.

- [ ] **Step 4: Run AdminApi user settings tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~UserSettings"
```

Expected: PASS for Blue, Green, Yellow, and Nijiiro user settings tests.

- [ ] **Step 5: Run AC15 and AdminApi slices**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15UserSettingsServiceTests|FullyQualifiedName~BlueAdminApiParityTests|FullyQualifiedName~GreenAdminApiControllerTests|FullyQualifiedName~YellowAdminApiTests"
```

Expected: PASS.

- [ ] **Step 6: Commit stage 5**

Run:

```powershell
git add Application/Ac15/Ac15UserSettingsAccess.cs Application/Ac15/Ac15UserSettingsService.cs Adapters.AdminApi/Controllers/UserSettingsController.Blue.cs Adapters.AdminApi/Controllers/UserSettingsController.Green.cs Adapters.AdminApi/Controllers/UserSettingsController.Yellow.cs Adapters.AdminApi/Controllers/UserSettingsController.cs Tests/Ac15/Ac15UserSettingsServiceTests.cs Tests/Blue/BlueAdminApiParityTests.cs Tests/Green/GreenAdminApiControllerTests.cs Tests/Yellow/YellowAdminApiTests.cs
git commit -m "Move AC15 user settings behavior into Application"
```

## Self-Review

- Spec coverage: controllers keep HTTP translation; shared service owns AC15 bitsets, save mutation, display-level validation, and selectable Dan calculation.
- Non-goals honored: no controller source-shape tests, no shared save table, no route changes, no Nijiiro behavior migration.
- Type consistency: `Ac15UserSettingsAccess<TSave>`, `Ac15UserSettingsService`, and `Ac15UserSettingsResult` are the stable AdminApi settings names.
