# AC15 Pure Utilities And Low-Risk Readback Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Extract duplicated AC15 byte packing, crown packing, self-best response building, and Taikojuku pack selection without changing persistence ownership.

**Architecture:** Add pure services that consume canonical records and era profiles. Blue and Green handlers/controllers map their current EF/catalog rows into canonical records, then keep wire placement in adapter code.

**Tech Stack:** C# 13 static services, .NET 10, xUnit, existing ASP.NET Core controllers for crown endpoints.

---

## File Structure

Create:

- `Application/Ac15/Ac15ProtocolBytes.cs` - fixed bitset, two-bit, and ten-bit packing helpers.
- `Application/Ac15/Ac15CrownState.cs` - shared crown wire-state enum.
- `Application/Ac15/Ac15BestRow.cs` - canonical best-score row.
- `Application/Ac15/Ac15CrownService.cs` - endpoint-agnostic crown body builder.
- `Application/Ac15/Ac15SelfBestService.cs` - requested-song self-best response builder.
- `Application/Ac15/Ac15TaikojukuService.cs` - requested Dan slot filtering, fallback, and pack projection.
- `Tests/Ac15/Ac15ProtocolBytesTests.cs`
- `Tests/Ac15/Ac15CrownServiceTests.cs`
- `Tests/Ac15/Ac15SelfBestServiceTests.cs`
- `Tests/Ac15/Ac15TaikojukuServiceTests.cs`

Modify:

- `Application/Common/BlueProtocolBytes.cs`
- `Application/Common/GreenProtocolBytes.cs`
- `Application/Common/BlueCrownState.cs`
- `Application/Common/GreenCrownState.cs`
- `Application/Common/BlueCrownResponseBuilder.cs`
- `Application/Common/GreenCrownResponseBuilder.cs`
- `Adapters.GameProtocol.Blue/Controllers/CrownsDataController.cs`
- `Adapters.GameProtocol.Green/Controllers/CrownsDataController.cs`
- `Application/Handlers/GetSelfBestQuery.Blue.cs`
- `Application/Handlers/GetSelfBestQuery.Green.cs`
- `Application/Handlers/GetTaikojukuQuery.Blue.cs`
- `Application/Handlers/GetTaikojukuQuery.Green.cs`

## Task 1: Shared Protocol Byte Helpers

**Files:**
- Create: `Tests/Ac15/Ac15ProtocolBytesTests.cs`
- Create: `Application/Ac15/Ac15ProtocolBytes.cs`
- Create: `Application/Ac15/Ac15CrownState.cs`
- Modify: `Application/Common/BlueProtocolBytes.cs`
- Modify: `Application/Common/GreenProtocolBytes.cs`
- Modify: `Application/Common/BlueCrownState.cs`
- Modify: `Application/Common/GreenCrownState.cs`

- [ ] **Step 1: Write failing byte-helper tests**

Create `Tests/Ac15/Ac15ProtocolBytesTests.cs`:

