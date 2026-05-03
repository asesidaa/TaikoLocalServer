# PR4 — Contracts + WebUI + Documentation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Carve out a `TaikoLocalServer.Contracts.AdminApi` project containing every DTO/ViewModel/Request/Response that the admin REST API exchanges with TaikoWebUI, swap the WebUI off `SharedProject` and onto the new contracts project, delete the now-empty `SharedProject`, delete the legacy `TaikoWebUI/TaikoWebUI.sln`, and rewrite `CLAUDE.md` + skim top-level `README.md` so the documentation matches the new layout.

**Architecture:** This PR finalizes the hexagonal layout. After this PR, the dependency graph is:

- `Domain` (no deps)
- `Contracts.AdminApi` → Domain (for `Domain.Enums`)
- `Application` → Domain, Contracts.AdminApi
- `Infrastructure` → Application
- `Adapters.AdminApi` → Application, Contracts.AdminApi
- `Adapters.AllnetMucha` → Application
- `Adapters.GameProtocol.Shared` → (none beyond ASP.NET)
- `Adapters.GameProtocol.WwR08` → Application, Adapters.GameProtocol.Shared
- `Adapters.GameProtocol.CnR00` → Application, Adapters.GameProtocol.Shared
- `Host` → all Adapters + Infrastructure
- `TaikoWebUI` → Contracts.AdminApi
- `LocalSaveModScoreMigrator` → Infrastructure (or whatever PR2 wired it to)

`SharedProject/` is deleted. The single source of truth for what the admin API and the WebUI exchange is `Contracts.AdminApi/`.

**Tech Stack:** unchanged (.NET 10, Blazor WebAssembly with MudBlazor 9, the same JSON shapes — only namespaces change).

**Spec reference:** `docs/superpowers/specs/2026-05-04-clean-architecture-refactor-design.md` — see §3 (per-project file lists, Contracts.AdminApi line items), §6 (CLAUDE.md rewrite scope), §7 (PR4 verification gates), §8 risks #10–#13.

**Prerequisites:** PR3 (`dev/clean-arch-adapters`) is merged into `dev`. Run:

```bash
git checkout dev
git pull --ff-only origin dev
```

Confirm `dotnet build` succeeds against the 12-project solution that PR3 produced (Domain, Application, Infrastructure, Adapters.AdminApi, Adapters.AllnetMucha, Adapters.GameProtocol.{Shared, WwR08, CnR00}, Host, SharedProject, TaikoWebUI, LocalSaveModScoreMigrator). `GameDatabase` was already replaced by `Infrastructure` in PR2.

> If after PR2/PR3 the actual `.slnx` count differs from 12, treat that as the new baseline — PR4's net change is 0 (SharedProject removed, Contracts.AdminApi added).

---

## How this plan works

This PR is **mostly file relocation, csproj rewiring, and documentation**. There is one non-trivial nuance: the WebUI consumes a small set of "JSON-shape" types that PR2 placed in `Application/ServerData/` (DanData, MusicDetail, etc.). Phase 4.6 relocates those into `Contracts.AdminApi/ServerData/` so the WebUI doesn't have to reach into Application. After that move, both server-side file-load and admin-API serialization use the same canonical types from Contracts.AdminApi.

Verification rhythm: build green at the end of every phase. Final smoke walks every WebUI page that exists today (Dashboard, Login, Register, Users, Profile, AccessCode, ChangePassword, HighScores, PlayHistory, SongList, Song, DaniDojo) and exercises a Login + SetFavorite round-trip end-to-end through the JWT auth flow.

