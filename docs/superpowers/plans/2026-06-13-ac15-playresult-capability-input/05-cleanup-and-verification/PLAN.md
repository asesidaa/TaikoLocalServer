# AC15 Playresult Cleanup and Verification Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Remove transitional AC15 `CommonPlayResultData` usage, delete the temporary bridge, confirm Mapperly warning cleanup, and run final regression verification.

**Architecture:** The final boundary has Nijiiro using `UpdatePlayResultCommand` plus `CommonPlayResultData`, and AC15 using `UpdateAc15PlayResultCommand` plus `Ac15PlayResultEnvelope`. Shared AC15 writers and special helpers do not accept `CommonPlayResultData`.

**Tech Stack:** ripgrep audits, dotnet test, dotnet build, Mapperly warning review, git diff review.

---

## Files

- Delete: `Application/Dtos/Ac15/Ac15PlayResultCommonBridge.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand*.cs` only if audits find remaining AC15 `CommonPlayResultData` references
- Modify: `Application/Ac15/*.cs` only if audits find remaining shared-writer `CommonPlayResultData` references
- Modify: `Tests/**/*.cs` only for AC15 tests still constructing `CommonPlayResultData`

### Task 1: Remove Temporary Bridge

**Files:**
- Delete: `Application/Dtos/Ac15/Ac15PlayResultCommonBridge.cs`

- [ ] **Step 1: Delete the bridge file**

Use `apply_patch`:

```patch
*** Begin Patch
*** Delete File: Application/Dtos/Ac15/Ac15PlayResultCommonBridge.cs
*** End Patch
```

- [ ] **Step 2: Search for bridge references**

Run:

```powershell
rg -n "Ac15PlayResultCommonBridge" Application Tests Adapters.GameProtocol.Blue Adapters.GameProtocol.Green Adapters.GameProtocol.Yellow Adapters.GameProtocol.Red
```

Expected: no matches.

### Task 2: Audit AC15 Common DTO References

**Files:**
- Modify only files with incorrect remaining AC15 usage.

- [ ] **Step 1: Search shared AC15 code**

Run:

```powershell
rg -n "CommonPlayResultData" Application\\Ac15 Application\\Common\\BlueBattleStateExtensions.cs Application\\Handlers\\UpdatePlayResultCommand.Blue.cs Application\\Handlers\\UpdatePlayResultCommand.Green.cs Application\\Handlers\\UpdatePlayResultCommand.Yellow.cs Application\\Handlers\\UpdatePlayResultCommand.Red.cs Application\\Handlers\\UpdatePlayResultCommand.BlueBattle.cs Application\\Handlers\\UpdatePlayResultCommand.BlueTokkun.cs Application\\Handlers\\UpdatePlayResultCommand.YellowTokkun.cs
```

Expected: no matches in AC15 shared writers or AC15 handler partials. `UpdatePlayResultCommand.Nijiiro.cs` may still use `CommonPlayResultData` and must remain unchanged.

- [ ] **Step 2: Fix any remaining AC15 handler usage**

If a remaining match is in an AC15 handler, replace it with the matching capability type:

```csharp
Ac15PlayResultEnvelope
Ac15StageResult
Ac15ProfileMutationFacts
Ac15TokkunPlayResult
Ac15BlueBattlePlayResult
Ac15GreenGhostPlayResult
```

Run the search from Step 1 again.

Expected: no matches in the audited AC15 files.

- [ ] **Step 3: Search adapter mapper targets**

Run:

```powershell
rg -n "public static partial CommonPlayResultData Map|public static CommonPlayResultData Map|CommonPlayResultData\\.StageData" Adapters.GameProtocol.Blue\\Mappers\\PlayResultMappers.cs Adapters.GameProtocol.Green\\Mappers\\PlayResultMappers.cs Adapters.GameProtocol.Yellow\\Mappers\\PlayResultMappers.cs Adapters.GameProtocol.Red\\Mappers\\PlayResultMappers.cs
```

Expected: no matches.

Nijiiro mapper files under `Adapters.GameProtocol.WwR08` and `Adapters.GameProtocol.CnR00` may still target `CommonPlayResultData`.

### Task 3: Decide Common DTO Field Cleanup Boundary

**Files:**
- Modify: `Application/Dtos/CommonPlayResultData.BlueBattle.cs`
- Modify: `Application/Dtos/CommonPlayResultData.BlueTokkun.cs`
- Modify: `Application/Dtos/CommonPlayResultData.Green.cs`
- Modify: `Application/Dtos/CommonPlayResultData.Red.cs`
- Modify: `Application/Dtos/CommonPlayResultData.cs`

- [ ] **Step 1: Search remaining non-AC15 consumers**

Run:

```powershell
rg -n "TokkunStageData|BattleReleaseData|GhostReleaseData|GetDonpoint|GetDonmedal|WaiwaiTutorialFlg|AryChallengeIds|BattleStageData|GhostStageData" Application Adapters.GameProtocol.WwR08 Adapters.GameProtocol.CnR00 Tests
```

Expected: only Nijiiro/common tests and old DTO files remain. Do not delete fields required by Nijiiro mappers or handlers.

- [ ] **Step 2: Leave Common DTO partial cleanup out unless the search proves no consumers**

If any field is still used by Nijiiro, keep the corresponding `CommonPlayResultData` partial file unchanged. If a whole AC15-only partial has no remaining non-AC15 consumers, delete that partial file with `apply_patch`.

Concrete safe rule:

```text
Delete a CommonPlayResultData partial only when `rg` shows every member in that partial has zero references outside the partial file itself.
```

- [ ] **Step 3: Run compile after any DTO cleanup**

Run:

```powershell
dotnet build TaikoLocalServer.slnx
```

Expected: exit `0`.

### Task 4: Mapperly Warning Verification

**Files:**
- Modify AC15 mapper files only if `RMG012` remains for playresult target mappings.

- [ ] **Step 1: Build and capture Mapperly warnings**

Run:

```powershell
dotnet build TaikoLocalServer.slnx
```

Expected: exit `0`.

Inspect output for:

```text
RMG012
PlayResultMappers
```

Expected: no `RMG012` warnings from these files:

```text
Adapters.GameProtocol.Blue/Mappers/PlayResultMappers.cs
Adapters.GameProtocol.Green/Mappers/PlayResultMappers.cs
Adapters.GameProtocol.Yellow/Mappers/PlayResultMappers.cs
Adapters.GameProtocol.Red/Mappers/PlayResultMappers.cs
```

- [ ] **Step 2: Fix remaining playresult target warnings**

If a playresult mapper still emits `RMG012`, add an explicit `MapProperty`, `MapPropertyFromSource`, `MapValue`, or `MapperIgnoreTarget` for the new capability target member that is actually unsupported by that era. Do not weaken `[assembly: MapperDefaults(RequiredMappingStrategy = RequiredMappingStrategy.Target)]`.

Example for an era that does not own Green ghost stage data:

```csharp
[MapperIgnoreTarget(nameof(Ac15StageResult.GreenGhostStage))]
```

Example for a source optional byte array:

```csharp
[MapProperty(nameof(PlayResultRequest.StageData.OptionFlg), nameof(Ac15StageResult.OptionFlg), Use = nameof(MapBytes))]
```

- [ ] **Step 3: Rebuild**

Run:

```powershell
dotnet build TaikoLocalServer.slnx
```

Expected: exit `0` and no AC15 playresult mapper `RMG012` warnings.

### Task 5: Final Behavior Verification

**Files:**
- No additional file edits unless tests expose a behavior regression.

- [ ] **Step 1: Run AC15 writer tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Ac15"
```

Expected: pass.

- [ ] **Step 2: Run era playresult slices**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BluePlayResult|FullyQualifiedName~GreenPlayResult|FullyQualifiedName~YellowPlayResult|FullyQualifiedName~RedPlayResult"
```

Expected: pass.

- [ ] **Step 3: Run special-mode slices**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Tokkun|FullyQualifiedName~BattlePlayResult|FullyQualifiedName~Ghost"
```

Expected: pass.

- [ ] **Step 4: Run broad era regressions**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Blue"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Green"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Yellow"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Red"
```

Expected: all pass.

- [ ] **Step 5: Run full test suite**

Run:

```powershell
dotnet test Tests/Tests.csproj
```

Expected: pass.

- [ ] **Step 6: Run temp-output Host build**

Run:

```powershell
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"
```

Expected: exit `0`.

### Task 6: Final Diff Review and Commit

**Files:**
- All files changed across checkpoints.

- [ ] **Step 1: Review changed file list**

Run:

```powershell
git status --short
```

Expected: only AC15 playresult input, mapper, handler, shared writer, test, and plan files are modified.

- [ ] **Step 2: Review no generated wire files changed**

Run:

```powershell
git status --short -- proto Adapters.GameProtocol.Blue/Wire Adapters.GameProtocol.Green/Wire Adapters.GameProtocol.Yellow/Wire Adapters.GameProtocol.Red/Wire
```

Expected: no output.

- [ ] **Step 3: Commit checkpoint 5**

Run:

```powershell
git add Application Adapters.GameProtocol.Blue Adapters.GameProtocol.Green Adapters.GameProtocol.Yellow Adapters.GameProtocol.Red Tests docs/superpowers/plans/2026-06-13-ac15-playresult-capability-input
git commit -m "Finish AC15 playresult capability input migration"
```