```csharp
using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15ProtocolBytesTests
{
    [Fact]
    public void CreateFixedBitset_SetsLittleEndianBitsAndIgnoresOutOfRangeIds()
    {
        var bytes = Ac15ProtocolBytes.CreateFixedBitset([0, 1, 7, 8, 1023, 1024], 128);

        Assert.Equal(128, bytes.Length);
        Assert.Equal(0b1000_0011, bytes[0]);
        Assert.Equal(0b0000_0001, bytes[1]);
        Assert.Equal(0b1000_0000, bytes[127]);
    }

    [Fact]
    public void SetBits_NormalizesExistingBufferAndSkipsOutOfRangeIds()
    {
        var bytes = Ac15ProtocolBytes.SetBits([0b0000_0001], [1, 16], 2);

        Assert.Equal([0b0000_0011, 0b0000_0000], bytes);
    }

    [Fact]
    public void PackTwoBitValues_PacksValuesLittleEndian()
    {
        var bytes = Ac15ProtocolBytes.PackTwoBitValues([1, 2, 3, 0, 1], 18);

        Assert.Equal(18, bytes.Length);
        Assert.Equal(0b0011_1001, bytes[0]);
        Assert.Equal(0b0000_0001, bytes[1]);
    }

    [Fact]
    public void BuildCrownValue_PacksFiveCourseStates()
    {
        var value = Ac15ProtocolBytes.BuildCrownValue(
            Ac15CrownState.Clear,
            Ac15CrownState.FullCombo,
            Ac15CrownState.FullCombo,
            Ac15CrownState.None,
            Ac15CrownState.Clear);

        Assert.Equal((ushort)0b10_00_11_11_10, value);
    }

    [Fact]
    public void PackTenBitValues_CreatesFixedBodyAndMasksValues()
    {
        var values = new ushort[1024];
        values[0] = 0xffff;
        values[1023] = 0x03ff;

        var bytes = Ac15ProtocolBytes.PackTenBitValues(values, byteCount: 1280, maxValues: 1024);

        Assert.Equal(1280, bytes.Length);
        Assert.Equal(0x03ff, ReadTenBitValue(bytes, 0));
        Assert.Equal(0x03ff, ReadTenBitValue(bytes, 1023));
    }

    private static ushort ReadTenBitValue(byte[] packed, int songIndex)
    {
        ushort value = 0;
        var bitOffset = songIndex * 10;
        for (var bit = 0; bit < 10; bit++)
        {
            var absoluteBit = bitOffset + bit;
            if ((packed[absoluteBit >> 3] & (1 << (absoluteBit & 7))) != 0)
            {
                value |= (ushort)(1 << bit);
            }
        }

        return value;
    }
}
```

- [ ] **Step 2: Run tests and verify failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15ProtocolBytesTests"
```

Expected: compile failure because `Ac15ProtocolBytes` and `Ac15CrownState` do not exist.

- [ ] **Step 3: Add shared byte helpers**

Create `Application/Ac15/Ac15CrownState.cs`:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public enum Ac15CrownState : ushort
{
    None = 0,
    Clear = 2,
    FullCombo = 3
}
```

Create `Application/Ac15/Ac15ProtocolBytes.cs`:

```csharp
using TaikoLocalServer.Application.Common;

namespace TaikoLocalServer.Application.Ac15;

public static class Ac15ProtocolBytes
{
    public static byte[] CreateFixedBitset(IEnumerable<uint> enabledIds, int byteCount)
        => BitsetCodec.Encode(enabledIds, byteCount);

    public static byte[] FixedOrZero(byte[]? source, int byteCount)
        => BitsetCodec.Normalize(source, byteCount);

    public static byte[] OrBitsets(byte[] left, byte[] right, int byteCount)
    {
        var result = FixedOrZero(left, byteCount);
        var normalizedRight = FixedOrZero(right, byteCount);
        for (var i = 0; i < result.Length; i++)
        {
            result[i] |= normalizedRight[i];
        }

        return result;
    }

    public static byte[] SetBits(byte[]? source, IEnumerable<uint> ids, int byteCount)
    {
        var result = FixedOrZero(source, byteCount);
        var maxBits = byteCount * 8;
        foreach (var id in ids)
        {
            if (id >= maxBits)
            {
                continue;
            }

            result[id >> 3] |= (byte)(1 << ((int)id & 7));
        }

        return result;
    }

    public static byte[] PackTwoBitValues(IEnumerable<uint> values, int byteCount)
    {
        var result = new byte[byteCount];
        var index = 0;

        foreach (var value in values)
        {
            if (((index * 2) >> 3) >= byteCount)
            {
                break;
            }

            SetTwoBitValue(result, index, value);
            index++;
        }

        return result;
    }

    public static void SetTwoBitValue(byte[] buffer, int index, uint value)
    {
        var masked = value & 0b11;
        var bitOffset = index * 2;

        for (var bit = 0; bit < 2; bit++)
        {
            var absoluteBit = bitOffset + bit;
            var byteIndex = absoluteBit >> 3;
            if ((uint)byteIndex >= (uint)buffer.Length)
            {
                return;
            }

            var mask = (byte)(1 << (absoluteBit & 7));
            if ((masked & (1u << bit)) != 0)
            {
                buffer[byteIndex] |= mask;
            }
            else
            {
                buffer[byteIndex] &= (byte)~mask;
            }
        }
    }

    public static ushort BuildCrownValue(
        Ac15CrownState easy,
        Ac15CrownState normal,
        Ac15CrownState hard,
        Ac15CrownState oni,
        Ac15CrownState uraOni)
    {
        return (ushort)(
            (((ushort)easy & 3) << 0) |
            (((ushort)normal & 3) << 2) |
            (((ushort)hard & 3) << 4) |
            (((ushort)oni & 3) << 6) |
            (((ushort)uraOni & 3) << 8));
    }

    public static byte[] PackTenBitValues(IReadOnlyList<ushort> values, int byteCount, int maxValues)
    {
        var result = new byte[byteCount];

        for (var valueIndex = 0; valueIndex < Math.Min(maxValues, values.Count); valueIndex++)
        {
            var value = values[valueIndex] & 0x03ff;
            var bitOffset = valueIndex * 10;

            for (var bit = 0; bit < 10; bit++)
            {
                if ((value & (1 << bit)) == 0)
                {
                    continue;
                }

                var absoluteBit = bitOffset + bit;
                result[absoluteBit >> 3] |= (byte)(1 << (absoluteBit & 7));
            }
        }

        return result;
    }
}
```