All commands assume the working directory is the repo root (`D:\TaikoLocalServer\`).

## Final layout this PR produces

```
Domain/                                  (unchanged)
Application/                             (ServerData/ folder loses 2 types — DanData, MusicDetail —
                                          which move to Contracts.AdminApi; the rest stay)
Infrastructure/                          (unchanged)

Adapters.AdminApi/                       (ProjectReference adds Contracts.AdminApi)
Adapters.AllnetMucha/                    (unchanged)
Adapters.GameProtocol.Shared/            (unchanged)
Adapters.GameProtocol.WwR08/             (unchanged)
Adapters.GameProtocol.CnR00/             (unchanged)
Host/                                    (unchanged)

Contracts.AdminApi/                      ← NEW
  Contracts.AdminApi.csproj              <RootNamespace>=TaikoLocalServer.Contracts.AdminApi
                                         ProjectReference: ..\Domain\Domain.csproj
                                         (no PackageReferences)
  ViewModels/                            (13 files moved from SharedProject/Models/)
    User.cs, UserCredential.cs, UserSetting.cs,
    SongBestData.cs, SongHistoryData.cs, SongLeaderboard.cs, SongPlayDatumDto.cs,
    DanBestData.cs, DanBestStageData.cs, AiSectionBestData.cs,
    Title.cs, Costume.cs, PlaySetting.cs
  Requests/                              (8 files moved from SharedProject/Models/Requests/)
    BindAccessCodeRequest.cs, ChangePasswordRequest.cs, GenerateOtpRequest.cs,
    LoginRequest.cs, RegisterRequest.cs, ResetPasswordRequest.cs,
    SetFavoriteRequest.cs, VerifyOtpRequest.cs
  Responses/                             (5 files moved from SharedProject/Models/Responses/)
    DanBestDataResponse.cs, SongBestResponse.cs, SongHistoryResponse.cs,
    SongLeaderboardResponse.cs, UsersResponse.cs
  ServerData/                            (2 files moved from Application/ServerData/ —
                                          the ones the WebUI also consumes)
    DanData.cs, MusicDetail.cs
  Converters/
    PlaySettingConverter.cs              (moved from SharedProject/Utils/)
  GlobalUsings.cs

TaikoWebUI/                              (csproj swaps SharedProject ref → Contracts.AdminApi;
                                          GlobalUsings.cs + _Imports.razor swap namespaces)

SharedProject/                           ← DELETED (folder + csproj + .slnx entry gone)

TaikoWebUI/TaikoWebUI.sln                ← DELETED (legacy duplicate solution file)

CLAUDE.md                                ← REWRITTEN (7 sections updated; see Phase 4.10)
README.md                                ← skimmed for stale paths (Phase 4.11)
```

Final `.slnx` project count: 12 — same as post-PR3 (SharedProject removed, Contracts.AdminApi added → net change 0).

> **Sanity check:** PR3 ends at 12 projects (Domain, Application, Infrastructure, 5 Adapters, Host, TaikoWebUI, SharedProject, LocalSaveModScoreMigrator). PR4 ends at 12 too — replace SharedProject with Contracts.AdminApi → net change 0. The test in Phase 4.12 is "exactly one entry per non-empty project folder, no `SharedProject` entry, exactly one `Contracts.AdminApi` entry."

---

## Phase 4.0 — Branch and baseline

### Task 4.0.1: Create the PR4 branch

**Files:** none.

- [ ] **Step 1: Create and check out the PR4 branch**

```bash
git checkout dev
git pull --ff-only origin dev
git checkout -b dev/clean-arch-contracts-webui
```

- [ ] **Step 2: Sanity check the post-PR3 layout**

```bash
ls Domain/ Application/ Infrastructure/ Adapters.AdminApi/ Adapters.AllnetMucha/ Adapters.GameProtocol.Shared/ Adapters.GameProtocol.WwR08/ Adapters.GameProtocol.CnR00/ Host/ SharedProject/ TaikoWebUI/ LocalSaveModScoreMigrator/
```

Expected: every directory exists. `SharedProject/` still has `Models/` (with what PR1 + PR2 left behind), `Models/Requests/`, `Models/Responses/`, `Utils/PlaySettingConverter.cs`, and `SharedProject.csproj` — and nothing else of substance.

- [ ] **Step 3: Inventory what's left in SharedProject**

```bash
ls SharedProject/
ls SharedProject/Models/
ls SharedProject/Models/Requests/
ls SharedProject/Models/Responses/
ls SharedProject/Utils/
ls SharedProject/Enums/ 2>/dev/null || echo "(empty — moved in PR1)"
```

Expected output (ignoring `bin/` and `obj/`):

- `SharedProject/Models/`: 13 .cs files — `User.cs`, `UserCredential.cs`, `UserSetting.cs`, `SongBestData.cs`, `SongHistoryData.cs`, `SongLeaderboard.cs`, `SongPlayDatumDto.cs`, `DanBestData.cs`, `DanBestStageData.cs`, `AiSectionBestData.cs`, `Title.cs`, `Costume.cs`, `PlaySetting.cs`
- `SharedProject/Models/Requests/`: 8 files
- `SharedProject/Models/Responses/`: 5 files
- `SharedProject/Utils/`: `PlaySettingConverter.cs` only (PathHelper moved in PR2, ValueHelpers moved in PR2)
- `SharedProject/Enums/`: empty or absent (moved in PR1)

If the inventory differs (e.g., PathHelper still present), stop and audit PR2 — do not patch around it in PR4.

- [ ] **Step 4: Capture pre-flight build green**

```bash
dotnet build
```

Expected: 0 errors, 0 warnings (or only the same warnings PR3 ended with).

- [ ] **Step 5: Capture WebUI page list for the smoke gate**

```bash
ls TaikoWebUI/Pages/*.razor
```

Expected (alphabetical): `AccessCode.razor`, `ChangePassword.razor`, `DaniDojo.razor`, `Dashboard.razor`, `HighScores.razor`, `Login.razor`, `PlayHistory.razor`, `Profile.razor`, `Register.razor`, `Song.razor`, `SongList.razor`, `Users.razor` — 12 pages. Final smoke (Phase 4.12) walks each.

- [ ] **Step 6: No commit yet**

The remaining phases each end with their own commit.

---

## Phase 4.1 — Create Contracts.AdminApi project

### Task 4.1.1: Scaffold the project

**Files:**
- Create: `Contracts.AdminApi/Contracts.AdminApi.csproj`
- Create: `Contracts.AdminApi/GlobalUsings.cs`

- [ ] **Step 1: Create the directory**

```bash
mkdir -p Contracts.AdminApi
```

- [ ] **Step 2: Write `Contracts.AdminApi/Contracts.AdminApi.csproj`**

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <RootNamespace>TaikoLocalServer.Contracts.AdminApi</RootNamespace>
    <AssemblyName>TaikoLocalServer.Contracts.AdminApi</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Domain\Domain.csproj" />
  </ItemGroup>

</Project>
```

Notes:
- `Contracts.AdminApi` references **only** `Domain` (for `TaikoLocalServer.Domain.Enums.*` like `RandomType`, `Difficulty`, `CrownType`, `ScoreRank`, `DanClearState`, `SongGenre`).
- No `PackageReference` blocks — pure DTO contracts. CPM (`Directory.Packages.props`) is inherited and will not error on an empty package list.
- `Throw` is **not** added; the previous `SharedProject.csproj` had `<PackageReference Include="Throw" />` but that was used by `PlaySettingConverter.cs` only. Phase 4.5 keeps the `Throw` reference there (we'll add it).

- [ ] **Step 3: Write `Contracts.AdminApi/GlobalUsings.cs`**

```csharp
global using TaikoLocalServer.Domain.Enums;
```

- [ ] **Step 4: Add the project to `.slnx`**

Edit `TaikoLocalServer.slnx`. Insert the line for Contracts.AdminApi alphabetically among the existing entries:

```xml
<Solution>
  <!-- ...existing entries from PR3... -->
  <Project Path="Contracts.AdminApi/Contracts.AdminApi.csproj" />
  <!-- ...existing entries from PR3... -->
</Solution>
```

(Match whatever ordering convention PR3 left in the file — alphabetical by path is the usual convention in this repo.)

- [ ] **Step 5: Build to confirm the empty project compiles**

```bash
dotnet build Contracts.AdminApi/Contracts.AdminApi.csproj
```

Expected: 0 errors. Warning about no source files is fine; the next phases populate it.

- [ ] **Step 6: Commit**

```bash
git add Contracts.AdminApi TaikoLocalServer.slnx
git commit -m "PR4.1: scaffold Contracts.AdminApi project (refs Domain only)"
```

---

## Phase 4.2 — Move ViewModels

### Task 4.2.1: Move 13 ViewModel files into `Contracts.AdminApi/ViewModels/`

**Files:**
- Move: `SharedProject/Models/{User, UserCredential, UserSetting, SongBestData, SongHistoryData, SongLeaderboard, SongPlayDatumDto, DanBestData, DanBestStageData, AiSectionBestData, Title, Costume, PlaySetting}.cs` → `Contracts.AdminApi/ViewModels/`

- [ ] **Step 1: Create the target directory**

```bash
mkdir -p Contracts.AdminApi/ViewModels
```

- [ ] **Step 2: Move the 13 files**

```bash
git mv SharedProject/Models/User.cs                Contracts.AdminApi/ViewModels/User.cs
git mv SharedProject/Models/UserCredential.cs      Contracts.AdminApi/ViewModels/UserCredential.cs
git mv SharedProject/Models/UserSetting.cs         Contracts.AdminApi/ViewModels/UserSetting.cs
git mv SharedProject/Models/SongBestData.cs        Contracts.AdminApi/ViewModels/SongBestData.cs
git mv SharedProject/Models/SongHistoryData.cs     Contracts.AdminApi/ViewModels/SongHistoryData.cs
git mv SharedProject/Models/SongLeaderboard.cs     Contracts.AdminApi/ViewModels/SongLeaderboard.cs
git mv SharedProject/Models/SongPlayDatumDto.cs    Contracts.AdminApi/ViewModels/SongPlayDatumDto.cs
git mv SharedProject/Models/DanBestData.cs         Contracts.AdminApi/ViewModels/DanBestData.cs
git mv SharedProject/Models/DanBestStageData.cs    Contracts.AdminApi/ViewModels/DanBestStageData.cs
git mv SharedProject/Models/AiSectionBestData.cs   Contracts.AdminApi/ViewModels/AiSectionBestData.cs
git mv SharedProject/Models/Title.cs               Contracts.AdminApi/ViewModels/Title.cs
git mv SharedProject/Models/Costume.cs             Contracts.AdminApi/ViewModels/Costume.cs
git mv SharedProject/Models/PlaySetting.cs         Contracts.AdminApi/ViewModels/PlaySetting.cs
```

- [ ] **Step 3: Rewrite namespaces inside each moved file**

For every moved file, change `namespace SharedProject.Models;` → `namespace TaikoLocalServer.Contracts.AdminApi.ViewModels;`.

For `PlaySetting.cs` specifically, also remove the `using SharedProject.Enums;` line — it's no longer needed because `Contracts.AdminApi/GlobalUsings.cs` already exposes `TaikoLocalServer.Domain.Enums` globally.

Resulting `Contracts.AdminApi/ViewModels/PlaySetting.cs`:

```csharp
namespace TaikoLocalServer.Contracts.AdminApi.ViewModels;

public class PlaySetting
{
    public uint Speed { get; set; }

    public bool IsVanishOn { get; set; }

    public bool IsInverseOn { get; set; }

    public RandomType RandomType { get; set; }
}
```

Resulting `Contracts.AdminApi/ViewModels/User.cs`:

```csharp
namespace TaikoLocalServer.Contracts.AdminApi.ViewModels;

public class User
{
    public uint Baid { get; set; }

    public List<string> AccessCodes { get; set; } = new();

    public bool IsAdmin { get; set; }

    public UserSetting UserSetting { get; set; } = new();
}
```

For the rest, apply the namespace rewrite verbatim. If an existing file imports types via `using SharedProject.Models;` or `using SharedProject.Enums;`, replace those with the corresponding `TaikoLocalServer.Contracts.AdminApi.ViewModels;` or `TaikoLocalServer.Domain.Enums;` (or rely on global usings) — but only for the moved files; we update the consumers in Phase 4.7 / 4.8.

- [ ] **Step 4: Rebuild Contracts.AdminApi to confirm files compile**

```bash
dotnet build Contracts.AdminApi/Contracts.AdminApi.csproj
```

Expected: 0 errors. Warnings about unused types in the assembly are fine.

- [ ] **Step 5: Build the rest of the solution and confirm it breaks at consumers**

```bash
dotnet build
```

Expected: many errors of the form `error CS0234: The type or namespace name 'User' does not exist in the namespace 'SharedProject.Models'`. These are **expected and will be fixed in Phase 4.7 (Adapters.AdminApi) and Phase 4.8 (TaikoWebUI)**. Don't chase them here.

- [ ] **Step 6: Commit**

```bash
git add Contracts.AdminApi/ViewModels SharedProject
git commit -m "PR4.2: move 13 admin-API ViewModels from SharedProject to Contracts.AdminApi"
```

---

## Phase 4.3 — Move Requests

### Task 4.3.1: Move 8 Request DTOs into `Contracts.AdminApi/Requests/`

**Files:**
- Move: `SharedProject/Models/Requests/*.cs` → `Contracts.AdminApi/Requests/`

- [ ] **Step 1: Create the target directory**

```bash
mkdir -p Contracts.AdminApi/Requests
```

- [ ] **Step 2: Move the 8 files**

```bash
git mv SharedProject/Models/Requests/BindAccessCodeRequest.cs Contracts.AdminApi/Requests/BindAccessCodeRequest.cs
git mv SharedProject/Models/Requests/ChangePasswordRequest.cs Contracts.AdminApi/Requests/ChangePasswordRequest.cs
git mv SharedProject/Models/Requests/GenerateOtpRequest.cs    Contracts.AdminApi/Requests/GenerateOtpRequest.cs
git mv SharedProject/Models/Requests/LoginRequest.cs          Contracts.AdminApi/Requests/LoginRequest.cs
git mv SharedProject/Models/Requests/RegisterRequest.cs       Contracts.AdminApi/Requests/RegisterRequest.cs
git mv SharedProject/Models/Requests/ResetPasswordRequest.cs  Contracts.AdminApi/Requests/ResetPasswordRequest.cs
git mv SharedProject/Models/Requests/SetFavoriteRequest.cs    Contracts.AdminApi/Requests/SetFavoriteRequest.cs
git mv SharedProject/Models/Requests/VerifyOtpRequest.cs      Contracts.AdminApi/Requests/VerifyOtpRequest.cs
```

- [ ] **Step 3: Rewrite namespaces**

In every moved file, change `namespace SharedProject.Models.Requests;` → `namespace TaikoLocalServer.Contracts.AdminApi.Requests;`.

If a file imports `SharedProject.Models` or `SharedProject.Enums` explicitly, drop the using and rely on the global using added in Phase 4.1 (or qualify inline with `TaikoLocalServer.Contracts.AdminApi.ViewModels.X` / `TaikoLocalServer.Domain.Enums.Y`).

Reference outputs:

`Contracts.AdminApi/Requests/LoginRequest.cs`:

```csharp
namespace TaikoLocalServer.Contracts.AdminApi.Requests;

public class LoginRequest
{
    public string AccessCode { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

`Contracts.AdminApi/Requests/SetFavoriteRequest.cs` (verify by reading the file; if it references `Difficulty` or other enums, the global using covers it):

```csharp
namespace TaikoLocalServer.Contracts.AdminApi.Requests;

public class SetFavoriteRequest
{
    public uint Baid { get; set; }
    public uint SongId { get; set; }
    public bool IsFavorite { get; set; }
}
```

(Actual property set may differ — preserve exactly what the file already contains, only swap the namespace.)

- [ ] **Step 4: Build to confirm Contracts.AdminApi still compiles**

```bash
dotnet build Contracts.AdminApi/Contracts.AdminApi.csproj
```

Expected: 0 errors.

- [ ] **Step 5: Commit**

```bash
git add Contracts.AdminApi/Requests SharedProject
git commit -m "PR4.3: move 8 admin-API Request DTOs from SharedProject to Contracts.AdminApi"
```

---

## Phase 4.4 — Move Responses

### Task 4.4.1: Move 5 Response DTOs into `Contracts.AdminApi/Responses/`

**Files:**
- Move: `SharedProject/Models/Responses/*.cs` → `Contracts.AdminApi/Responses/`

- [ ] **Step 1: Create the target directory**

```bash
mkdir -p Contracts.AdminApi/Responses
```

- [ ] **Step 2: Move the 5 files**

```bash
git mv SharedProject/Models/Responses/DanBestDataResponse.cs    Contracts.AdminApi/Responses/DanBestDataResponse.cs
git mv SharedProject/Models/Responses/SongBestResponse.cs       Contracts.AdminApi/Responses/SongBestResponse.cs
git mv SharedProject/Models/Responses/SongHistoryResponse.cs    Contracts.AdminApi/Responses/SongHistoryResponse.cs
git mv SharedProject/Models/Responses/SongLeaderboardResponse.cs Contracts.AdminApi/Responses/SongLeaderboardResponse.cs
git mv SharedProject/Models/Responses/UsersResponse.cs          Contracts.AdminApi/Responses/UsersResponse.cs
```

- [ ] **Step 3: Rewrite namespaces**

For every moved file, change `namespace SharedProject.Models.Responses;` → `namespace TaikoLocalServer.Contracts.AdminApi.Responses;`.

Each response references the moved ViewModels (e.g., `UsersResponse.Users` is `List<User>`). Since `Contracts.AdminApi/GlobalUsings.cs` doesn't currently import `TaikoLocalServer.Contracts.AdminApi.ViewModels;`, **add it** so responses can reference ViewModels without explicit usings:

Edit `Contracts.AdminApi/GlobalUsings.cs` to:

```csharp
global using TaikoLocalServer.Domain.Enums;
global using TaikoLocalServer.Contracts.AdminApi.ViewModels;
```

Reference output `Contracts.AdminApi/Responses/UsersResponse.cs`:

```csharp
namespace TaikoLocalServer.Contracts.AdminApi.Responses;

public class UsersResponse
{
    public List<User> Users { get; set; } = new();
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 0;
    public int TotalUsers { get; set; } = 0;
}
```

- [ ] **Step 4: Build Contracts.AdminApi**

```bash
dotnet build Contracts.AdminApi/Contracts.AdminApi.csproj
```

Expected: 0 errors.

- [ ] **Step 5: Commit**

```bash
git add Contracts.AdminApi/Responses Contracts.AdminApi/GlobalUsings.cs SharedProject
git commit -m "PR4.4: move 5 admin-API Response DTOs from SharedProject to Contracts.AdminApi"
```

---

## Phase 4.5 — Move PlaySettingConverter

### Task 4.5.1: Move converter into `Contracts.AdminApi/Converters/` and add `Throw` package

**Files:**
- Move: `SharedProject/Utils/PlaySettingConverter.cs` → `Contracts.AdminApi/Converters/PlaySettingConverter.cs`
- Modify: `Contracts.AdminApi/Contracts.AdminApi.csproj` (add `Throw` PackageReference)

- [ ] **Step 1: Create the target directory**

```bash
mkdir -p Contracts.AdminApi/Converters
```

- [ ] **Step 2: Move the file**

```bash
git mv SharedProject/Utils/PlaySettingConverter.cs Contracts.AdminApi/Converters/PlaySettingConverter.cs
```

- [ ] **Step 3: Rewrite the file's namespace and usings**

Replace the file contents with:

```csharp
using System.Collections.Specialized;
using Throw;

namespace TaikoLocalServer.Contracts.AdminApi.Converters;

public static class PlaySettingConverter
{
    public static PlaySetting ShortToPlaySetting(short input)
    {
        var bits = new BitVector32(input);
        var speedSection = BitVector32.CreateSection(15);
        var vanishSection = BitVector32.CreateSection(1, speedSection);
        var inverseSection = BitVector32.CreateSection(1, vanishSection);
        var randomSection = BitVector32.CreateSection(2, inverseSection);

        var randomType = (RandomType)bits[randomSection];
        randomType.Throw().IfOutOfRange();
        var result = new PlaySetting
        {
            Speed = (uint)bits[speedSection],
            IsVanishOn = bits[vanishSection] == 1,
            IsInverseOn = bits[inverseSection] == 1,
            RandomType = randomType
        };

        return result;
    }

    public static short PlaySettingToShort(PlaySetting setting)
    {
        var bits = new BitVector32();
        var speedSection = BitVector32.CreateSection(15);
        var vanishSection = BitVector32.CreateSection(1, speedSection);
        var inverseSection = BitVector32.CreateSection(1, vanishSection);
        var randomSection = BitVector32.CreateSection(2, inverseSection);

        bits[speedSection] = (int)setting.Speed;
        bits[vanishSection] = setting.IsVanishOn ? 1 : 0;
        bits[inverseSection] = setting.IsInverseOn ? 1 : 0;
        bits[randomSection] = (int)setting.RandomType;

        return (short)bits.Data;
    }
}
```

`PlaySetting` and `RandomType` resolve via the global usings added in Phase 4.4.

- [ ] **Step 4: Add `Throw` PackageReference to `Contracts.AdminApi.csproj`**

Edit `Contracts.AdminApi/Contracts.AdminApi.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <RootNamespace>TaikoLocalServer.Contracts.AdminApi</RootNamespace>
    <AssemblyName>TaikoLocalServer.Contracts.AdminApi</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Throw" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Domain\Domain.csproj" />
  </ItemGroup>

</Project>
```

`Directory.Packages.props` already pins the `Throw` version (it was previously used by `SharedProject`), so no version is needed here.

- [ ] **Step 5: Build Contracts.AdminApi**

```bash
dotnet build Contracts.AdminApi/Contracts.AdminApi.csproj
```

Expected: 0 errors.

- [ ] **Step 6: Commit**

```bash
git add Contracts.AdminApi/Converters Contracts.AdminApi/Contracts.AdminApi.csproj SharedProject
git commit -m "PR4.5: move PlaySettingConverter from SharedProject to Contracts.AdminApi"
```

---

## Phase 4.6 — Relocate WebUI-consumed ServerData types

> **Why this phase exists:** PR2 placed admin-API JSON-shape types (DanData, MusicDetail, etc.) in `Application/ServerData/`. The WebUI deserializes these from API responses, so leaving them in Application would force `TaikoWebUI → Application`, which violates the layering. We move only the two types the WebUI actually consumes — `DanData` and `MusicDetail` — into `Contracts.AdminApi/ServerData/`. The other Application/ServerData files (EventFolderData, MovieData, QRCodeData, ShopFolderData, IVerupNo, SongIntroductionData) are server-only and stay in Application.

### Task 4.6.1: Verify which ServerData types the WebUI consumes

**Files:** none (audit only).

- [ ] **Step 1: Search the WebUI for ServerData type names**

```bash
grep -rE "DanData|MusicDetail|EventFolderData|MovieData|QRCodeData|ShopFolderData|IVerupNo|SongIntroductionData" TaikoWebUI/ --include="*.cs" --include="*.razor"
```

Expected hits: `DanData` and `MusicDetail` only. If others surface (e.g., `EventFolderData`), expand the move list in Step 2 below to cover them. Do not skip this audit — PR2's move list could have grown.

- [ ] **Step 2: Re-audit `Contracts.AdminApi/ViewModels/Costume.cs` and `Title.cs`**

These were moved in Phase 4.2 (treated as ViewModels). Confirm WebUI consumes them:

```bash
grep -rE "\bCostume\b|\bTitle\b" TaikoWebUI/ --include="*.cs" --include="*.razor"
```

Expected: many hits in WebUI. The Phase 4.2 move was correct.

### Task 4.6.2: Move DanData and MusicDetail into Contracts.AdminApi

**Files:**
- Move: `Application/ServerData/DanData.cs` → `Contracts.AdminApi/ServerData/DanData.cs`
- Move: `Application/ServerData/MusicDetail.cs` → `Contracts.AdminApi/ServerData/MusicDetail.cs`

- [ ] **Step 1: Create the target directory**

```bash
mkdir -p Contracts.AdminApi/ServerData
```

- [ ] **Step 2: Move the two files**

```bash
git mv Application/ServerData/DanData.cs     Contracts.AdminApi/ServerData/DanData.cs
git mv Application/ServerData/MusicDetail.cs Contracts.AdminApi/ServerData/MusicDetail.cs
```

- [ ] **Step 3: Rewrite namespaces**

In each moved file, change `namespace TaikoLocalServer.Application.ServerData;` → `namespace TaikoLocalServer.Contracts.AdminApi.ServerData;`.

Drop any `using TaikoLocalServer.Application.ServerData;` statements inside the file. If a file references `Difficulty`, `DanType`, etc. from Domain.Enums, the global using added in Phase 4.1 covers it.

If `MusicDetail.cs` or `DanData.cs` references `EventFolderData` / `MovieData` / `QRCodeData` / `ShopFolderData` / `IVerupNo` / `SongIntroductionData` (the Application/ServerData types staying behind), keep an explicit `using TaikoLocalServer.Application.ServerData;` — except that creates a Contracts → Application dependency, which is forbidden. **If this case actually arises**, stop and either (a) move the additional referenced types here too, or (b) restructure the type to drop the cross-reference. Read both files in full before proceeding to confirm.

- [ ] **Step 4: Update Application's GlobalUsings to expose Contracts.AdminApi.ServerData**

Edit `Application/GlobalUsings.cs` (PR2 created it). Add:

```csharp
global using TaikoLocalServer.Contracts.AdminApi.ServerData;
```

This makes existing `using TaikoLocalServer.Application.ServerData;` consumers continue to work for the still-resident types and silently pick up the relocated DanData/MusicDetail via the new global.

- [ ] **Step 5: Add ProjectReference from Application to Contracts.AdminApi**

Edit `Application/Application.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="..\Domain\Domain.csproj" />
  <ProjectReference Include="..\Contracts.AdminApi\Contracts.AdminApi.csproj" />
</ItemGroup>
```

(Replace the existing `<ItemGroup>` block that holds project references; preserve any other ProjectReferences PR2 added.)

- [ ] **Step 6: Find and fix lingering `Application.ServerData.DanData` / `MusicDetail` qualifiers**

```bash
grep -rE "Application\.ServerData\.(DanData|MusicDetail)" --include="*.cs"
```

For each hit (other than inside `bin/`/`obj/`), replace `TaikoLocalServer.Application.ServerData.DanData` → `TaikoLocalServer.Contracts.AdminApi.ServerData.DanData` (and similarly for MusicDetail). If the hit uses just `DanData` or `MusicDetail` unqualified, the global using already added covers it — leave it alone.

- [ ] **Step 7: Build the whole solution**

```bash
dotnet build
```

Expected: errors only at WebUI consumer sites (those still reference `SharedProject.*`). The Application + Adapter + Host build green for these two types now. If you see *new* errors in Application that weren't there before Phase 4.2, audit Step 6.

- [ ] **Step 8: Commit**

```bash
git add Contracts.AdminApi/ServerData Application/ServerData Application/GlobalUsings.cs Application/Application.csproj
git commit -m "PR4.6: move DanData + MusicDetail from Application/ServerData into Contracts.AdminApi/ServerData (allows WebUI to consume without depending on Application)"
```

---

## Phase 4.7 — Update Adapters.AdminApi to reference Contracts.AdminApi

### Task 4.7.1: Add ProjectReference and fix usings

**Files:**
- Modify: `Adapters.AdminApi/Adapters.AdminApi.csproj`
- Modify: `Adapters.AdminApi/GlobalUsings.cs`
- Modify: `Adapters.AdminApi/Controllers/*.cs` (10 controllers from PR3)

- [ ] **Step 1: Add ProjectReference to `Adapters.AdminApi.csproj`**

Edit `Adapters.AdminApi/Adapters.AdminApi.csproj`. The `<ItemGroup>` containing project references should now include Contracts.AdminApi:

```xml
<ItemGroup>
  <ProjectReference Include="..\Application\Application.csproj" />
  <ProjectReference Include="..\Domain\Domain.csproj" />
  <ProjectReference Include="..\Contracts.AdminApi\Contracts.AdminApi.csproj" />
</ItemGroup>
```

(Domain reference may not be strictly required if Application already exposes it transitively; keep whatever PR3 left, just add the Contracts.AdminApi line.)

- [ ] **Step 2: Update `Adapters.AdminApi/GlobalUsings.cs`**

Replace the file contents with:

```csharp
global using TaikoLocalServer.Domain.Enums;
global using TaikoLocalServer.Contracts.AdminApi.ViewModels;
global using TaikoLocalServer.Contracts.AdminApi.Requests;
global using TaikoLocalServer.Contracts.AdminApi.Responses;
global using TaikoLocalServer.Contracts.AdminApi.Converters;
global using TaikoLocalServer.Contracts.AdminApi.ServerData;
global using TaikoLocalServer.Application;
// ...preserve any other globals PR3 added (Mediator, Microsoft.AspNetCore.Mvc, etc.)
```

This means the controllers don't need to add explicit `using TaikoLocalServer.Contracts.AdminApi.*;` — they already reference `User`, `LoginRequest`, etc. unqualified; the global usings make those resolve to the new namespaces.

- [ ] **Step 3: Strip stale `SharedProject.*` usings from controllers**

```bash
grep -rE "using\s+SharedProject\." Adapters.AdminApi/ --include="*.cs"
```

For each hit (e.g., `using SharedProject.Models;`, `using SharedProject.Models.Requests;`, `using SharedProject.Models.Responses;`, `using SharedProject.Utils;`, `using SharedProject.Enums;`), simply delete the line. The replacement globals are already in place from Step 2.

If any controller fully-qualifies a type (e.g., `SharedProject.Models.User`), replace those qualifiers with `TaikoLocalServer.Contracts.AdminApi.ViewModels.User` (or just `User` since the global using covers it).

- [ ] **Step 4: Build Adapters.AdminApi**

```bash
dotnet build Adapters.AdminApi/Adapters.AdminApi.csproj
```

Expected: 0 errors. If something fails to resolve, the most likely cause is a still-explicit `SharedProject.X` qualifier — grep again.

- [ ] **Step 5: Build the full solution**

```bash
dotnet build
```

Expected: errors remain **only** in TaikoWebUI (still using `SharedProject` references). Server-side builds green.

- [ ] **Step 6: Commit**

```bash
git add Adapters.AdminApi
git commit -m "PR4.7: rewire Adapters.AdminApi from SharedProject to Contracts.AdminApi"
```

---

## Phase 4.8 — Update TaikoWebUI to reference Contracts.AdminApi

### Task 4.8.1: Swap ProjectReference

**Files:**
- Modify: `TaikoWebUI/TaikoWebUI.csproj`

- [ ] **Step 1: Replace the SharedProject reference**

Edit `TaikoWebUI/TaikoWebUI.csproj`. Replace:

```xml
<ItemGroup>
  <ProjectReference Include="..\SharedProject\SharedProject.csproj" />
</ItemGroup>
```

with:

```xml
<ItemGroup>
  <ProjectReference Include="..\Contracts.AdminApi\Contracts.AdminApi.csproj" />
</ItemGroup>
```

Don't touch the rest of the csproj — `<PackageReference>` blocks, `<Content Update>` for wwwroot, Localization `<EmbeddedResource>` blocks, `<_ContentIncludedByDefault>`, `<UpToDateCheckInput>` all stay as-is.

### Task 4.8.2: Update WebUI GlobalUsings

**Files:**
- Modify: `TaikoWebUI/GlobalUsings.cs`

- [ ] **Step 1: Replace the file contents**

Replace `TaikoWebUI/GlobalUsings.cs` with:

```csharp
// Global using directives
global using System.Net.Http;
global using System.Net.Http.Json;
global using Microsoft.AspNetCore.Components;
global using Microsoft.AspNetCore.Components.Web;
global using MudBlazor;
global using TaikoWebUI;
global using TaikoWebUI.Services;
global using TaikoWebUI.Shared;
global using TaikoLocalServer.Contracts.AdminApi.ViewModels;
global using TaikoLocalServer.Contracts.AdminApi.Requests;
global using TaikoLocalServer.Contracts.AdminApi.Responses;
global using TaikoLocalServer.Contracts.AdminApi.ServerData;
global using TaikoLocalServer.Contracts.AdminApi.Converters;
global using TaikoLocalServer.Domain.Enums;
global using Throw;
```

Note the swap: 4 `SharedProject.*` globals removed, 5 `TaikoLocalServer.Contracts.AdminApi.*` globals added (Converters is new for `PlaySettingConverter`), plus `TaikoLocalServer.Domain.Enums` replaces `SharedProject.Enums`.

### Task 4.8.3: Update WebUI _Imports.razor

**Files:**
- Modify: `TaikoWebUI/_Imports.razor`

- [ ] **Step 1: Replace the file contents**

Replace `TaikoWebUI/_Imports.razor` with:

```razor
@using System.Net.Http
@using System.Net.Http.Json
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Web.Virtualization
@using Microsoft.AspNetCore.Components.WebAssembly.Http
@using Microsoft.JSInterop
@using MudBlazor
@using TaikoWebUI
@using TaikoWebUI.Shared
@using TaikoWebUI.Services
@using TaikoLocalServer.Contracts.AdminApi.ViewModels
@using TaikoLocalServer.Contracts.AdminApi.Requests
@using TaikoLocalServer.Contracts.AdminApi.Responses
@using TaikoLocalServer.Contracts.AdminApi.ServerData
@using TaikoLocalServer.Domain.Enums
@using Throw
@using Localization
@using System.Globalization
@using Microsoft.Extensions.Localization
@inject IStringLocalizer<LocalizationResource> Localizer
```

### Task 4.8.4: Strip stale `SharedProject.*` usings from WebUI source files

**Files:**
- Modify (audit + fix): `TaikoWebUI/**/*.cs` and `TaikoWebUI/**/*.razor`

- [ ] **Step 1: Find every WebUI file still importing SharedProject**

```bash
grep -rE "(@using\s+SharedProject\.|using\s+SharedProject\.)" TaikoWebUI/ --include="*.cs" --include="*.razor"
```

Expected hits (per the pre-flight audit captured during planning):
- `TaikoWebUI/Services/GameDataService.cs:1` → `using SharedProject.Models;`
- `TaikoWebUI/Pages/PlayHistory.razor.cs:3-4` → `using SharedProject.Enums;` and `using SharedProject.Models;`
- `TaikoWebUI/Pages/Song.razor.cs:1` → `using SharedProject.Models;`

Plus possibly a few we didn't catch — fix every hit.

- [ ] **Step 2: Delete each `using SharedProject.*;` line**

The replacements are already covered by the new global usings in Phase 4.8.2 / 4.8.3. Each file just needs the line(s) deleted.

For example, `TaikoWebUI/Services/GameDataService.cs` becomes:

```csharp
using System.Collections.Immutable;

namespace TaikoWebUI.Services;

public class GameDataService : IGameDataService
{
    // ...rest unchanged...
}
```

(The `using SharedProject.Models;` is gone; `DanData`, `MusicDetail`, `Costume`, `Title` resolve via the new global usings to `TaikoLocalServer.Contracts.AdminApi.{ViewModels,ServerData}`.)

For `TaikoWebUI/Pages/Song.razor.cs`:

```csharp
namespace TaikoWebUI.Pages;

public partial class Song
{
    [Parameter]
    public int SongId { get; set; }

    [Parameter]
    public int Baid { get; set; }

    private UserSetting? userSetting;
    private SongHistoryResponse? response;
    private List<SongHistoryData>? songHistoryData;
    // ...rest unchanged...
}
```

For `TaikoWebUI/Pages/PlayHistory.razor.cs`: drop both `using SharedProject.Enums;` and `using SharedProject.Models;`. The unqualified type references resolve via globals.

- [ ] **Step 3: Build TaikoWebUI**

```bash
dotnet build TaikoWebUI/TaikoWebUI.csproj
```

Expected: 0 errors. If something fails:
- `error CS0246: The type or namespace name 'X' could not be found` — likely a fully-qualified `SharedProject.Models.X` somewhere; grep for `SharedProject\.` and replace with the new namespace.
- `error CS0234: type 'Y' does not exist in 'TaikoLocalServer.Contracts.AdminApi'` — check Phase 4.2/4.3/4.4/4.5 actually moved that file; if it's a ServerData type the WebUI references but isn't yet in Contracts, expand Phase 4.6's move list.

- [ ] **Step 4: Build the full solution**

```bash
dotnet build
```

Expected: 0 errors across all 12 projects (Contracts.AdminApi is now wired in; SharedProject still present but with no consumers — harmless).

- [ ] **Step 5: Commit**

```bash
git add TaikoWebUI
git commit -m "PR4.8: rewire TaikoWebUI from SharedProject to Contracts.AdminApi"
```

---

## Phase 4.9 — Delete SharedProject and the legacy WebUI .sln

### Task 4.9.1: Confirm SharedProject is empty of consumers

**Files:** none (audit only).

- [ ] **Step 1: Confirm no remaining references**

```bash
grep -rE "(SharedProject\.|\\\\SharedProject\\\\|/SharedProject/|\"\.\.\\\\SharedProject)" --include="*.cs" --include="*.razor" --include="*.csproj" --include="*.slnx"
```

Expected: zero hits outside `bin/`, `obj/`, and `docs/`. If there are hits, fix them before deleting (otherwise the solution will fail to load).

- [ ] **Step 2: Confirm SharedProject folder is empty of code**

```bash
find SharedProject -type f -not -path '*/bin/*' -not -path '*/obj/*'
```

Expected output: only `SharedProject.csproj` (everything else moved). If `Models/`, `Models/Requests/`, `Models/Responses/`, or `Utils/` still contain `.cs` files, the earlier phases skipped something — go back and find what's missing.

### Task 4.9.2: Delete the SharedProject folder and its .slnx entry

**Files:**
- Delete: `SharedProject/` (entire directory)
- Modify: `TaikoLocalServer.slnx` (remove the SharedProject line)

- [ ] **Step 1: Delete the folder**

```bash
git rm -r SharedProject
```

`git rm -r` will remove the tracked `SharedProject.csproj` and any leftover tracked files. `bin/` and `obj/` are git-ignored, so they're handled by:

```bash
rm -rf SharedProject/bin SharedProject/obj 2>/dev/null || true
rmdir SharedProject 2>/dev/null || true
```

- [ ] **Step 2: Edit `TaikoLocalServer.slnx`**

Remove the line `<Project Path="SharedProject/SharedProject.csproj" />`. The remaining entries are: GameDatabase (replaced by Infrastructure in PR2 — verify it's already gone), LocalSaveModScoreMigrator, Domain, Application, Infrastructure, Adapters.AdminApi, Adapters.AllnetMucha, Adapters.GameProtocol.Shared, Adapters.GameProtocol.WwR08, Adapters.GameProtocol.CnR00, Host, Contracts.AdminApi, TaikoWebUI.

Final count check:

```bash
grep -c "<Project Path=" TaikoLocalServer.slnx
```

Expected: 13 (Domain, Application, Infrastructure, 5 Adapters, Host, Contracts.AdminApi, TaikoWebUI, LocalSaveModScoreMigrator).

If GameDatabase is still present, PR2 was incomplete — flag and stop.

- [ ] **Step 3: Build**

```bash
dotnet build
```

Expected: 0 errors. If a project fails to find SharedProject, an earlier phase missed a reference.

### Task 4.9.3: Delete the legacy TaikoWebUI.sln

**Files:**
- Delete: `TaikoWebUI/TaikoWebUI.sln`

- [ ] **Step 1: Confirm the file is genuinely a duplicate**

```bash
cat TaikoWebUI/TaikoWebUI.sln
```

Expected: a Visual Studio solution that at most references `TaikoWebUI.csproj` and `SharedProject.csproj`. It is a leftover from when the WebUI was developed standalone. The repo's actual solution is `TaikoLocalServer.slnx` at the root.

- [ ] **Step 2: Delete it**

```bash
git rm TaikoWebUI/TaikoWebUI.sln
```

- [ ] **Step 3: Build**

```bash
dotnet build
```

Expected: 0 errors.

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "PR4.9: delete SharedProject (now empty) and legacy TaikoWebUI/TaikoWebUI.sln"
```

---

## Phase 4.10 — Rewrite CLAUDE.md

### Task 4.10.1: Update each section to match the new layout

**Files:**
- Modify: `CLAUDE.md` (entire file)

> The current `CLAUDE.md` describes 5 projects. PR4 ends with 13. Seven sections need updates per spec §6: solution layout, common commands, configuration model, where data files live, request architecture, conventions, Mediator handler conventions. We add a new short section on the Adapter contract.

- [ ] **Step 1: Read the current `CLAUDE.md` to confirm what's there**

Read `CLAUDE.md` start-to-end. Note: the current text references `TaikoLocalServer/` as both the folder and the assembly name. After PR3, the folder is `Host/` but `<AssemblyName>` preserves `TaikoLocalServer.exe` as the published binary.

- [ ] **Step 2: Rewrite the file**

Replace `CLAUDE.md` with the version below. The structure mirrors the original; only content changes. Preserve the existing tone (terse, opinionated, "what to do / what not to do").

```markdown
# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A private server emulator for Taiko no Tatsujin Nijiiro (CHN `v12r00_cn` and 39.06 `v12r08_ww`). The same ASP.NET Core 10 process serves the game's protobuf endpoints **and** hosts the Blazor WebAssembly admin UI on the same Kestrel server, so there is no separate frontend deployment. The codebase follows a hexagonal/ports-and-adapters layout: a pure `Domain` core, an `Application` orchestration layer that depends only on ports, an `Infrastructure` project that owns the real EF Core context + filesystem catalog + JWT issuer + clock, and one adapter project per inbound surface (admin REST, AllNet/Mucha lifecycle, and one per game protocol version).

## Solution layout

12 projects, all `net10.0` unless noted:

- **Domain** — pure entities, value objects, enums, domain constants. No package or project references. Namespace `TaikoLocalServer.Domain.*`.
- **Application** — orchestration. Mediator request/handler types, port interfaces (`ITaikoDbContext`, `IGameDataCatalog`, `IJwtTokenService`, `IClock`), Common* DTOs, ServerData JSON shapes, application-only utilities, settings (`ServerSettings`, `DataSettings`). References Domain + Contracts.AdminApi (the latter for shared JSON shapes).
- **Infrastructure** — EF Core 10 + SQLite (`TaikoDbContext` implementing `ITaikoDbContext`), filesystem catalog (`FileGameDataCatalog`), `JwtTokenService`, `SystemClock`. Owns **all migrations** (under `Infrastructure/Persistence/Migrations/`). Migrations are applied automatically on server startup.
- **Contracts.AdminApi** — DTO-only project: ViewModels, Requests, Responses, ServerData JSON shapes, and the `PlaySettingConverter` that the admin REST API and TaikoWebUI both serialize/deserialize. References Domain only (for enums). No business logic.
- **Adapters.AdminApi** — admin REST controllers (`/api/...`) consumed by TaikoWebUI. JWT bearer auth scheme registered here. Apply `[AuthorizeIfRequired]` on these.
- **Adapters.AllnetMucha** — AmAuth/AmUpdater/Garmc/MuchaActivation controllers, the `AllNetRequestMiddleware` (zlib-decompresses base64 form bodies for `/sys/servlet/PowerOn`), and the Mucha wire types.
- **Adapters.GameProtocol.Shared** — `BaseProtocolController`, gzip/header-strip helpers shared by both game-protocol adapters.
- **Adapters.GameProtocol.WwR08** — protobuf controllers + wire types + Mapperly mappers for the 39.06 WW client. Routes `/v12r08_ww/...`.
- **Adapters.GameProtocol.CnR00** — protobuf controllers + wire types + Mapperly mappers for the CHN client. Routes `/v12r00_cn/...`.
- **Host** — ASP.NET Core 10 entry point. Owns `Program.cs`, `Configurations/`, `Certificates/`, `wwwroot/`, `Logging/CsvFormatter.cs`. Hosts the Blazor UI via `UseBlazorFrameworkFiles()` + `MapFallbackToFile("index.html")`. `<AssemblyName>TaikoLocalServer</AssemblyName>` preserves the published exe name (`TaikoLocalServer.exe`).
- **TaikoWebUI** — Blazor WebAssembly admin UI (MudBlazor 9). References Contracts.AdminApi only. Bundled into Host's publish output via the `UseBlazorFrameworkFiles()` plumbing (TaikoWebUI is NOT a project reference of Host; the build pipeline copies the `_framework/` artifacts into `Host/wwwroot/`).
- **LocalSaveModScoreMigrator** — standalone CLI (System.CommandLine) for importing local-save-mod JSON dumps into `taiko.db3`. References Infrastructure to reuse the EF Core context. Not part of the runtime.

The dependency direction is strictly: `Domain ← Contracts.AdminApi ← Application ← Infrastructure ← Host`, with the adapters slotting in at the right layer (`Adapters.* → Application` for game/admin/AllNet logic; Host references all adapters and Infrastructure to compose the runtime). Domain has zero outbound references.

## Adapter contract

Every adapter project ships a `DependencyInjection.cs` that defines exactly one extension method on `IServiceCollection` (or `IApplicationBuilder` if it needs middleware wiring), e.g. `AddAdminApi(IConfiguration)`, `AddAllnetMucha()` / `UseAllnetMucha()`, `AddGameProtocolWwR08()`, `AddGameProtocolCnR00()`. Adding a new game version means: create `Adapters.GameProtocol.<Version>/`, add one `<ProjectReference>` to Host, add one `Add...()` call in `Program.cs`. **Adapters never reference each other** (except `GameProtocol.WwR08` and `GameProtocol.CnR00` may both reference `GameProtocol.Shared`). Cross-adapter coordination flows through the Application layer.

## Common commands

Run from the repo root:

```bash
# Build the whole solution
dotnet build

# Publish a self-contained single-file Windows exe (Release config sets PublishSingleFile/SelfContained)
dotnet publish Host/Host.csproj
# Output: Host/bin/Release/net10.0/win-x64/publish/TaikoLocalServer.exe

# Run the server in dev (auto-applies migrations)
dotnet run --project Host

# Add an EF Core migration — note the split: Infrastructure owns the context, Host is the startup project
dotnet ef migrations add <Name> --project Infrastructure --startup-project Host

# Update database manually (normally not needed — server migrates on startup)
dotnet ef database update --project Infrastructure --startup-project Host

# Run the score migrator
dotnet run --project LocalSaveModScoreMigrator -- --save-file-path <path> --baid <id>
```

There are no automated tests in this repository.

## Configuration model

`Host/appsettings.json` is intentionally near-empty. **Real configuration lives in `Host/Configurations/`** (loaded explicitly in `Program.cs`):

- `Kestrel.json` — multi-port binding. The server listens on **multiple ports simultaneously** (5000 base, 80 AmAuth, 10122 Mucha, 54430/54431 game WW, 57402 game CN, 443 Garmc) using one cert at `Host/Certificates/cert.pfx`.
- `Database.json` — only sets `DbFileName`. The DB lives at `<exe-dir>/wwwroot/<DbFileName>` (default `taiko.db3`).
- `ServerSettings.json` — `EnableMoreSongs` raises `DomainConstants.MusicIdMax` from 1600 to 9000 (use with caution per the warning in `Program.cs`).
- `DataSettings.json` — filenames for the server-owned JSON datatables under `wwwroot/data/`.
- `AuthSettings.json` — JWT issuer/audience/key. **`AuthenticationRequired: false` disables auth on all admin API endpoints** (the `AuthorizeIfRequiredAttribute` filter short-circuits).
- `Logging.json` — Serilog. There is also a side-channel CSV sink in `Program.cs` (`HeadClerkLog-*.csv`) that captures only log messages starting with `"CSV WRITE:"` from the HeadClerk2 controller in `Adapters.GameProtocol.{WwR08,CnR00}`.

These files are copied to the publish output via `<None Include=... CopyToOutputDirectory="PreserveNewest"/>` in `Host/Host.csproj`. WebUI configuration is fetched at runtime from `wwwroot/appsettings.json` over HTTP (see `TaikoWebUI/Program.cs`).

## Where data files live (and why it matters)

`Infrastructure.GameDataCatalog.PathHelper.GetRootPath()` returns the **`wwwroot` directory next to the running executable** — not the project source's `wwwroot`. Anything under `Host/wwwroot/data/` is copied to that location on build/publish:

- `wwwroot/data/*.json` — **server-owned** datatables that operators edit (`dan_data.json`, `event_folder_data.json`, `gaiden_data.json`, `intro_data.json`, `locked_*_data.json`, `movie_data.json`, `qrcode_data.json`, `shop_folder_data.json`, `token_data.json`, `special_songs_data.json`). Documented in `Host/README.md` (or the top-level README).
- `wwwroot/data/datatable/*.bin` — **game-owned** binary datatables (`musicinfo.bin`, `music_order.bin`, `wordlist.bin`, `don_cos_reward.bin`, `shougou.bin`, `neiro.bin`). These ship with the game install and **must be copied in by the operator before first run** (see top-level `README.md`). Decoded `*.json` siblings may also be present.
- `wwwroot/taiko.db3` — SQLite DB.

`IGameDataCatalog` (singleton, implemented in Infrastructure as `FileGameDataCatalog`) loads all of the above once at startup via `await catalog.InitializeAsync()` in `Program.cs`. Treat its in-memory dictionaries as immutable for the lifetime of the process.

## Request architecture (game side)

The game protocol uses **protobuf-net** over HTTP. Requests are gzip-compressed and (for some endpoints) prefixed with a 32-byte header that controllers strip before deserializing.

Two game versions are supported in parallel and cleanly separated:

- `Adapters.GameProtocol.CnR00/Wire/` — protobuf wire types for the CHN client. Controller routes use `/v12r00_cn/...`.
- `Adapters.GameProtocol.WwR08/Wire/` — protobuf wire types for the 39.06 WW client. Controller routes use `/v12r08_ww/...`.
- `Application/Common/` — version-agnostic Common* DTOs.

The flow: adapter controller deserializes the version-specific request → **Riok.Mapperly** source-generated mapper (in the same adapter project) converts it to a `Common*` DTO in Application → **Mediator** (martinothamar) `Handlers/*Query`/`*Command` in Application operates only on `Common*` types → mapper converts the response back to the requested version's protobuf type. Adding a new game version means adding a new `Adapters.GameProtocol.<Version>/` project (Wire types + mappers + controllers + a `DependencyInjection.cs`); handlers and DB code do not change.

Controllers in `Adapters.GameProtocol.*` inherit `BaseProtocolController` (in `Adapters.GameProtocol.Shared`). Controllers in `Adapters.AdminApi` inherit `BaseAdminController`. Both lazily resolve `IMediator` and `Logger` from `HttpContext.RequestServices`. Don't inject these via constructor — match the existing pattern.

## Conventions to follow

- **Don't add config to `Host/appsettings.json`.** Add a new section to one of the files in `Host/Configurations/`, register it in `Program.cs` (or in the relevant adapter's `DependencyInjection.cs`), and add a `<None Include="Configurations/Foo.json"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></None>` block to `Host/Host.csproj`.
- **New persisted field?** Add it to the entity in `Domain/Entities/`, then `dotnet ef migrations add ...` against `Infrastructure` with `Host` as the startup project. Migrations live under `Infrastructure/Persistence/Migrations/`. Never edit existing migration files.
- **New game endpoint?** Implement both `_cn` and `_ww` route variants in their respective adapter projects (`Adapters.GameProtocol.CnR00` and `Adapters.GameProtocol.WwR08`), each mapping into the same `Common*` request via Mapperly. Send through Mediator; never query `ITaikoDbContext` directly from a controller — that's a handler's job.
- **New admin API endpoint?** Add it to `Adapters.AdminApi/Controllers/`. The DTO types it accepts/returns live in `Contracts.AdminApi/{Requests,Responses,ViewModels}` so the WebUI can deserialize them. Apply `[AuthorizeIfRequired]`.
- **New port?** Define the interface in `Application/Ports/`, implement it in `Infrastructure/`, and register both in `Infrastructure/DependencyInjection.cs`'s `AddInfrastructure(IConfiguration)`. Handlers depend on the port, never on the implementation.
- **GlobalUsings.** Each project has its own `GlobalUsings.cs`. The Host's globals re-export Mediator, ProtoBuf where relevant. The Contracts.AdminApi globals re-export Domain.Enums and the project's own ViewModels. Read the relevant project's `GlobalUsings.cs` before adding explicit `using` statements that might already be global.
- **Auth.** Use `[AuthorizeIfRequired]` (in `Adapters.AdminApi/Filters/`) on admin API endpoints so the operator-controlled `AuthSettings.AuthenticationRequired` toggle works.
- **Mediator handler conventions** (martinothamar's library, registered with `AddApplication()` extension in `Application/DependencyInjection.cs`):
  - Request types are `readonly record struct`, not `record class`.
  - `IRequestHandler<TRequest, TResponse>.Handle` returns `ValueTask<TResponse>`, not `Task<TResponse>`. For void requests use `IRequestHandler<TRequest>` and return `ValueTask<Unit>` ending with `return Unit.Value;`.
  - Service lifetime is **scoped** (set by `opt.ServiceLifetime = ServiceLifetime.Scoped`) because handlers inject `ITaikoDbContext`. Don't change this without also switching to `IDbContextFactory<TaikoDbContext>` and reworking the handlers.
  - Controllers always pass `HttpContext.RequestAborted` as the second argument to `Mediator.Send(...)` for cancellation hygiene.
```

- [ ] **Step 3: Sanity check**

Re-read the rewritten `CLAUDE.md` end-to-end. Verify:

- Every project name in §"Solution layout" is also referenced somewhere in §"Conventions" or §"Common commands".
- Folder paths in §"Common commands" actually exist (`Host/`, `Infrastructure/`, etc.).
- The `dotnet ef migrations` command targets `Infrastructure` (where the context lives), not `GameDatabase` (gone).
- `<AssemblyName>TaikoLocalServer</AssemblyName>` is mentioned at least once so future readers don't get confused by the folder rename.
- Mediator namespace setting is correct: PR2 changed `opt.Namespace = "TaikoLocalServer"` to live in `Application/DependencyInjection.cs` — confirm by reading that file. If PR2 left it in Host's `Program.cs` instead (per Risk #2 fallback), update CLAUDE.md to match reality.
- The `IGameDataService` → `IGameDataCatalog` rename from PR2 is reflected. If PR2 kept the old name, revert this in CLAUDE.md and use `IGameDataService` instead.

If anything in the rewrite contradicts what landed in PR2/PR3, fix the rewrite to match the code (the code is the source of truth).

- [ ] **Step 4: Commit**

```bash
git add CLAUDE.md
git commit -m "PR4.10: rewrite CLAUDE.md for the post-refactor 12-project layout"
```

---

## Phase 4.11 — Skim README.md for stale paths

### Task 4.11.1: Audit and fix any path references in the top-level README

**Files:**
- Modify (audit + fix): `README.md`

> The repo's top-level `README.md` may reference `TaikoLocalServer/` (the old folder) or `wwwroot/data/datatable/*.bin` paths. Don't rewrite the README — just sweep for stale references.

- [ ] **Step 1: Read `README.md` end-to-end**

```bash
cat README.md
```

- [ ] **Step 2: Grep for stale paths**

```bash
grep -nE "TaikoLocalServer/|GameDatabase/|SharedProject/" README.md
```

For each hit, decide:
- "TaikoLocalServer/wwwroot/data/datatable/...": replace `TaikoLocalServer/` → `Host/` (the folder name changed, the binary path didn't).
- "GameDatabase/...": replace with `Infrastructure/Persistence/...` if it's about migrations or context, or just delete the line if it's about the old project structure.
- "SharedProject/...": delete or rewrite to point at `Contracts.AdminApi/`.

If any hit is in the top-level `README` published to users (e.g., setup instructions for operators), be careful — operators see this. Stick close to the original phrasing; only change paths.

- [ ] **Step 3: Skim `TaikoLocalServer/README.md` if it still exists**

PR3 moved `TaikoLocalServer/` → `Host/`. There may have been a `TaikoLocalServer/README.md` that PR3 moved verbatim to `Host/README.md`. If so, do the same path audit on it.

```bash
[ -f Host/README.md ] && grep -nE "TaikoLocalServer/|GameDatabase/|SharedProject/" Host/README.md
```

- [ ] **Step 4: Build to confirm nothing got accidentally broken**

```bash
dotnet build
```

Expected: 0 errors. README-only changes can't break the build, but verify anyway in case of an accidental edit.

- [ ] **Step 5: Commit (only if README changed)**

If the grep found stale paths and you fixed them:

```bash
git add README.md Host/README.md 2>/dev/null || git add README.md
git commit -m "PR4.11: scrub stale project paths from README files"
```

If no changes were needed, skip the commit.

---

## Phase 4.12 — Final smoke gate

### Task 4.12.1: Build green and project count check

**Files:** none (verification).

- [ ] **Step 1: Clean build**

```bash
dotnet build --no-incremental
```

Expected: 0 errors, 0 warnings (or only the warnings PR3 ended with).

- [ ] **Step 2: Verify `.slnx` project list**

```bash
cat TaikoLocalServer.slnx
```

Expected: exactly 13 `<Project Path=...>` entries. No `SharedProject/SharedProject.csproj`. One `Contracts.AdminApi/Contracts.AdminApi.csproj`. No `GameDatabase/GameDatabase.csproj`.

- [ ] **Step 3: Verify SharedProject is gone**

```bash
[ ! -d SharedProject ] && echo "OK" || echo "FAIL: SharedProject still exists"
[ ! -f TaikoWebUI/TaikoWebUI.sln ] && echo "OK" || echo "FAIL: legacy WebUI .sln still exists"
```

Both should print `OK`.

- [ ] **Step 4: Verify no remaining `SharedProject` references**

```bash
grep -rE "SharedProject" --include="*.cs" --include="*.razor" --include="*.csproj" --include="*.slnx" --include="*.md"
```

Acceptable hits: only inside `docs/superpowers/specs/*.md` and `docs/superpowers/plans/*.md` (history). Any hit in `*.cs`, `*.razor`, `*.csproj`, `*.slnx`, or non-spec `*.md` is a fail — find and fix.

### Task 4.12.2: Server boots clean

**Files:** none (runtime smoke).

- [ ] **Step 1: Run the server**

```bash
dotnet run --project Host
```

Expected log lines (roughly):

```
TaikoLocalServer version <version>
Server starting up...
[mediator init OK]
[GameDataCatalog initialized — N entries loaded]
Application started. Press Ctrl+C to shut down.
```

No exceptions, no `error` log lines.

- [ ] **Step 2: Verify migrations applied**

The first run applies migrations from `Infrastructure/Persistence/Migrations/`. Confirm `wwwroot/taiko.db3` exists or was created on disk. No migration errors in the log.

- [ ] **Step 3: Stop the server**

`Ctrl+C` in the run terminal. Confirm clean shutdown:

```
Shut down complete
```

### Task 4.12.3: WebUI page-by-page smoke

**Files:** none (runtime smoke).

> The WebUI ships in the same Kestrel host on port 5000. Visit each page and confirm it renders without console errors. Don't deeply test each page's logic — just confirm the page mounts and the network calls return 2xx (or the expected redirect/auth response when not logged in).

- [ ] **Step 1: Start server**

```bash
dotnet run --project Host
```

- [ ] **Step 2: In a browser, walk every page**

Open the dev tools console. For each URL below, load the page and confirm:
- Page renders without a Blazor crash overlay.
- No red console errors (network 4xx/5xx that are part of normal "no data yet" flow are fine; runtime exceptions are not).
- Required UI controls are visible (rough check — buttons, tables, MudBlazor components).

URLs (start from `http://localhost:5000`):

| Page | URL | Expected |
|---|---|---|
| Dashboard | `/` | Markdown rendered from `wwwroot/Dashboard.md` |
| Login | `/Login` | Login form visible |
| Register | `/Register` | Register form visible |
| Users (admin) | `/Users` | Table — empty or populated |
| Profile | `/Profile/<baid>` (use any baid the DB has, or just the route base if it 404s gracefully) | Profile UI |
| AccessCode | `/AccessCode/<baid>` | Access code list |
| ChangePassword | `/ChangePassword/<baid>` | Password change form |
| HighScores | `/HighScores/<baid>` | Score table |
| PlayHistory | `/PlayHistory/<baid>` | History table |
| SongList | `/SongList` | Song table populated from `MusicDetail` |
| Song | `/Song/<baid>/<songid>` | Per-song detail |
| DaniDojo | `/DaniDojo/<baid>` | Dan stage table populated from `DanData` |

If any page crashes with a deserialization error (`JsonException: ... could not be deserialized into ...`), the most likely cause is a property name mismatch between the server's response shape and the WebUI's `Contracts.AdminApi` type — but since the types are now literally shared via project reference, this should not happen. If it does, audit Phase 4.6 and Phase 4.8.

- [ ] **Step 3: Round-trip Login via the API**

In another terminal:

```bash
curl -X POST http://localhost:5000/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"AccessCode":"<some-existing-access-code>","Password":"<password>"}'
```

Expected: a JSON response with a JWT, or a 401 with a sensible body if the credentials are wrong. Confirm the response shape matches `Contracts.AdminApi.Responses` definitions (or whatever `Auth/login` returns — read the controller).

- [ ] **Step 4: Round-trip SetFavorite via the API (auth-required path)**

If `AuthSettings.AuthenticationRequired = true`, attach the JWT from Step 3:

```bash
curl -X POST http://localhost:5000/api/Users/<baid>/setfavorite \
  -H "Authorization: Bearer <jwt>" \
  -H "Content-Type: application/json" \
  -d '{"SongId":1,"IsFavorite":true}'
```

Expected: 204 No Content (or whatever the controller returns on success). The actual route may differ — read `Adapters.AdminApi/Controllers/UsersController.cs` to confirm the exact path.

If `AuthSettings.AuthenticationRequired = false`, the same call without `Authorization` header should also work (auth attribute is short-circuited).

- [ ] **Step 5: Stop the server**

`Ctrl+C`.

### Task 4.12.4: Publish smoke

**Files:** none (runtime smoke).

- [ ] **Step 1: Publish**

```bash
dotnet publish Host/Host.csproj -c Release
```

Expected output: `Host/bin/Release/net10.0/win-x64/publish/TaikoLocalServer.exe` exists.

- [ ] **Step 2: List the publish directory contents**

```bash
ls Host/bin/Release/net10.0/win-x64/publish/
```

Expected entries:
- `TaikoLocalServer.exe` (single-file)
- `Configurations/` (6 JSON files — match PR3 layout)
- `Certificates/` (cert.pfx, root.pfx)
- `wwwroot/` (with `data/`, the WebUI `_framework/`, `index.html`, `Dashboard.md`, `appsettings.json`)

Notably absent (because they're embedded in the single-file exe): no loose `.dll` files (or only a couple of native-AOT-style sidecar files).

- [ ] **Step 3: Run the published exe**

```bash
cd Host/bin/Release/net10.0/win-x64/publish/
./TaikoLocalServer.exe
```

Expected: same startup log as Phase 4.12.2 Step 1. Browse to `http://localhost:5000/` and confirm the Dashboard renders.

- [ ] **Step 4: Stop and return to repo root**

`Ctrl+C`, then:

```bash
cd ../../../../../..
pwd
```

Expected: back at `D:\TaikoLocalServer\`.

### Task 4.12.5: Final sanity sweep

**Files:** none (verification).

- [ ] **Step 1: Status check**

```bash
git status
```

Expected: clean tree (every phase committed).

- [ ] **Step 2: Log check**

```bash
git log --oneline dev..HEAD
```

Expected (in order): a commit per phase 4.1 through 4.10/4.11 (4.11 may be skipped if README needed no changes), each prefixed `PR4.<N>:`.

- [ ] **Step 3: Diff against `dev`**

```bash
git diff --stat dev...HEAD
```

Expected: many small file moves (high churn, low net delta — git tracked the renames). New `Contracts.AdminApi/` tree. Deleted `SharedProject/` tree. Modified `CLAUDE.md`. Possibly modified `README.md`. Modified `Adapters.AdminApi/`, `TaikoWebUI/`, `Application/`. Modified `TaikoLocalServer.slnx`.

---

## Phase 4.13 — Push and (optionally) open PR

### Task 4.13.1: Push the branch

- [ ] **Step 1: Confirm clean tree**

```bash
git status
```

- [ ] **Step 2: Push**

```bash
git push -u origin dev/clean-arch-contracts-webui
```

- [ ] **Step 3: Open PR (only if user authorized)**

Ask before running `gh pr create`. If approved:

```bash
gh pr create --title "Clean architecture refactor PR4: Contracts.AdminApi + WebUI rewire + docs" --body "$(cat <<'EOF'
## Summary

Carves out a `TaikoLocalServer.Contracts.AdminApi` project owning every DTO/ViewModel/Request/Response shared between the admin REST API and TaikoWebUI:

- Created `Contracts.AdminApi/` with `ViewModels/`, `Requests/`, `Responses/`, `ServerData/`, `Converters/`. References `Domain` only.
- Moved 13 ViewModels, 8 Requests, 5 Responses, `PlaySettingConverter`, plus `DanData` + `MusicDetail` (the two WebUI-consumed ServerData types) out of `SharedProject` / `Application/ServerData`.
- Rewired `TaikoWebUI` to reference `Contracts.AdminApi` instead of `SharedProject` (csproj + GlobalUsings + _Imports).
- Rewired `Adapters.AdminApi` to reference `Contracts.AdminApi`.
- Deleted `SharedProject/` (now empty) and the legacy `TaikoWebUI/TaikoWebUI.sln`.
- Rewrote `CLAUDE.md` for the 12-project hexagonal layout and scrubbed stale paths from `README.md`.

Spec: docs/superpowers/specs/2026-05-04-clean-architecture-refactor-design.md (§7 PR4)

## Test plan

- [x] `dotnet build` green; `.slnx` shows 12 projects with no `SharedProject` entry and one `Contracts.AdminApi` entry
- [x] No remaining `SharedProject.*` namespace references in `*.cs`, `*.razor`, `*.csproj`, `*.slnx`, or non-spec `*.md`
- [x] Server boots, applies migrations, `IGameDataCatalog` initializes
- [x] Every WebUI page (Dashboard, Login, Register, Users, Profile, AccessCode, ChangePassword, HighScores, PlayHistory, SongList, Song, DaniDojo) renders without console errors
- [x] Login round-trip via `POST /api/Auth/login` returns JWT
- [x] SetFavorite round-trip via authenticated POST returns 2xx
- [x] `dotnet publish -c Release` produces `Host/bin/Release/net10.0/win-x64/publish/TaikoLocalServer.exe` with `wwwroot/`, `Configurations/`, `Certificates/`; published exe smoke-tests pass

🤖 Generated with [Claude Code](https://claude.com/claude-code)
EOF
)" --base dev
```

---

## Risk register specific to PR4

From spec §8:

| # | Risk | Mitigation |
|---|---|---|
| 10 | WebUI deserialization breaks because Contracts.AdminApi DTOs differ subtly from the SharedProject originals | Phase 4.2/4.3/4.4 instructions say "preserve exactly what the file already contains, only swap the namespace." The types are byte-identical — only their fully-qualified name changes. Final smoke (4.12.3) walks every page that consumes them. |
| 11 | WebUI's GameDataService.cs references types that live in Application (DanData, MusicDetail) which would create an illegal WebUI→Application dependency | Phase 4.6 explicitly relocates DanData and MusicDetail from `Application/ServerData/` to `Contracts.AdminApi/ServerData/`. Phase 4.6 Step 1 audits whether other ServerData types need the same treatment — expand the move list if so. |
| 12 | CLAUDE.md drifts from reality (e.g., describes `IGameDataCatalog` when PR2 actually kept `IGameDataService`) | Phase 4.10 Step 3 explicitly cross-references the rewrite against PR2's actual landings before committing. The code is the source of truth. |
| 13 | Operator's existing tooling (CI, scripts) breaks because the legacy `TaikoWebUI/TaikoWebUI.sln` was depended on | Phase 4.9.3 Step 1 confirms the file is a duplicate. Repo CI uses `.slnx`. If a CI workflow targets `TaikoWebUI/TaikoWebUI.sln` explicitly, fix the workflow in the same PR. Read `.github/workflows/*.yml` if present. |

---

## Definition of done for PR4

- [ ] All 12 projects build via `dotnet build` from repo root with 0 errors.
- [ ] `Contracts.AdminApi/` exists with `ViewModels/` (13 files), `Requests/` (8 files), `Responses/` (5 files), `ServerData/` (2 files: DanData, MusicDetail), `Converters/PlaySettingConverter.cs`, `GlobalUsings.cs`, `Contracts.AdminApi.csproj`.
- [ ] `Contracts.AdminApi.csproj` references **only** `Domain` (project) and `Throw` (package); no other project or package references.
- [ ] `SharedProject/` folder is gone. No `SharedProject.csproj` anywhere in the tree.
- [ ] `TaikoWebUI/TaikoWebUI.sln` is gone.
- [ ] `TaikoLocalServer.slnx` lists exactly 12 projects with no `SharedProject` entry and one `Contracts.AdminApi` entry.
- [ ] `TaikoWebUI.csproj` ProjectReference is `..\Contracts.AdminApi\Contracts.AdminApi.csproj` (single project reference).
- [ ] `Adapters.AdminApi.csproj` ProjectReferences include `..\Contracts.AdminApi\Contracts.AdminApi.csproj`.
- [ ] `Application.csproj` ProjectReferences include `..\Contracts.AdminApi\Contracts.AdminApi.csproj`.
- [ ] No `*.cs`, `*.razor`, `*.csproj`, or `*.slnx` file in the tree contains the substring `SharedProject` (outside `docs/`).
- [ ] Server's startup logs show `IGameDataCatalog` initialization (or `IGameDataService` if PR2 kept the old name) without exceptions.
- [ ] Every WebUI page in the smoke list (4.12.3 Step 2) renders.
- [ ] Login + SetFavorite round-trips return 2xx.
- [ ] `dotnet publish -c Release` produces `Host/bin/Release/net10.0/win-x64/publish/TaikoLocalServer.exe`; published exe boots and serves the Dashboard.
- [ ] `CLAUDE.md` describes the 12-project layout, the new `Adapter contract` section, the `dotnet ef` command targeting `Infrastructure`, and the `<AssemblyName>TaikoLocalServer</AssemblyName>` rename trick.
- [ ] `README.md` (and `Host/README.md` if present) contain no stale `TaikoLocalServer/`, `GameDatabase/`, or `SharedProject/` path references in operator-facing instructions.
- [ ] Branch `dev/clean-arch-contracts-webui` is pushed and (if user-authorized) a PR is open.
