# AC15 Profile Settings - Design

**Date:** 2026-06-18
**Status:** brainstorm-approved draft
**Scope:** Replace the AC15 use of the mixed `UserSetting` AdminApi/WebUI contract with an AC15-only profile settings contract that supports 11 AC15 eras implemented newest-to-oldest, while leaving Nijiiro/AC16 profile editing unchanged.

## Purpose

AC15 profile settings currently reuse `Contracts.AdminApi.ViewModels.UserSetting`, which also carries Nijiiro/AC16 settings. The AC15 portion grew from Green support, so several fields still have Green-specific names even though Blue, Yellow, Red, and future older AC15 eras use the same AdminApi/WebUI surface.

That model does not scale to all AC15 eras. Green is the newest AC15 era and has the widest current profile surface. Older eras generally have fewer options, and rare non-shared options should remain explicit instead of becoming always-present no-op fields.

This design creates a separate AC15 profile settings resource. It models profile editing as capability groups, not as a latest-era superset. Nijiiro/AC16 keeps the existing `UserSetting` contract and route behavior.

## Relationship To Existing Designs

This spec follows the AC15 capability-composition rule from `docs/superpowers/specs/2026-06-11-ac15-capability-composition-design.md`:

- share behavior through capability-first modules;
- keep era routes, wire DTOs, and EF tables era-owned;
- bind concrete tables and policies at the era edge;
- do not make shared AC15 modules switch on `GameEra` internally.

This spec supersedes the AdminApi user-settings portion of that design where it kept `UserSetting` as the AC15 DTO and used `Ac15UserSettingsAccess<TSave>` as a large delegate adapter.

## Goals

- Keep Nijiiro/AC16 profile editing unchanged.
- Add an AC15-only AdminApi contract and WebUI model.
- Represent AC15 profile settings as optional typed capability groups.
- Return only capability groups supported by the selected era.
- Reject unsupported groups or invalid values on update.
- Support all 11 AC15 eras without growing a latest-era superset DTO.
- Keep frontend rendering driven by returned AC15 groups/capabilities, not by hardcoded era names.
- Remove the delegate-heavy `Ac15UserSettingsAccess<TSave>` shape in favor of narrow save-data capability interfaces.

## Non-Goals

- Do not redesign Nijiiro/AC16 `UserSetting`.
- Do not make a universal profile DTO spanning AC15 and AC16.
- Do not expose unsupported older-era options as no-op or defaulted fields.
- Do not add a runtime form schema language for profile options.
- Do not create 11 separate per-era profile DTOs when behavior is shared.
- Do not merge AC15 persistence tables.
- Do not infer older-era capabilities from newer eras without evidence.

## Core Rule

`GET` defines exactly what `PUT` may submit for an era.

If a profile option group is absent from the read DTO for an era, the WebUI must not render it and must not submit it. If the update request includes an unsupported group or field, the server returns `400 Bad Request`.

There is no compatibility requirement between independently deployed frontend and backend versions. The WebUI is paired with the server in this repo, so strict validation is preferred over silent ignore behavior.

## API

Add an AC15-only route family:

```text
GET /api/{era}/Ac15ProfileSettings/{baid}
PUT /api/{era}/Ac15ProfileSettings/{baid}
```

`{era}` must resolve to an implemented AC15 era. Nijiiro/AC16 is rejected for these routes.

The existing routes remain Nijiiro/AC16-owned:

```text
GET  /api/UserSettings/{baid}
POST /api/UserSettings/{baid}
GET  /api/{era}/UserSettings/{baid}
POST /api/{era}/UserSettings/{baid}
```

AC15 WebUI code should move to `Ac15ProfileSettings`. Existing AC15 usage of `UserSettings` can be removed once the WebUI migration is complete.

## Read Contract

Suggested namespace:

```text
Contracts.AdminApi.Ac15ProfileSettings
```

Top-level DTO:

```csharp
public sealed record Ac15ProfileSettingsDto(
    string Era,
    uint Baid,
    Ac15ProfileIdentityDto Identity,
    Ac15CustomizationDto? Customization,
    Ac15ProfileOptionGroupsDto Options,
    Ac15ProfileCapabilitiesDto Capabilities,
    DateTime LastPlayDateTime);
```

Identity:

```csharp
public sealed record Ac15ProfileIdentityDto(
    string MyDonName,
    uint MyDonNameLanguage);
```

Option groups:

