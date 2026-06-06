# AC15 Core Contracts And Profiles Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add the typed AC15 profile, feature, limit, wire-placement, and hook contracts that later services can depend on.

**Architecture:** Keep the core contracts in `Application/Ac15` and make profile differences explicit data. Default hooks are conservative and do not infer Blue battle, Blue Tokkun, or Green AI battle.

**Tech Stack:** C# 13 records/classes, .NET 10, xUnit.

---

## File Structure

Create:

- `Application/Ac15/Ac15FeatureSet.cs` - explicit client-supported feature flags.
- `Application/Ac15/Ac15ProtocolLimits.cs` - byte widths, course bounds, song counts, and list limits.
- `Application/Ac15/Ac15WirePlacement.cs` - where response data is placed on era wire DTOs.
- `Application/Ac15/Ac15EraProfile.cs` - aggregate profile record.
- `Application/Ac15/IAc15EraHooks.cs` - narrow extension points for special modes and era extras.
- `Application/Ac15/DefaultAc15EraHooks.cs` - default no-special-mode hooks.
- `Application/Ac15/Ac15EraProfiles.cs` - Blue and Green profiles.
- `Tests/Ac15/Ac15EraProfileTests.cs` - profile contract tests.

No generated wire or EF files change in this stage.

## Task 1: Profile Contract Tests

**Files:**
- Create: `Tests/Ac15/Ac15EraProfileTests.cs`

- [ ] **Step 1: Write failing profile tests**

Create `Tests/Ac15/Ac15EraProfileTests.cs`:

```csharp
using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15EraProfileTests
{
    [Fact]
    public void BlueProfile_DeclaresSharedAc15ModulesAndBlueWireExtras()
    {
        var profile = Ac15EraProfiles.Blue;

        Assert.Equal(GameEra.Blue, profile.Era);
        Assert.True(profile.Features.NormalPlay);
        Assert.True(profile.Features.UserData);
        Assert.True(profile.Features.SelfBest);
        Assert.True(profile.Features.Crowns);
        Assert.True(profile.Features.InitialData);
        Assert.True(profile.Features.Folders);
        Assert.True(profile.Features.Telops);
        Assert.True(profile.Features.Taikojuku);
        Assert.True(profile.Features.Dani);
        Assert.True(profile.Features.ItemShop);
        Assert.True(profile.Features.Recommendations);

        Assert.Equal(128, profile.Limits.SongFlagBytes);
        Assert.Equal(1280, profile.Limits.CrownPackedBytes);
        Assert.Equal(5u, profile.Limits.MaxCourseLevel);
        Assert.Equal(5, profile.Limits.MaxFavoriteSongs);
        Assert.Equal(10, profile.Limits.MaxRecentSongs);
        Assert.Equal(Ac15CrownWirePlacement.DedicatedEndpoint, profile.WirePlacement.CrownPlacement);
        Assert.True(profile.WirePlacement.HasTokkunTutorialFlagInUserData);
        Assert.True(profile.WirePlacement.HasInitialDataLegalTermsRows);
    }

    [Fact]
    public void GreenProfile_DeclaresSharedAc15ModulesWithoutBlueExtras()
    {
        var profile = Ac15EraProfiles.Green;

        Assert.Equal(GameEra.Green, profile.Era);
        Assert.True(profile.Features.NormalPlay);
        Assert.True(profile.Features.UserData);
        Assert.True(profile.Features.SelfBest);
        Assert.True(profile.Features.Crowns);
        Assert.True(profile.Features.InitialData);
        Assert.True(profile.Features.Folders);
        Assert.True(profile.Features.Telops);
        Assert.True(profile.Features.Taikojuku);
        Assert.True(profile.Features.Dani);
        Assert.True(profile.Features.ItemShop);
        Assert.True(profile.Features.Recommendations);

        Assert.Equal(128, profile.Limits.SongFlagBytes);
        Assert.Equal(1280, profile.Limits.CrownPackedBytes);
        Assert.Equal(5u, profile.Limits.MaxCourseLevel);
        Assert.Equal(5, profile.Limits.MaxFavoriteSongs);
        Assert.Equal(10, profile.Limits.MaxRecentSongs);
        Assert.Equal(Ac15CrownWirePlacement.DedicatedEndpoint, profile.WirePlacement.CrownPlacement);
        Assert.False(profile.WirePlacement.HasTokkunTutorialFlagInUserData);
        Assert.False(profile.WirePlacement.HasInitialDataLegalTermsRows);
    }

    [Fact]
    public async Task DefaultHooks_DoNotHandleSpecialModes()
    {
        var result = await Ac15EraProfiles.Blue.Hooks.TryHandleSpecialPlayModeAsync(
            new CommonPlayResultData(),
            new Ac15SpecialModeContext(baid: 1, era: GameEra.Blue),
            CancellationToken.None);

        Assert.Equal(Ac15SpecialModeAction.ContinueNormal, result.Action);
        Assert.Equal(1u, result.Result);
    }
}
```

