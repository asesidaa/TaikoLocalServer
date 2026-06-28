---
phase: 43-momoiro-adminapi-and-webui-routing
reviewed: 2026-06-28T18:09:14Z
depth: deep
files_reviewed: 20
files_reviewed_list:
  - Adapters.AdminApi/Controllers/Ac15ProfileSettingsController.Momoiro.cs
  - Adapters.AdminApi/Controllers/Ac15ProfileSettingsController.cs
  - Adapters.AdminApi/Controllers/CustomizationCatalogController.cs
  - Adapters.AdminApi/Controllers/GameDataController.cs
  - Application/Abstractions/IMomoiroCatalog.cs
  - Application/Ac15/Ac15EraProfiles.cs
  - Application/Ac15/Ac15ProfileCapabilities.cs
  - Application/Ac15/Ac15ProfileSettingsService.cs
  - Application/Common/UserSaveDataMomoiroExtensions.cs
  - Infrastructure/GameDataCatalog/Momoiro/MomoiroEraGameDataCatalog.cs
  - Infrastructure/GameDataCatalog/Ac15/Ac15CustomizationCatalogSupport.cs
  - Infrastructure/GameDataCatalog/Ac15/Ac15CustomizationCatalogExtractor.cs
  - Infrastructure/GameDataCatalog/Ac15/Ac15CustomizationCatalogComposer.cs
  - TaikoWebUI/Utilities/WebUiEra.cs
  - TaikoWebUI/Pages/ProfileEditor/Ac15ProfileEditor.razor
  - TaikoWebUI/Pages/ProfileEditor/Ac15ProfileEditorState.cs
  - TaikoWebUI/Services/GameDataService.cs
  - Tests/Momoiro/MomoiroAdminApiTests.cs
  - Tests/WebUi/Ac15ProfileSettingsWebUiTests.cs
  - Tests/WebUi/GameDataServiceTests.cs
findings:
  critical: 2
  warning: 1
  info: 0
  total: 3
status: issues_found
---

# Phase 43: Code Review Report

**Reviewed:** 2026-06-28T18:09:14Z
**Depth:** deep
**Files Reviewed:** 20
**Status:** issues_found

## Narrative Findings (AI reviewer)

## Summary

Reviewed the current Momoiro AdminApi/WebUI routing, capability profile, catalog loading path, profile editor exposure, update validation, and tests against `proto/momoiro/taiko.proto` plus the Phase 43 requirements. The implementation wires Momoiro routes, but it ships two user-visible correctness defects: Momoiro customization catalogs are hard-coded empty, and the shared capability profile still exposes at least the folder-close control even though the Momoiro protocol path does not carry it.

## Critical Issues

### CR-01: [BLOCKER] Momoiro customization endpoints always return empty catalogs, hiding supported costume editing

**File:** `Infrastructure/GameDataCatalog/Momoiro/MomoiroEraGameDataCatalog.cs:24`

**Issue:** `MomoiroEraGameDataCatalog` declares `costumeList`, `titleDictionary`, and `neiroDictionary` as readonly empty collections and returns them from the new catalog getters at lines 46-50. `InitializeAsync` never calls `Ac15CustomizationCatalogSupport.EnsureExtractedAsync`, never loads sidecar JSON through `LoadEraCatalogAsync`, and never composes names the way adjacent AC15 catalogs do. The new AdminApi switch arms therefore route `/api/Momoiro/customization/costumes`, `/titles`, and `/neiros` to permanently empty collections. This contradicts the Momoiro proto evidence for costume/current costume fields (`proto/momoiro/taiko.proto:30-44`, `242-252`) and makes the WebUI costume editor unusable because the picker renders only when its catalog is non-empty.

**Fix:**
```csharp
// MomoiroEraGameDataCatalog.cs
public const string CostumeFileName = "momoiro_costume_data.json";
public const string TitleFileName = "momoiro_title_data.json";
public const string NeiroFileName = "momoiro_neiro_data.json";

private IReadOnlyList<Costume> costumeList = [];
private IReadOnlyDictionary<uint, Title> titleDictionary = new Dictionary<uint, Title>();
private IReadOnlyDictionary<uint, Neiro> neiroDictionary = new Dictionary<uint, Neiro>();

// In InitializeAsync, before publishing fields:
await Ac15CustomizationCatalogSupport.EnsureExtractedAsync(
    GameEra.Momoiro,
    momoiroSettings,
    CostumeFileName,
    TitleFileName,
    NeiroFileName,
    logger,
    cancellationToken);

var momoiroCustomization = await Ac15CustomizationCatalogSupport.LoadEraCatalogAsync(
    GameEra.Momoiro,
    CostumeFileName,
    TitleFileName,
    NeiroFileName,
    cancellationToken);

var sharedNames = await Ac15CustomizationCatalogSupport.LoadCustomizationNamesAsync(
    momoiroSettings,
    cancellationToken);

var customizationCatalog = Ac15CustomizationCatalogComposer.Compose(
    momoiroCustomization.Costumes,
    momoiroCustomization.Titles,
    momoiroCustomization.Neiros,
    sharedNames.Costumes,
    sharedNames.Titles,
    sharedNames.Neiros,
    nijiiroCatalog?.GetCostumeList(),
    nijiiroCatalog?.GetTitleDictionary(),
    nijiiroCatalog?.GetNeiroDictionary());

costumeList = customizationCatalog.Costumes;
titleDictionary = customizationCatalog.Titles;
neiroDictionary = customizationCatalog.Neiros;
```