```csharp
public sealed record Ac15ProfileOptionGroupsDto(
    Ac15NamePlateOptionsDto? NamePlate,
    Ac15FolderOptionsDto? Folder,
    Ac15SongSelectOptionsDto? SongSelect,
    Ac15TaikojukuOptionsDto? Taikojuku,
    Ac15TutorialOptionsDto? Tutorials,
    Ac15CustomizationBehaviorOptionsDto? CustomizationBehavior);

public sealed record Ac15NamePlateOptionsDto(
    bool DisplayDanOnNamePlate);

public sealed record Ac15FolderOptionsDto(
    bool ShowFolderCloseButton);

public sealed record Ac15SongSelectOptionsDto(
    uint? LocalRankingDifficulty,
    uint? DefaultSelectedAndSelfBestDifficulty);

public sealed record Ac15TaikojukuOptionsDto(
    uint FolderDan,
    IReadOnlyList<uint> SelectableFolderDans);

public sealed record Ac15TutorialOptionsDto(
    bool? DisableHowToPlayTutorial);

public sealed record Ac15CustomizationBehaviorOptionsDto(
    bool ApplyCostumeChangesFromPlayResults);
```

The read DTO uses null groups to mean unsupported capability. Nullable fields inside a present group are reserved for cases where the group is real but only part of the group exists in a specific era. Prefer splitting groups before adding many nullable fields.

## Customization Contract

Customization should not preserve Green-shaped fields such as `Kigurumi`, `Head`, `Body`, `Face`, and `Puchi` at the top level. AC15 profile editing should use slot-based customization so older eras can expose fewer slots.

```csharp
public sealed record Ac15CustomizationDto(
    IReadOnlyList<Ac15CostumeSlotDto> CostumeSlots,
    Ac15TitleSelectionDto? Title,
    Ac15ToneSelectionDto? Tone,
    Ac15CostumeColorsDto? Colors);

public sealed record Ac15CostumeSlotDto(
    string Slot,
    uint CurrentId,
    IReadOnlyList<uint> UnlockedIds);

public sealed record Ac15TitleSelectionDto(
    string TitleText,
    uint TitleId,
    IReadOnlyList<uint> UnlockedTitleIds);

public sealed record Ac15ToneSelectionDto(
    uint ToneId,
    IReadOnlyList<uint> UnlockedToneIds);

public sealed record Ac15CostumeColorsDto(
    uint BodyColor,
    uint FaceColor,
    uint LimbColor);
```

Known slot names for current AC15 eras:

```text
kigurumi
head
body
face
puchi
```

The server returns only supported slots. The WebUI renders slot pickers from the returned slot list and catalog data.

## Update Contract

Use a separate update DTO so read-only helper data is not submitted back.

```csharp
public sealed record Ac15ProfileSettingsUpdateDto(
    Ac15ProfileIdentityDto Identity,
    Ac15CustomizationUpdateDto? Customization,
    Ac15ProfileOptionGroupsUpdateDto Options);
```

Update option groups:

```csharp
public sealed record Ac15ProfileOptionGroupsUpdateDto(
    Ac15NamePlateOptionsDto? NamePlate,
    Ac15FolderOptionsDto? Folder,
    Ac15SongSelectOptionsDto? SongSelect,
    Ac15TaikojukuFolderDanUpdateDto? Taikojuku,
    Ac15TutorialOptionsDto? Tutorials,
    Ac15CustomizationBehaviorOptionsDto? CustomizationBehavior);

public sealed record Ac15TaikojukuFolderDanUpdateDto(
    uint FolderDan);
```

Update customization:

```csharp
public sealed record Ac15CustomizationUpdateDto(
    IReadOnlyList<Ac15CostumeSlotUpdateDto>? CostumeSlots,
    Ac15TitleSelectionUpdateDto? Title,
    Ac15ToneSelectionUpdateDto? Tone,
    Ac15CostumeColorsDto? Colors);

public sealed record Ac15CostumeSlotUpdateDto(
    string Slot,
    uint CurrentId,
    IReadOnlyList<uint>? UnlockedIds);

public sealed record Ac15TitleSelectionUpdateDto(
    string TitleText,
    uint TitleId,
    IReadOnlyList<uint>? UnlockedTitleIds);

public sealed record Ac15ToneSelectionUpdateDto(
    uint ToneId,
    IReadOnlyList<uint>? UnlockedToneIds);
```