- [ ] **Step 4: Delegate Blue and Green wrappers to shared helpers**

Replace the duplicated methods in `Application/Common/BlueProtocolBytes.cs` and `Application/Common/GreenProtocolBytes.cs` with calls to `Ac15ProtocolBytes`, keeping all public method names stable. Add `using TaikoLocalServer.Application.Ac15;` to both files.

For `Application/Common/BlueCrownState.cs`, replace the file content with:

```csharp
using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Common;

public enum BlueCrownState : ushort
{
    None = Ac15CrownState.None,
    Clear = Ac15CrownState.Clear,
    FullCombo = Ac15CrownState.FullCombo
}
```

For `Application/Common/GreenCrownState.cs`, replace the file content with:

```csharp
using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Common;

public enum GreenCrownState : ushort
{
    None = Ac15CrownState.None,
    Clear = Ac15CrownState.Clear,
    FullCombo = Ac15CrownState.FullCombo
}
```

- [ ] **Step 5: Run byte tests and existing protocol tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15ProtocolBytesTests|FullyQualifiedName~GreenProtocolBytesTests|FullyQualifiedName~BlueCrownsDataTests"
```

Expected: PASS.

## Task 2: Crown Service And Controller Migration

**Files:**
- Create: `Application/Ac15/Ac15BestRow.cs`
- Create: `Application/Ac15/Ac15CrownService.cs`
- Create: `Tests/Ac15/Ac15CrownServiceTests.cs`
- Modify: `Application/Common/BlueCrownResponseBuilder.cs`
- Modify: `Application/Common/GreenCrownResponseBuilder.cs`
- Modify: `Adapters.GameProtocol.Blue/Controllers/CrownsDataController.cs`
- Modify: `Adapters.GameProtocol.Green/Controllers/CrownsDataController.cs`

- [ ] **Step 1: Write failing crown service tests**

Create `Tests/Ac15/Ac15CrownServiceTests.cs`:

```csharp
using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15CrownServiceTests
{
    [Fact]
    public void BuildInflatedBody_EmptyRowsProduceFixedZeroBody()
    {
        var packed = Ac15CrownService.BuildInflatedBody(
            [],
            validSongNoes: [101u],
            Ac15EraProfiles.Blue.Limits);

        Assert.Equal(Ac15EraProfiles.Blue.Limits.CrownPackedBytes, packed.Length);
        Assert.All(packed, value => Assert.Equal(0, value));
    }

    [Fact]
    public void BuildInflatedBody_PacksBestCrownPerCourseAndIncludesShin()
    {
        var rows = new[]
        {
            new Ac15BestRow(101, Difficulty.Easy, false, 1000, 80, CrownType.Clear),
            new Ac15BestRow(101, Difficulty.Easy, true, 2000, 90, CrownType.Gold),
            new Ac15BestRow(999, Difficulty.Easy, false, 3000, 91, CrownType.Dondaful)
        };

        var packed = Ac15CrownService.BuildInflatedBody(rows, validSongNoes: [101u], Ac15EraProfiles.Blue.Limits);

        Assert.Equal((ushort)0b0000000011, ReadTenBitValue(packed, 101));
        Assert.Equal(0, ReadTenBitValue(packed, 999));
    }

    private static ushort ReadTenBitValue(byte[] packed, int songIndex)
    {
        ushort value = 0;
        var bitOffset = songIndex * 10;
        for (var bit = 0; bit < 10; bit++)
        {
            var absoluteBit = bitOffset + bit;
            if ((packed[absoluteBit >> 3] & (1 << (absoluteBit & 7))) != 0)
            {
                value |= (ushort)(1 << bit);
            }
        }

        return value;
    }
}
```

- [ ] **Step 2: Run tests and verify failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15CrownServiceTests"
```