- [ ] **Step 2: Run tests and verify failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15EraProfileTests"
```

Expected: compile failure mentioning missing namespace or types under `TaikoLocalServer.Application.Ac15`.

## Task 2: Core Contract Files

**Files:**
- Create: `Application/Ac15/Ac15FeatureSet.cs`
- Create: `Application/Ac15/Ac15ProtocolLimits.cs`
- Create: `Application/Ac15/Ac15WirePlacement.cs`
- Create: `Application/Ac15/Ac15EraProfile.cs`
- Create: `Application/Ac15/IAc15EraHooks.cs`
- Create: `Application/Ac15/DefaultAc15EraHooks.cs`
- Create: `Application/Ac15/Ac15EraProfiles.cs`

- [ ] **Step 1: Add feature and limit contracts**

Create `Application/Ac15/Ac15FeatureSet.cs`:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15FeatureSet(
    bool NormalPlay,
    bool UserData,
    bool SelfBest,
    bool Crowns,
    bool InitialData,
    bool Folders,
    bool Telops,
    bool Recommendations,
    bool Taikojuku,
    bool Dani,
    bool ItemShop);
```

Create `Application/Ac15/Ac15ProtocolLimits.cs`:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15ProtocolLimits(
    int SongFlagBytes,
    int ToneFlagBytes,
    int TitleFlagBytes,
    int CostumeFlagBytes,
    int DanFlagBytes,
    int DanExtraFlagBytes,
    int ContentInfoBytes,
    int CrownPackedBytes,
    int CrownSongCount,
    int MaxFavoriteSongs,
    int MaxRecentSongs,
    int MaxSongsPerTaikojukuPack,
    int MaxRequestedTaikojukuSlots,
    uint MinCourseLevel,
    uint MaxCourseLevel,
    uint MinNormalDanId,
    uint MaxNormalDanId,
    uint MinExtraDanId,
    uint MaxKnownExtraDanId,
    uint SafeDisplayDanFallback);
```

- [ ] **Step 2: Add wire placement and profile records**

Create `Application/Ac15/Ac15WirePlacement.cs`:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public enum Ac15CrownWirePlacement
{
    Absent = 0,
    DedicatedEndpoint = 1,
    UserData = 2
}

public sealed record Ac15WirePlacement(
    Ac15CrownWirePlacement CrownPlacement,
    bool HasInitialDataItemShopRows,
    bool HasInitialDataLegalTermsRows,
    bool HasTokkunTutorialFlagInUserData);
```

Create `Application/Ac15/Ac15EraProfile.cs`:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15EraProfile(
    GameEra Era,
    Ac15FeatureSet Features,
    Ac15ProtocolLimits Limits,
    Ac15WirePlacement WirePlacement,
    IAc15EraHooks Hooks);
```

- [ ] **Step 3: Add hook contracts**

Create `Application/Ac15/IAc15EraHooks.cs`:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public enum Ac15SpecialModeAction
{
    ContinueNormal = 0,
    Handled = 1
}

public sealed record Ac15SpecialModeResult(Ac15SpecialModeAction Action, uint Result)
{
    public static Ac15SpecialModeResult ContinueNormal(uint result = 1) => new(Ac15SpecialModeAction.ContinueNormal, result);

    public static Ac15SpecialModeResult Handled(uint result = 1) => new(Ac15SpecialModeAction.Handled, result);
}

public sealed record Ac15SpecialModeContext(uint Baid, GameEra Era);

public sealed record Ac15StageSupportDecision(bool IsSupported, string? Reason)
{
    public static Ac15StageSupportDecision Supported { get; } = new(true, null);

    public static Ac15StageSupportDecision Unsupported(string reason) => new(false, reason);
}

public sealed record Ac15NormalSaveContext(uint Baid, GameEra Era, CommonPlayResultData PlayResultData);

public sealed record Ac15UserDataContext(uint Baid, GameEra Era);

public sealed record Ac15InitialDataContext(GameEra Era);

public interface IAc15EraHooks
{
    ValueTask<Ac15SpecialModeResult> TryHandleSpecialPlayModeAsync(
        CommonPlayResultData request,
        Ac15SpecialModeContext context,
        CancellationToken cancellationToken);

    Ac15StageSupportDecision IsSupportedStage(CommonPlayResultData.StageData stage);

    ValueTask BeforeNormalSaveAsync(Ac15NormalSaveContext context, CancellationToken cancellationToken);

    ValueTask AfterNormalSaveAsync(Ac15NormalSaveContext context, CancellationToken cancellationToken);

    void BuildUserDataExtras(CommonUserDataResponse response, Ac15UserDataContext context);

    void BuildInitialDataExtras(CommonInitialDataCheckResponse response, Ac15InitialDataContext context);
}
```