Omitted supported groups mean leave unchanged. Submitted unsupported groups return `400 Bad Request`.

Unlock list semantics:

- If free profile editing is enabled, submitted unlock lists replace the stored unlock set after adding required defaults/current selections.
- If free profile editing is disabled, submitted unlock lists are ignored or rejected consistently with the existing authorization policy. The preferred implementation is to reject unlock-list edits when the caller lacks permission, while still allowing current selection if already unlocked.

## Capabilities

Capabilities are server-owned metadata derived from `Ac15EraProfiles` and evidence-backed era behavior.

```csharp
public sealed record Ac15ProfileCapabilitiesDto(
    IReadOnlyList<string> CostumeSlots,
    bool SupportsTitle,
    bool SupportsTone,
    bool SupportsColors,
    bool SupportsDisplayDanOnNamePlate,
    bool SupportsFolderCloseButton,
    bool SupportsAutoCostume,
    bool SupportsHowToPlayTutorialFlag,
    bool SupportsLocalRankingDifficulty,
    bool SupportsDefaultSelectedSelfBestDifficulty,
    bool SupportsTaikojukuFolderDan);
```

Capability metadata is not a dynamic form schema. It is used for frontend rendering decisions, validation, and tests. The typed DTO groups remain the source of profile contract shape.

Example newest-era capability set:

```csharp
new Ac15ProfileCapabilitiesDto(
    CostumeSlots: ["kigurumi", "head", "body", "face", "puchi"],
    SupportsTitle: true,
    SupportsTone: true,
    SupportsColors: true,
    SupportsDisplayDanOnNamePlate: true,
    SupportsFolderCloseButton: true,
    SupportsAutoCostume: true,
    SupportsHowToPlayTutorialFlag: true,
    SupportsLocalRankingDifficulty: true,
    SupportsDefaultSelectedSelfBestDifficulty: true,
    SupportsTaikojukuFolderDan: true);
```

Example older-era capability set:

```csharp
new Ac15ProfileCapabilitiesDto(
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
    SupportsTaikojukuFolderDan: false);
```

## Application Service Shape

Replace `Ac15UserSettingsAccess<TSave>` with narrow save-data interfaces.

```csharp
public interface IAc15ProfileSettingsSaveData :
    IAc15CustomizationSaveData,
    IAc15PlayProfileSaveData
{
    string Title { get; set; }
    uint TitleplateId { get; set; }
    uint DefaultToneSetting { get; set; }
    uint ColorBody { get; set; }
    uint ColorFace { get; set; }
    uint ColorLimb { get; set; }
    uint DispDanType { get; set; }
    uint DispTaikojukuDan { get; set; }
    bool IsTojiru { get; set; }
    bool IsAutoCostumeOn { get; set; }
    bool IsExplain { get; set; }
    uint DispLevelChassis { get; set; }
    uint DispLevelSelf { get; set; }
}
```

Service shape:

```csharp
public static class Ac15ProfileSettingsService
{
    public static ValueTask<Ac15ProfileSettingsResult> GetAsync<TSave, TDanScore>(
        UserDatum user,
        TSave saveData,
        DbSet<TDanScore>? danScores,
        Ac15EraProfile profile,
        CancellationToken cancellationToken)
        where TSave : class, IAc15ProfileSettingsSaveData
        where TDanScore : class, IAc15DanScoreDatum;

    public static ValueTask<Ac15ProfileSettingsResult> SaveAsync<TSave, TDanScore>(
        UserDatum user,
        TSave saveData,
        DbSet<TDanScore>? danScores,
        Ac15EraProfile profile,
        Ac15ProfileSettingsUpdateDto request,
        Ac15ProfileEditPolicy policy,
        CancellationToken cancellationToken)
        where TSave : class, IAc15ProfileSettingsSaveData
        where TDanScore : class, IAc15DanScoreDatum;
}
```

The era controller partial remains the composition root. It binds:

- the concrete save row factory;
- the concrete Dan score table, or null when unsupported;
- the `Ac15EraProfile`;
- the edit policy derived from authentication/free-editing settings.

The shared service does not switch on `GameEra`.

## Controller Shape

Controller partials should stay transport-only:

```csharp
[ApiController]
[Route("api/{era}/[controller]")]
public sealed partial class Ac15ProfileSettingsController(
    ITaikoDbContext context,
    IOptions<AuthSettings> authOptions) : BaseAdminController<Ac15ProfileSettingsController>
{
    [HttpGet("{baid}")]
    public Task<ActionResult<Ac15ProfileSettingsDto>> Get(string era, uint baid);

    [HttpPut("{baid}")]
    public Task<IActionResult> Put(string era, uint baid, Ac15ProfileSettingsUpdateDto request);
}
```

Routing rule:

- Parse `{era}` through `EraRoute.TryParse`.
- Reject non-AC15 eras.
- Dispatch to era partial methods for save-row binding.
- Translate application results to `Ok`, `BadRequest`, `NotFound`, `Forbid`, or `NoContent`.

## WebUI Shape

Keep the existing Nijiiro path on the existing model:

```csharp
var nijiiro = await Client.GetFromJsonAsync<UserSetting>(
    WebUiEra.Api(CurrentEra, $"UserSettings/{Baid}"));
```

Use the new AC15 path only for AC15 eras:

```csharp
var ac15 = await Client.GetFromJsonAsync<Ac15ProfileSettingsDto>(
    WebUiEra.Api(CurrentEra, $"Ac15ProfileSettings/{Baid}"));
```

The profile page can branch at the top:

```razor
@if (IsAc15)
{
    <Ac15ProfileEditor Model="@ac15Model" />
}
else
{
    <NijiiroProfileEditor Model="@nijiiroModel" />
}
```

AC15 editor components should render by group presence:

```razor
@if (Model.Options.NamePlate is not null)
{
    <Ac15NamePlateOptionsEditor Value="@Model.Options.NamePlate" />
}

@if (Model.Options.SongSelect is not null)
{
    <Ac15SongSelectOptionsEditor Value="@Model.Options.SongSelect" />
}

@if (Model.Customization is not null)
{
    <Ac15CustomizationEditor Value="@Model.Customization" />
}
```

The WebUI should submit one whole AC15 profile document per save. Per-section endpoints are not needed unless identity, customization, or option edits later require separate permissions or audit trails.

## Validation Rules

- Non-AC15 era on `Ac15ProfileSettings` returns `400`.
- Unsupported submitted option group returns `400`.
- Unsupported submitted customization slot returns `400`.
- Submitted Taikojuku folder Dan must be selectable for the user and era, or return `400`.
- Display difficulty values must be in the era-supported range.
- Read-only helper data such as selectable Dan lists is never accepted in update DTOs.
- Unknown JSON fields should be rejected for this endpoint if practical without changing global JSON behavior.

## Testing Strategy

Tests should protect behavior and boundaries, not source shape.

Useful tests:

- Green/Blue current capabilities return the expected option groups.
- An older AC15 test fixture with fewer capabilities omits unsupported groups.
- PUT rejects a group not supported by the era.
- PUT rejects an unsupported customization slot.
- PUT persists identity, customization, and supported option changes for an existing era.
- PUT does not mutate unsupported state.
- Nijiiro `UserSettings` behavior remains unchanged.
- `Ac15ProfileSettings` rejects Nijiiro.
- WebUI AC15 editor renders from group presence and does not reference `Green*` DTO fields.

Avoid tests that assert controller attribute inventory, source text, or the existence of generated/wire members.

## Migration Plan

1. Add AC15 contracts and capability metadata.
2. Add `IAc15ProfileSettingsSaveData` to implemented AC15 save entities.
3. Add `Ac15ProfileSettingsService` and move settings behavior out of `Ac15UserSettingsService`.
4. Add `Ac15ProfileSettingsController` with Green, Blue, Yellow, and Red bindings.
5. Port AC15 WebUI profile editing to `Ac15ProfileSettingsDto`.
6. Keep Nijiiro WebUI editing on `UserSetting`.
7. Remove AC15 fields from `UserSetting` after the WebUI no longer consumes them.
8. Delete `Ac15UserSettingsAccess<TSave>` after no callers remain.
9. Add future older eras by binding their save rows and enabling only proven capabilities.

## Open Questions

- Should unlock-list edits be rejected when the caller lacks free-editing permission, or should they be ignored while still allowing current selection changes within already-unlocked IDs? The stricter and clearer choice is rejection.
- Should JSON unknown-field rejection be endpoint-local or global? The safer initial choice is endpoint-local.
- Should capability profiles live inside `Ac15EraProfile` directly or as a nested `Ac15ProfileCapabilities` value? The cleaner shape is a nested value owned by `Ac15EraProfile`.