Expected: compile failure because `Ac15BestRow` and `Ac15CrownService` do not exist.

- [ ] **Step 3: Add canonical best row and crown service**

Create `Application/Ac15/Ac15BestRow.cs`:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public sealed record Ac15BestRow(
    uint SongId,
    Difficulty Difficulty,
    bool IsShin,
    uint BestScore,
    uint BestRate,
    CrownType BestCrown);
```

Create `Application/Ac15/Ac15CrownService.cs`:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public static class Ac15CrownService
{
    public static byte[] BuildInflatedBody(
        IEnumerable<Ac15BestRow> bestRows,
        IEnumerable<uint> validSongNoes,
        Ac15ProtocolLimits limits)
    {
        var values = new ushort[limits.CrownSongCount];
        var validSongs = validSongNoes
            .Where(songNo => songNo < limits.CrownSongCount)
            .ToHashSet();

        foreach (var group in bestRows.GroupBy(row => row.SongId))
        {
            if (!validSongs.Contains(group.Key))
            {
                continue;
            }

            var easy = Ac15CrownState.None;
            var normal = Ac15CrownState.None;
            var hard = Ac15CrownState.None;
            var oni = Ac15CrownState.None;
            var ura = Ac15CrownState.None;

            foreach (var row in group)
            {
                var state = MapCrownState(row.BestCrown);
                switch (row.Difficulty)
                {
                    case Difficulty.Easy:
                        easy = Max(easy, state);
                        break;
                    case Difficulty.Normal:
                        normal = Max(normal, state);
                        break;
                    case Difficulty.Hard:
                        hard = Max(hard, state);
                        break;
                    case Difficulty.Oni:
                        oni = Max(oni, state);
                        break;
                    case Difficulty.UraOni:
                        ura = Max(ura, state);
                        break;
                }
            }

            values[group.Key] = Ac15ProtocolBytes.BuildCrownValue(easy, normal, hard, oni, ura);
        }

        return Ac15ProtocolBytes.PackTenBitValues(values, limits.CrownPackedBytes, limits.CrownSongCount);
    }

    public static Ac15CrownState MapCrownState(CrownType crown) => crown switch
    {
        CrownType.Clear => Ac15CrownState.Clear,
        CrownType.Gold => Ac15CrownState.FullCombo,
        CrownType.Dondaful => Ac15CrownState.FullCombo,
        _ => Ac15CrownState.None
    };

    private static Ac15CrownState Max(Ac15CrownState left, Ac15CrownState right)
        => left >= right ? left : right;
}
```

- [ ] **Step 4: Update existing crown builders and controllers**

Update `BlueCrownResponseBuilder.BuildInflatedBody` and `GreenCrownResponseBuilder.BuildInflatedBody` to map EF rows to `Ac15BestRow` and call `Ac15CrownService.BuildInflatedBody`.