Create `Application/Ac15/DefaultAc15EraHooks.cs`:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public sealed class DefaultAc15EraHooks : IAc15EraHooks
{
    public static DefaultAc15EraHooks Instance { get; } = new();

    private DefaultAc15EraHooks()
    {
    }

    public ValueTask<Ac15SpecialModeResult> TryHandleSpecialPlayModeAsync(
        CommonPlayResultData request,
        Ac15SpecialModeContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult(Ac15SpecialModeResult.ContinueNormal());
    }

    public Ac15StageSupportDecision IsSupportedStage(CommonPlayResultData.StageData stage)
        => stage.StageMode is 0 or 1
            ? Ac15StageSupportDecision.Supported
            : Ac15StageSupportDecision.Unsupported($"stage_mode {stage.StageMode} is not a normal AC15 stage");

    public ValueTask BeforeNormalSaveAsync(Ac15NormalSaveContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.CompletedTask;
    }

    public ValueTask AfterNormalSaveAsync(Ac15NormalSaveContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.CompletedTask;
    }

    public void BuildUserDataExtras(CommonUserDataResponse response, Ac15UserDataContext context)
    {
    }

    public void BuildInitialDataExtras(CommonInitialDataCheckResponse response, Ac15InitialDataContext context)
    {
    }
}
```

- [ ] **Step 4: Add Blue and Green profiles**

Create `Application/Ac15/Ac15EraProfiles.cs`:

```csharp
using TaikoLocalServer.Application.Common;

namespace TaikoLocalServer.Application.Ac15;

public static class Ac15EraProfiles
{
    private static readonly Ac15FeatureSet BlueGreenFeatures = new(
        NormalPlay: true,
        UserData: true,
        SelfBest: true,
        Crowns: true,
        InitialData: true,
        Folders: true,
        Telops: true,
        Recommendations: true,
        Taikojuku: true,
        Dani: true,
        ItemShop: true);

    public static Ac15EraProfile Blue { get; } = new(
        GameEra.Blue,
        BlueGreenFeatures,
        CreateCommonLimits(),
        new Ac15WirePlacement(
            CrownPlacement: Ac15CrownWirePlacement.DedicatedEndpoint,
            HasInitialDataItemShopRows: true,
            HasInitialDataLegalTermsRows: true,
            HasTokkunTutorialFlagInUserData: true),
        DefaultAc15EraHooks.Instance);

    public static Ac15EraProfile Green { get; } = new(
        GameEra.Green,
        BlueGreenFeatures,
        CreateCommonLimits(),
        new Ac15WirePlacement(
            CrownPlacement: Ac15CrownWirePlacement.DedicatedEndpoint,
            HasInitialDataItemShopRows: true,
            HasInitialDataLegalTermsRows: false,
            HasTokkunTutorialFlagInUserData: false),
        DefaultAc15EraHooks.Instance);

    private static Ac15ProtocolLimits CreateCommonLimits() => new(
        SongFlagBytes: BlueProtocolBytes.SongFlagBytes,
        ToneFlagBytes: BlueProtocolBytes.ToneFlagBytes,
        TitleFlagBytes: BlueProtocolBytes.TitleFlagBytes,
        CostumeFlagBytes: BlueProtocolBytes.CostumeFlagBytes,
        DanFlagBytes: BlueProtocolBytes.DanFlagBytes,
        DanExtraFlagBytes: BlueProtocolBytes.DanExtraFlagBytes,
        ContentInfoBytes: BlueProtocolBytes.ContentInfoBytes,
        CrownPackedBytes: BlueProtocolBytes.CrownInflatedBytes,
        CrownSongCount: 1024,
        MaxFavoriteSongs: 5,
        MaxRecentSongs: 10,
        MaxSongsPerTaikojukuPack: 10,
        MaxRequestedTaikojukuSlots: 11,
        MinCourseLevel: 1,
        MaxCourseLevel: 5,
        MinNormalDanId: 1,
        MaxNormalDanId: 25,
        MinExtraDanId: 101,
        MaxKnownExtraDanId: 128,
        SafeDisplayDanFallback: 1);
}
```

- [ ] **Step 5: Run profile tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15EraProfileTests"
```

Expected: PASS.

- [ ] **Step 6: Commit stage 1**

Run:

```powershell
git add Application/Ac15 Tests/Ac15/Ac15EraProfileTests.cs docs/superpowers/plans/2026-06-07-ac15-core-extraction
git commit -m "Add AC15 core profile contracts"
```