Add the matching constructor dependencies used by neighboring AC15 catalogs (`IOptions<ServerSettings>` and optional `INijiiroCatalog`) and verify with a real `MomoiroEraGameDataCatalog` test, not only a fake catalog.

### CR-02: [BLOCKER] Momoiro exposes and accepts folder-close settings that are not wired to the Momoiro protocol

**File:** `Application/Ac15/Ac15EraProfiles.cs:119`

**Issue:** `Ac15EraProfiles.Momoiro` reuses `Ac15ProfileCapabilities.CurrentWithoutTitlePlateOrTaikojuku`, but that preset only disables title plates and Taikojuku. It still inherits `SupportsFolderCloseButton = true` from `CurrentFull` (`Application/Ac15/Ac15ProfileCapabilities.cs:19-31`). As a result, `Ac15ProfileSettingsService` returns a non-null `Options.Folder` group (`Application/Ac15/Ac15ProfileSettingsService.cs:142-144`), the WebUI renders "Show Folder Close Button" (`TaikoWebUI/Pages/ProfileEditor/Ac15ProfileEditor.razor:29-32`), and PUT validation accepts and persists it (`Application/Ac15/Ac15ProfileSettingsService.cs:185-188`, `320-322`). The Momoiro protocol path does not expose this setting: the Momoiro mapper explicitly ignores `Ac15UserDataDisplaySettings.IsTojiru` when building `UserDataResponse` (`Adapters.GameProtocol.Momoiro/Mappers/UserDataMappers.cs:19-25`), and the Momoiro proto response fields around `proto/momoiro/taiko.proto:363-425` contain no folder-close equivalent.

**Fix:**
```csharp
// Ac15ProfileCapabilities.cs
public static Ac15ProfileCapabilities Momoiro { get; } = CurrentWithoutTitlePlateOrTaikojuku with
{
    SupportsFolderCloseButton = false
};

// Ac15EraProfiles.cs
public static Ac15EraProfile Momoiro { get; } = new(
    GameEra.Momoiro,
    MomoiroFeatures,
    CreateMomoiroLimits(),
    new Ac15WirePlacement(
        CrownPlacement: Ac15CrownWirePlacement.UserData,
        HasInitialDataItemShopRows: false,
        HasInitialDataLegalTermsRows: false,
        HasTokkunTutorialFlagInUserData: false),
    Ac15ProfileCapabilities.Momoiro);
```

Then update the Momoiro profile test to assert `setting.Options.Folder` is null and that a PUT containing `Folder` returns `400 BadRequest` with no `UserSaveDataMomoiro.IsTojiru` mutation.

## Warnings

### WR-01: [WARNING] Tests assert route plumbing and fake data, so the reported Momoiro defects pass

**File:** `Tests/Momoiro/MomoiroAdminApiTests.cs:53`

**Issue:** `Ac15ProfileSettings_Momoiro_ReadsAndSavesMomoiroProfileOnly` submits `Folder`, `Tutorials`, `CustomizationBehavior`, `SongSelect`, `Title`, `Tone`, and `Colors` and expects `NoContent`, then asserts the unsupported folder value was saved at line 78. That test locks in the same over-broad capability behavior that the UI is supposed to hide. The catalog test also uses `MomoiroHandlerFixture.TestMomoiroCatalog` with injected customization rows (`Tests/Momoiro/MomoiroAdminApiTests.cs:208-237`), so it never exercises the real `MomoiroEraGameDataCatalog` that currently returns empty collections. `GameDataServiceTests.CatalogLookups_RequestMomoiroAdminApiRoutes` only checks request paths (`Tests/WebUi/GameDataServiceTests.cs:230-253`) while the fake handler returns empty customization responses for every catalog path (`Tests/WebUi/GameDataServiceTests.cs:408-411`).

**Fix:** Split the tests by contract. One test should prove unsupported Momoiro groups are absent from GET and rejected by PUT, starting with `Folder`. A second test should instantiate or integration-drive `MomoiroEraGameDataCatalog` with temporary Momoiro customization sidecars/extraction input and assert `/api/Momoiro/customization/costumes` returns non-empty, slot-typed entries. The WebUI service test should assert parsed catalog contents for Momoiro, not only the URL list.

---

_Reviewed: 2026-06-28T18:09:14Z_
_Reviewer: the agent (gsd-code-reviewer)_
_Depth: deep_