Use this mapping in the Blue builder:

```csharp
var canonicalRows = bestRows.Select(row => new Ac15BestRow(
    row.SongId,
    row.Difficulty,
    row.IsShin,
    row.BestScore,
    row.BestRate,
    row.BestCrown));
var validSongNoes = blue.BlueMusicInfos.Keys;
return Ac15CrownService.BuildInflatedBody(canonicalRows, validSongNoes, Ac15EraProfiles.Blue.Limits);
```

Use this mapping in the Green builder:

```csharp
var canonicalRows = bestRows.Select(row => new Ac15BestRow(
    row.SongId,
    row.Difficulty,
    row.IsShin,
    row.BestScore,
    row.BestRate,
    row.BestCrown));
var validSongNoes = green.GreenMusicInfos.Keys;
return Ac15CrownService.BuildInflatedBody(canonicalRows, validSongNoes, Ac15EraProfiles.Green.Limits);
```

Then update both `CrownsDataController` files to keep querying era-specific tables but rely on the updated builder. Do not move gzip or wire response creation into `Application/Ac15`.

- [ ] **Step 5: Run crown tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15CrownServiceTests|FullyQualifiedName~BlueCrownsDataTests|FullyQualifiedName~GreenProtocolBytesTests"
```

Expected: PASS.

## Task 3: Self-Best Service

**Files:**
- Create: `Application/Ac15/Ac15SelfBestService.cs`
- Create: `Tests/Ac15/Ac15SelfBestServiceTests.cs`
- Modify: `Application/Handlers/GetSelfBestQuery.Blue.cs`
- Modify: `Application/Handlers/GetSelfBestQuery.Green.cs`

- [ ] **Step 1: Write failing self-best service tests**

Create `Tests/Ac15/Ac15SelfBestServiceTests.cs`:

```csharp
using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15SelfBestServiceTests
{
    [Fact]
    public void BuildResponse_PreservesRequestedSongOrderAndAddsZeroRows()
    {
        var response = Ac15SelfBestService.BuildResponse(
            requestedDifficulty: 2,
            requestedSongs: [103, 102, 101],
            bestRows:
            [
                new Ac15BestRow(102, Difficulty.Normal, false, 222222, 88, CrownType.Gold)
            ]);

        Assert.Equal(1u, response.Result);
        Assert.Equal(2u, response.Level);
        Assert.Equal([103u, 102u, 101u], response.ArySelfbestScores.Select(row => row.SongNo));
        Assert.Equal(0u, response.ArySelfbestScores[0].SelfBestScore);
        Assert.Equal(222222u, response.ArySelfbestScores[1].SelfBestScore);
        Assert.Equal(0u, response.ArySelfbestScores[2].SelfBestScore);
    }

    [Fact]
    public void BuildResponse_SeparatesNormalAndShinRows()
    {
        var response = Ac15SelfBestService.BuildResponse(
            requestedDifficulty: 3,
            requestedSongs: [101],
            bestRows:
            [
                new Ac15BestRow(101, Difficulty.Hard, false, 111111, 77, CrownType.Clear),
                new Ac15BestRow(101, Difficulty.Hard, true, 333333, 99, CrownType.Dondaful)
            ]);

        Assert.Equal(111111u, Assert.Single(response.ArySelfbestScores).SelfBestScore);
        Assert.Equal(333333u, Assert.Single(response.AryShinSelfbestScores).SelfBestScore);
    }
}
```

- [ ] **Step 2: Run tests and verify failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15SelfBestServiceTests"
```

Expected: compile failure because `Ac15SelfBestService` does not exist.

- [ ] **Step 3: Add self-best service**

Create `Application/Ac15/Ac15SelfBestService.cs`:

```csharp
namespace TaikoLocalServer.Application.Ac15;

public static class Ac15SelfBestService
{
    public static CommonSelfBestResponse BuildResponse(
        uint requestedDifficulty,
        IReadOnlyList<uint> requestedSongs,
        IEnumerable<Ac15BestRow> bestRows)
    {
        var rows = bestRows.ToList();
        var normalRowsBySong = rows
            .Where(row => !row.IsShin)
            .ToDictionary(row => row.SongId);
        var shinRowsBySong = rows
            .Where(row => row.IsShin)
            .ToDictionary(row => row.SongId);

        return new CommonSelfBestResponse
        {
            Result = 1,
            Level = requestedDifficulty,
            ArySelfbestScores = requestedSongs.Select(songNo => BuildRow(songNo, normalRowsBySong)).ToList(),
            AryShinSelfbestScores = requestedSongs.Select(songNo => BuildRow(songNo, shinRowsBySong)).ToList()
        };
    }

    private static CommonSelfBestResponse.SelfBestData BuildRow(
        uint songNo,
        IReadOnlyDictionary<uint, Ac15BestRow> rowsBySong)
    {
        rowsBySong.TryGetValue(songNo, out var best);
        return new CommonSelfBestResponse.SelfBestData
        {
            SongNo = songNo,
            SelfBestScore = best?.BestScore ?? 0,
            SelfBestScoreRate = best?.BestRate ?? 0
        };
    }
}
```

- [ ] **Step 4: Update Blue and Green self-best handlers**

In `Application/Handlers/GetSelfBestQuery.Blue.cs`, keep the Blue query and replace the manual response-building block with:

```csharp
var canonicalRows = bestRows.Select(row => new Ac15BestRow(
    row.SongId,
    row.Difficulty,
    row.IsShin,
    row.BestScore,
    row.BestRate,
    row.BestCrown));

return Ac15SelfBestService.BuildResponse(request.Difficulty, requestedSongs, canonicalRows);
```

In `Application/Handlers/GetSelfBestQuery.Green.cs`, use the same canonical mapping with `SongBestDatumGreen` rows.

Add `using TaikoLocalServer.Application.Ac15;` to both files.

- [ ] **Step 5: Run self-best tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15SelfBestServiceTests|FullyQualifiedName~BlueSelfBestTests|FullyQualifiedName~GreenSelfBest"
```

Expected: PASS.

## Task 4: Taikojuku Service

**Files:**
- Create: `Application/Ac15/Ac15TaikojukuService.cs`
- Create: `Tests/Ac15/Ac15TaikojukuServiceTests.cs`
- Modify: `Application/Handlers/GetTaikojukuQuery.Blue.cs`
- Modify: `Application/Handlers/GetTaikojukuQuery.Green.cs`

- [ ] **Step 1: Write failing Taikojuku service tests**

Create `Tests/Ac15/Ac15TaikojukuServiceTests.cs`:

```csharp
using TaikoLocalServer.Application.Ac15;
using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Tests.Ac15;

public sealed class Ac15TaikojukuServiceTests
{
    [Fact]
    public void BuildResponse_ReturnsRequestedValidSlot()
    {
        var response = Ac15TaikojukuService.BuildResponse(
            requestedDans: [1],
            packs:
            [
                Pack(1, 9, [Song(101, 0), Song(102, 1), Song(103, 2)])
            ],
            musicFileOrder: [Music(101), Music(102), Music(103)],
            validSongNoes: [101, 102, 103],
            Ac15EraProfiles.Green.Limits,
            taikojukuVerupOffset: 0);

        var pack = Assert.Single(response.Packs);
        Assert.Equal(1u, pack.GetDan);
        Assert.Equal(3, pack.Songs.Count);
        Assert.Equal(9u, pack.VerupNo);
    }

    [Fact]
    public void BuildResponse_AllInvalidSlotsFallbackIsCappedToEleven()
    {
        var response = Ac15TaikojukuService.BuildResponse(
            requestedDans: Enumerable.Range(101, 25).Select(value => (uint)value).ToArray(),
            packs: [],
            musicFileOrder: [Music(101), Music(102), Music(103), Music(104)],
            validSongNoes: [101, 102, 103, 104],
            Ac15EraProfiles.Green.Limits,
            taikojukuVerupOffset: 0);

        Assert.Equal(11, response.Packs.Count);
        Assert.All(response.Packs, pack => Assert.InRange(pack.GetDan, 1u, 25u));
    }

