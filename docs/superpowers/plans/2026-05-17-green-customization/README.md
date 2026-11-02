# Green Customization Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add Green-era customization extraction, runtime catalog loading, era-aware profile settings APIs, Green unlock persistence, and reusable WebUI customization controls.

**Architecture:** The implementation keeps Green game-data extraction read-only and writes mod-friendly JSON under `Host/wwwroot/data/green/`. AdminApi becomes an era dispatcher for user settings and exposes catalog-only routes, while WebUI components consume the same shared contracts for Nijiiro and Green. Green item unlock bitsets are translated through one shared `BitsetCodec`, and protocol verification stays grounded in the generated Green protobuf plus IDA evidence.

**Tech Stack:** .NET 10, ASP.NET Core controllers, EF Core/SQLite tests, System.CommandLine, System.Text.Json, Blazor WebAssembly, MudBlazor, protobuf-net, ida-cli for reverse-engineering evidence.

---

## Spec Correction

The spec says to add `TitleFlg` and `ToneFlg` to `BaidQuery.Green`. The checked-in Green protobuf and IDA evidence show `BAIDResponse` has `costume_flg_1..5`, but no `tone_flg` or `title_flg`. Those fields live on `UserDataResponse`:

- `proto/green/green.proto`: `BAIDResponse` fields 20-24 are costume flags only.
- `proto/green/green.proto`: `UserDataResponse` fields 13 and 14 are `tone_flg` and `title_flg`.
- `proto/green/ida-byte-field-findings.md`: `UserDataResponse.tone_flg` is a 16-byte bitset and `UserDataResponse.title_flg` is a 128-byte bitset.

The implementation therefore persists Green tone/title unlocks through `UserSettingsController.Green`, verifies `UserDataQuery.Green` returns them, and does not add nonexistent fields to `BAIDResponse`.

The spec also names `[AuthorizeIfRequired]` for the new catalog endpoint. This repository currently implements the same operator-controlled auth toggle through `[Authorize]` plus `Infrastructure/Identity/AuthAwarePolicyEvaluator.cs`, and existing AdminApi controllers follow that pattern. Use `[Authorize]`; this plan does not introduce a separate `AuthorizeIfRequiredAttribute`.

## Split Plan Layout

This README is the master plan header and index. The numbered Markdown files in this directory are the executable task groups; each one is intentionally separate so an implementation agent can load one stage at a time without carrying the full plan in context.

## File Structure

- `Contracts.AdminApi/ViewModels/Costume.cs`: mark `partial`; keep the shared costume shape.
- `Contracts.AdminApi/ViewModels/Title.cs`: mark `partial`; keep the shared title shape.
- `Contracts.AdminApi/ViewModels/Neiro.cs`: create the shared tone catalog DTO.
- `Contracts.AdminApi/ViewModels/UserSetting.cs`: add `UnlockedTone`.
- `Contracts.AdminApi/ServerData/Green/Costume.Green.cs`: add `Source` provenance to Green costume rows.
- `Contracts.AdminApi/ServerData/Green/Title.Green.cs`: add `Source` provenance to Green title rows.
- `Contracts.AdminApi/ServerData/Green/Neiro.Green.cs`: add `Source` provenance to Green tone rows.
- `Application/Common/BitsetCodec.cs`: reusable byte bitset encode/decode helper.
- `Application/Abstractions/IGreenCatalog.cs`: add customization catalog accessors.
- `Application/Abstractions/INijiiroCatalog.cs`: add `GetNeiroDictionary()` so catalog endpoints are symmetric.
- `Infrastructure/GameDataCatalog/Green/Extractor/*`: read `nutdatapack.ndp`, reward XML, `don3d/`, overrides, merge, and write deterministic JSON.
- `GreenCatalogExtractor/GreenCatalogExtractor.csproj`: CLI project.
- `GreenCatalogExtractor/Program.cs`: System.CommandLine entry point calling the extractor facade.
- `Infrastructure/GameDataCatalog/Green/GreenCostumeLoader.cs`: load `green_costume_data.json`.
- `Infrastructure/GameDataCatalog/Green/GreenTitleLoader.cs`: load `green_title_data.json`.
- `Infrastructure/GameDataCatalog/Green/GreenNeiroLoader.cs`: load `green_neiro_data.json`.
- `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`: auto-bootstrap missing Green customization JSONs and expose loaded catalogs.
- `Application/Settings/ServerSettings.cs`: add `AutoExtractCatalog` and `GameDataPath` to era settings.
- `Adapters.AdminApi/Controllers/UserSettingsController.cs`: convert to era dispatcher.
- `Adapters.AdminApi/Controllers/UserSettingsController.Nijiiro.cs`: move existing Nijiiro behavior.
- `Adapters.AdminApi/Controllers/UserSettingsController.Green.cs`: implement Green settings read/write through `BitsetCodec`.
- `Adapters.AdminApi/Controllers/CustomizationCatalogController.cs`: expose era-aware costumes/titles/neiros catalog routes.
- `Application/Handlers/UserDataQuery.Green.cs`: verify Green `ToneFlg` and `TitleFlg` are sourced from persisted Green save data for the running game.
- `TaikoWebUI/Shared/Customize/*.razor`: reusable customization controls.
- `TaikoWebUI/Shared/Customize/CustomizeValues.cs`: value records used by the controls and profile page.
- `TaikoWebUI/Services/IGameDataService.cs`: add era-aware customization catalog methods.
- `TaikoWebUI/Services/GameDataService.cs`: cache customization catalogs by era.
- `TaikoWebUI/Pages/Profile.razor`: replace the inactive Green customization block and Nijiiro hard-coded customization blocks with shared components.
- `TaikoWebUI/Pages/Profile.razor.cs`: use era-aware user settings/catalog APIs and component value records.
- `.gitignore`: ignore generated Green customization JSONs.
- `Host/README.md`: add the Green customization extraction operator paragraph.
- `Tests/Green/*`: extractor, loader, catalog bootstrap, bitset, AdminApi, and protocol regression tests.

## Stage Files

1. [Contracts and Bitsets](01-contracts-and-bitsets.md)
2. [Extractor Phase 1](02-extractor-phase1.md)
3. [Runtime Catalog Loading](03-runtime-catalog-loading.md)
4. [AdminApi Settings and Catalog Routes](04-adminapi-settings-and-routes.md)
5. [Protocol Unlock Verification](05-protocol-unlock-verification.md)
6. [WebUI Customization Components](06-webui-customization-components.md)
7. [IDA Slot Mapping Enrichment](07-ida-slot-mapping-enrichment.md)
8. [Docs and Final Verification](08-docs-and-final-verification.md)

## Execution Order

Execute stages 1 through 6 for the v1 user-facing feature. Stage 7 is a concrete enrichment stage that can run after v1 using the local IDA snapshot and the `ida-cli` skill. Stage 8 closes the branch after the selected stages are complete.

## Verification Cadence

After each stage:

1. Run the focused tests listed in the stage.
2. Run `dotnet build`.
3. Commit only the files listed in that stage.

Before completion:

1. Run `dotnet test Tests/Tests.csproj`.
2. Run `dotnet build`.
3. Confirm generated catalogs are ignored and not staged.
