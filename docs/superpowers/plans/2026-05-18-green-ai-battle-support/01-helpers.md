# Task 1 — Helpers + Unit Tests

**Goal:** Land two pure-function helpers and their unit tests in a single commit. After this task, Task 2 has tested primitives to call.

**Files:**
- Create: `Application/Common/GreenStageModeInterpreter.cs`
- Create: `Application/Common/GreenAiBattleLevels.cs`
- Create: `Tests/Green/GreenStageModeInterpreterTests.cs`
- Create: `Tests/Green/GreenAiBattleLevelsTests.cs`

**Acceptance Criteria:**
- [ ] `GreenStageModeInterpreter.IsShin(uint)` returns true iff input is `1` or `4`.
- [ ] `GreenStageModeInterpreter.IsAiBattle(uint)` returns true iff input is `3` or `4`.
- [ ] `GreenAiBattleLevels.IsCertifiedLevel(uint)` returns true iff input is `1`, `5`, `9`, or `13`.
- [ ] All new tests pass under `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenStageModeInterpreter|FullyQualifiedName~GreenAiBattleLevels"`.

**Verify:** `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenStageModeInterpreter|FullyQualifiedName~GreenAiBattleLevels"` → all green.

---

## Background

The `Application` project's `GlobalUsings.cs` already imports `TaikoLocalServer.Application.Common`. New files in that namespace become available throughout the project with no extra `using` directives.

The `Tests` project's `GlobalUsings.cs` imports `TaikoLocalServer.Application.Common`, `Xunit`, and the rest of the test dependencies. New tests in `Tests/Green/` need no extra `using` lines for these helpers either.

Both helpers are static classes with `uint` inputs (matching the protobuf wire types).

---

## Steps

- [ ] **Step 1: Write the failing test for `GreenStageModeInterpreter`**

Create `Tests/Green/GreenStageModeInterpreterTests.cs`:

```csharp
namespace TaikoLocalServer.Tests.Green;

public sealed class GreenStageModeInterpreterTests
{
    [Theory]
    [InlineData(0u, false)]
    [InlineData(1u, true)]
    [InlineData(2u, false)]
    [InlineData(3u, false)]
    [InlineData(4u, true)]
    [InlineData(5u, false)]
    public void IsShin_ReturnsTrueForOneAndFour(uint stageMode, bool expected)
    {
        Assert.Equal(expected, GreenStageModeInterpreter.IsShin(stageMode));
    }

    [Theory]
    [InlineData(0u, false)]
    [InlineData(1u, false)]
    [InlineData(2u, false)]
    [InlineData(3u, true)]
    [InlineData(4u, true)]
    [InlineData(5u, false)]
    public void IsAiBattle_ReturnsTrueForThreeAndFour(uint stageMode, bool expected)
    {
        Assert.Equal(expected, GreenStageModeInterpreter.IsAiBattle(stageMode));
    }
}
```

- [ ] **Step 2: Run the test to confirm it fails to compile**

Run:

```bash
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenStageModeInterpreterTests"
```

Expected: compile error, `'GreenStageModeInterpreter' does not contain a definition for ...`.

- [ ] **Step 3: Add the `GreenStageModeInterpreter` helper**

Create `Application/Common/GreenStageModeInterpreter.cs`:

```csharp
namespace TaikoLocalServer.Application.Common;

public static class GreenStageModeInterpreter
{
    public static bool IsShin(uint stageMode) => stageMode is 1 or 4;

    public static bool IsAiBattle(uint stageMode) => stageMode is 3 or 4;
}
```

- [ ] **Step 4: Run the test to confirm it passes**

Run:

```bash
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenStageModeInterpreterTests"
```

Expected: 12 tests pass.

- [ ] **Step 5: Write the failing test for `GreenAiBattleLevels`**

Create `Tests/Green/GreenAiBattleLevelsTests.cs`:

```csharp
namespace TaikoLocalServer.Tests.Green;

public sealed class GreenAiBattleLevelsTests
{
    [Theory]
    [InlineData(0u, false)]
    [InlineData(1u, true)]
    [InlineData(2u, false)]
    [InlineData(3u, false)]
    [InlineData(4u, false)]
    [InlineData(5u, true)]
    [InlineData(6u, false)]
    [InlineData(7u, false)]
    [InlineData(8u, false)]
    [InlineData(9u, true)]
    [InlineData(10u, false)]
    [InlineData(11u, false)]
    [InlineData(12u, false)]
    [InlineData(13u, true)]
    [InlineData(14u, false)]
    [InlineData(100u, false)]
    public void IsCertifiedLevel_ReturnsTrueForCanonicalLevels(uint sdCertifiedLevelId, bool expected)
    {
        Assert.Equal(expected, GreenAiBattleLevels.IsCertifiedLevel(sdCertifiedLevelId));
    }
}
```

- [ ] **Step 6: Run the test to confirm it fails to compile**

Run:

```bash
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenAiBattleLevelsTests"
```

Expected: compile error, `The name 'GreenAiBattleLevels' does not exist in the current context`.

- [ ] **Step 7: Add the `GreenAiBattleLevels` helper**

Create `Application/Common/GreenAiBattleLevels.cs`:

```csharp
namespace TaikoLocalServer.Application.Common;

public static class GreenAiBattleLevels
{
    public static bool IsCertifiedLevel(uint sdCertifiedLevelId)
        => sdCertifiedLevelId is 1 or 5 or 9 or 13;
}
```

- [ ] **Step 8: Run both helper test groups**

Run:

```bash
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenStageModeInterpreter|FullyQualifiedName~GreenAiBattleLevels"
```

Expected: 28 tests pass (12 + 16).

- [ ] **Step 9: Confirm no regression in other Green tests**

Run:

```bash
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Green"
```

Expected: all Green tests pass (the new helpers are pure and unreferenced from production code yet, so no behavior change).

- [ ] **Step 10: Commit**

```bash
git -C H:/TaikoLocalServer status --short
git -C H:/TaikoLocalServer add \
  Application/Common/GreenStageModeInterpreter.cs \
  Application/Common/GreenAiBattleLevels.cs \
  Tests/Green/GreenStageModeInterpreterTests.cs \
  Tests/Green/GreenAiBattleLevelsTests.cs
git -C H:/TaikoLocalServer commit -m "$(cat <<'EOF'
Add Green AI Battle StageMode and certified-level helpers

Pure-function helpers used by the Green play-result handler to decode
the StageMode enum (0/1/3/4 → IsShin/IsAiBattle) and identify 正規 AI
difficulty levels (1/5/9/13 — the only sub-levels that record crowns
in AI Battle per the wiki).

Co-Authored-By: Claude Opus 4.7 (1M context) <noreply@anthropic.com>
EOF
)"
```

`git status --short` first lets you verify only the four expected files are staged; the working tree contains unrelated edits that must not land in this commit.