    private static Ac15TaikojukuEntry Pack(uint challengeLevel, uint verupNo, IReadOnlyList<Ac15TaikojukuSong> songs) => new()
    {
        UniqueId = 20000 + challengeLevel,
        ChallengeLevel = challengeLevel,
        VerupNo = verupNo,
        Songs = songs
    };

    private static Ac15TaikojukuSong Song(uint songNo, uint level) => new()
    {
        SongNo = songNo,
        Level = level,
        MusicId = songNo.ToString()
    };

    private static Ac15MusicInfoEntry Music(uint songNo) => new()
    {
        SongNo = songNo,
        MusicId = songNo.ToString()
    };
}
```

- [ ] **Step 2: Run tests and verify failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15TaikojukuServiceTests"
```

Expected: compile failure because `Ac15TaikojukuService` does not exist.

- [ ] **Step 3: Add Taikojuku service**

Create `Application/Ac15/Ac15TaikojukuService.cs`:

```csharp
using TaikoLocalServer.Application.Catalog.Ac15;

namespace TaikoLocalServer.Application.Ac15;

public static class Ac15TaikojukuService
{
    public static CommonTaikojukuResponse BuildResponse(
        IReadOnlyList<uint> requestedDans,
        IReadOnlyList<Ac15TaikojukuEntry> packs,
        IReadOnlyList<Ac15MusicInfoEntry> musicFileOrder,
        IReadOnlyCollection<uint> validSongNoes,
        Ac15ProtocolLimits limits,
        uint taikojukuVerupOffset)
    {
        var requestedSlots = GetRequestedSlots(requestedDans, limits);
        var validPacksBySlot = packs
            .Where(pack => IsValidDanSlot(pack.ChallengeLevel, limits))
            .GroupBy(pack => pack.ChallengeLevel)
            .ToDictionary(group => group.Key, group => group.First());

        var selectedPacks = new List<Ac15TaikojukuEntry>();
        foreach (var slot in requestedSlots)
        {
            if (validPacksBySlot.TryGetValue(slot, out var pack))
            {
                selectedPacks.Add(pack);
                continue;
            }

            var fallback = CreateFallbackPack(musicFileOrder, slot, selectedPacks.Count, limits);
            if (fallback is not null)
            {
                selectedPacks.Add(fallback);
            }
        }

        return new CommonTaikojukuResponse
        {
            Result = 1,
            Packs = selectedPacks
                .Select(pack => ToCommonPack(pack, validSongNoes, limits, taikojukuVerupOffset))
                .Where(pack => pack.Songs.Count > 0)
                .ToList()
        };
    }

    private static IReadOnlyList<uint> GetRequestedSlots(IReadOnlyList<uint> requestedDans, Ac15ProtocolLimits limits)
    {
        var requestedSlots = requestedDans
            .Where(slot => IsValidDanSlot(slot, limits))
            .Distinct()
            .ToArray();

        if (requestedSlots.Length > 0 || requestedDans.Count == 0)
        {
            return requestedSlots;
        }

        return Enumerable.Range(1, Math.Min(requestedDans.Count, limits.MaxRequestedTaikojukuSlots))
            .Select(slot => (uint)slot)
            .ToArray();
    }

    private static bool IsValidDanSlot(uint slot, Ac15ProtocolLimits limits)
        => slot >= limits.MinNormalDanId && slot <= limits.MaxNormalDanId;

    private static Ac15TaikojukuEntry? CreateFallbackPack(
        IReadOnlyList<Ac15MusicInfoEntry> musicFileOrder,
        uint slot,
        int index,
        Ac15ProtocolLimits limits)
    {
        var songs = musicFileOrder
            .Skip(index * 3)
            .Take(3)
            .ToArray();
        if (songs.Length == 0)
        {
            songs = musicFileOrder.Take(3).ToArray();
        }

        if (songs.Length == 0)
        {
            return null;
        }

        return new Ac15TaikojukuEntry
        {
            UniqueId = slot,
            ChallengeLevel = slot,
            Songs = songs.Select(song => new Ac15TaikojukuSong
            {
                SongNo = song.SongNo,
                Level = (uint)Math.Min(index, (int)limits.MaxCourseLevel - 1),
                MusicId = song.MusicId
            }).ToArray()
        };
    }

    private static CommonTaikojukuResponse.Pack ToCommonPack(
        Ac15TaikojukuEntry entry,
        IReadOnlyCollection<uint> validSongNoes,
        Ac15ProtocolLimits limits,
        uint taikojukuVerupOffset)
    {
        return new CommonTaikojukuResponse.Pack
        {
            GetDan = entry.ChallengeLevel,
            VerupNo = entry.VerupNo + taikojukuVerupOffset,
            Songs = entry.Songs
                .Where(song => validSongNoes.Contains(song.SongNo))
                .Where(song => song.Level <= limits.MaxCourseLevel - 1)
                .Take(limits.MaxSongsPerTaikojukuPack)
                .Select(song => new CommonTaikojukuResponse.Song
                {
                    SongNo = song.SongNo,
                    Level = song.Level
                })
                .ToList()
        };
    }
}
```

- [ ] **Step 4: Update Blue and Green Taikojuku handlers**

Map existing Blue/Green catalog rows into `Ac15TaikojukuEntry` inside each handler, then call `Ac15TaikojukuService.BuildResponse`.

For Blue, keep the current Blue `VerupNo = 3` behavior by passing `taikojukuVerupOffset: 0` and mapping Blue entries with `VerupNo = entry.VerupNo == 0 ? 3 : entry.VerupNo`. For Green, pass `taikojukuVerupOffset: 1` to preserve `entry.VerupNo + 1`.

Add `using TaikoLocalServer.Application.Ac15;` and `using TaikoLocalServer.Application.Catalog.Ac15;` to both files.

- [ ] **Step 5: Run Taikojuku tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15TaikojukuServiceTests|FullyQualifiedName~BlueTaikojukuTests|FullyQualifiedName~GreenTaikojukuTests"
```

Expected: PASS.

## Task 5: Stage 2 Verification And Commit

**Files:**
- All files listed in this stage.

- [ ] **Step 1: Run focused AC15 and era readback tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~TaikoLocalServer.Tests.Ac15|FullyQualifiedName~BlueCrownsDataTests|FullyQualifiedName~BlueSelfBestTests|FullyQualifiedName~BlueTaikojukuTests|FullyQualifiedName~GreenProtocolBytesTests|FullyQualifiedName~GreenSelfBest|FullyQualifiedName~GreenTaikojukuTests"
```

Expected: PASS.

- [ ] **Step 2: Commit stage 2**

Run:

```powershell
git add Application/Ac15 Application/Common/BlueProtocolBytes.cs Application/Common/GreenProtocolBytes.cs Application/Common/BlueCrownState.cs Application/Common/GreenCrownState.cs Application/Common/BlueCrownResponseBuilder.cs Application/Common/GreenCrownResponseBuilder.cs Adapters.GameProtocol.Blue/Controllers/CrownsDataController.cs Adapters.GameProtocol.Green/Controllers/CrownsDataController.cs Application/Handlers/GetSelfBestQuery.Blue.cs Application/Handlers/GetSelfBestQuery.Green.cs Application/Handlers/GetTaikojukuQuery.Blue.cs Application/Handlers/GetTaikojukuQuery.Green.cs Tests/Ac15 Tests/Blue/BlueCrownsDataTests.cs Tests/Blue/BlueSelfBestTests.cs Tests/Blue/BlueTaikojukuTests.cs Tests/Green/GreenProtocolBytesTests.cs Tests/Green/GreenSelfBestMapperTests.cs Tests/Green/GreenTaikojukuTests.cs
git commit -m "Extract AC15 pure readback services"
```
