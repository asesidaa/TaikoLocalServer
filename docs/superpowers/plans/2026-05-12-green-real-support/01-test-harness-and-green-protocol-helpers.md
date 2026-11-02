# 01 - Test Harness And Green Protocol Helpers

**Surface:** Add a test project and Green protocol helper APIs for fixed-width bitsets, 2-bit Dan packing, 10-bit crown packing, and zlib compression.

**Why first:** Later tasks depend on byte-level behavior. These helpers need tests before controller and handler wiring uses them.

**Files:**
- Modify: `Directory.Packages.props`
- Modify: `TaikoLocalServer.slnx`
- Create: `Tests/Tests.csproj`
- Create: `Tests/Green/GreenProtocolBytesTests.cs`
- Create: `Application/Common/GreenProtocolBytes.cs`
- Create: `Application/Common/GreenCrownState.cs`

---

## Task 01.1: Create The Test Project

**Acceptance Criteria:**
- [ ] `Tests/Tests.csproj` exists and references `Application`, `Domain`, `Infrastructure`, and `Adapters.GameProtocol.Green`.
- [ ] `dotnet test` discovers and runs at least one passing test.

**Steps:**

- [ ] **Step 1: Add package versions**

Modify `Directory.Packages.props` and add these `PackageVersion` entries near the other shared package versions:

```xml
<PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
<PackageVersion Include="xunit" Version="2.9.3" />
<PackageVersion Include="xunit.runner.visualstudio" Version="2.8.2" />
```

- [ ] **Step 2: Create `Tests/Tests.csproj`**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <RootNamespace>TaikoLocalServer.Tests</RootNamespace>
    <AssemblyName>TaikoLocalServer.Tests</AssemblyName>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Application\Application.csproj" />
    <ProjectReference Include="..\Domain\Domain.csproj" />
    <ProjectReference Include="..\Infrastructure\Infrastructure.csproj" />
    <ProjectReference Include="..\Adapters.GameProtocol.Green\Adapters.GameProtocol.Green.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 3: Add the test project to `TaikoLocalServer.slnx`**

Add this project entry near the other project entries:

```xml
<Project Path="Tests/Tests.csproj" />
```

- [ ] **Step 4: Create a smoke test**

Create `Tests/Green/GreenProtocolBytesTests.cs`:

```csharp
namespace TaikoLocalServer.Tests.Green;

public sealed class GreenProtocolBytesTests
{
    [Fact]
    public void TestProjectRuns()
    {
        Assert.True(true);
    }
}
```

- [ ] **Step 5: Run tests**

Run: `dotnet test`

Expected: PASS; output includes `Passed!`.

- [ ] **Step 6: Commit**

```bash
git add Directory.Packages.props TaikoLocalServer.slnx Tests/Tests.csproj Tests/Green/GreenProtocolBytesTests.cs
git commit -m "test: add test project for Green support"
```

---

## Task 01.2: Add Fixed Bitset Helpers

**Acceptance Criteria:**
- [ ] Song/tone/title/costume bitsets are fixed-width and little-bit-endian.
- [ ] Out-of-range IDs are ignored.

**Steps:**

- [ ] **Step 1: Replace the smoke test with bitset tests**

Replace `Tests/Green/GreenProtocolBytesTests.cs` with:

```csharp
using TaikoLocalServer.Application.Common;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenProtocolBytesTests
{
    [Fact]
    public void CreateFixedBitset_SetsLittleEndianBits()
    {
        var bytes = GreenProtocolBytes.CreateFixedBitset([0, 1, 7, 8, 1023], 128);

        Assert.Equal(128, bytes.Length);
        Assert.Equal(0b1000_0011, bytes[0]);
        Assert.Equal(0b0000_0001, bytes[1]);
        Assert.Equal(0b1000_0000, bytes[127]);
    }

    [Fact]
    public void CreateFixedBitset_IgnoresOutOfRangeIds()
    {
        var bytes = GreenProtocolBytes.CreateFixedBitset([1024, 2048], 128);

        Assert.Equal(128, bytes.Length);
        Assert.All(bytes, b => Assert.Equal(0, b));
    }
}
```

- [ ] **Step 2: Run the focused tests and confirm failure**

Run: `dotnet test --filter GreenProtocolBytesTests`

Expected: FAIL because `GreenProtocolBytes` does not exist.

- [ ] **Step 3: Add `Application/Common/GreenProtocolBytes.cs`**

```csharp
namespace TaikoLocalServer.Application.Common;

public static class GreenProtocolBytes
{
    public const int SongFlagBytes = 128;
    public const int ToneFlagBytes = 16;
    public const int TitleFlagBytes = 128;
    public const int CostumeFlagBytes = 32;
    public const int DanFlagBytes = 18;
    public const int DanExtraFlagBytes = 36;
    public const int ContentInfoBytes = 32;
    public const int GhostReleaseInfoBytes = 16;
    public const int GhostPlayedSongBytes = 128;
    public const int CrownInflatedBytes = 1280;

    public static byte[] CreateFixedBitset(IEnumerable<uint> enabledIds, int byteCount)
    {
        var result = new byte[byteCount];
        var maxBits = byteCount * 8;

        foreach (var id in enabledIds)
        {
            if (id >= maxBits)
            {
                continue;
            }

            result[id >> 3] |= (byte)(1 << ((int)id & 7));
        }

        return result;
    }

    public static byte[] FixedOrZero(byte[]? source, int byteCount)
    {
        var result = new byte[byteCount];
        if (source is null || source.Length == 0)
        {
            return result;
        }

        Array.Copy(source, result, Math.Min(source.Length, result.Length));
        return result;
    }
}
```

- [ ] **Step 4: Run tests**

Run: `dotnet test --filter GreenProtocolBytesTests`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Application/Common/GreenProtocolBytes.cs Tests/Green/GreenProtocolBytesTests.cs
git commit -m "feat(green): add fixed-width protocol byte helpers"
```

---

## Task 01.3: Add 2-Bit Dan Packing

**Acceptance Criteria:**
- [ ] Each Dan value uses 2 little-endian bits.
- [ ] Output is exactly the requested byte width.
- [ ] Values are masked to `0..3`.

**Steps:**

- [ ] **Step 1: Add tests**

Append to `GreenProtocolBytesTests`:

```csharp
[Fact]
public void PackTwoBitValues_PacksValuesLittleEndian()
{
    var bytes = GreenProtocolBytes.PackTwoBitValues([1, 2, 3, 0, 1], GreenProtocolBytes.DanFlagBytes);

    Assert.Equal(GreenProtocolBytes.DanFlagBytes, bytes.Length);
    Assert.Equal(0b0011_1001, bytes[0]);
    Assert.Equal(0b0000_0001, bytes[1]);
}

[Fact]
public void SetTwoBitValue_UpdatesOnlyRequestedIndex()
{
    var bytes = new byte[GreenProtocolBytes.DanFlagBytes];

    GreenProtocolBytes.SetTwoBitValue(bytes, 0, 1);
    GreenProtocolBytes.SetTwoBitValue(bytes, 1, 2);

    Assert.Equal(0b0000_1001, bytes[0]);
}
```

- [ ] **Step 2: Run the focused tests and confirm failure**

Run: `dotnet test --filter GreenProtocolBytesTests`

Expected: FAIL because `PackTwoBitValues` and `SetTwoBitValue` do not exist.

- [ ] **Step 3: Add methods to `GreenProtocolBytes`**

```csharp
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
```

- [ ] **Step 4: Run tests**

Run: `dotnet test --filter GreenProtocolBytesTests`

Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Application/Common/GreenProtocolBytes.cs Tests/Green/GreenProtocolBytesTests.cs
git commit -m "feat(green): add two-bit dan flag packing"
```

---

## Task 01.4: Add 10-Bit Crown Packing And Zlib Compression

**Acceptance Criteria:**
- [ ] 1024 crown values pack into exactly 1280 bytes.
- [ ] Five 2-bit course states pack into one 10-bit song value.
- [ ] Zlib compression round-trips through `ZLibStream`.

**Steps:**

- [ ] **Step 1: Add tests**

Append to `GreenProtocolBytesTests`:

```csharp
using System.IO.Compression;
using TaikoLocalServer.Domain.Enums;

[Fact]
public void BuildGreenCrownValue_PacksFiveCourseStates()
{
    var value = GreenProtocolBytes.BuildGreenCrownValue(
        GreenCrownState.Clear,
        GreenCrownState.FullCombo,
        GreenCrownState.Dondaful,
        GreenCrownState.None,
        GreenCrownState.Clear);

    Assert.Equal((ushort)0b01_00_11_10_01, value);
}

[Fact]
public void PackGreenCrowns_Creates1280ByteBody()
{
    var values = new ushort[1024];
    values[0] = GreenProtocolBytes.BuildGreenCrownValue(
        GreenCrownState.Clear,
        GreenCrownState.None,
        GreenCrownState.None,
        GreenCrownState.None,
        GreenCrownState.None);
    values[1023] = 0x03ff;

    var packed = GreenProtocolBytes.PackGreenCrowns(values);

    Assert.Equal(GreenProtocolBytes.CrownInflatedBytes, packed.Length);
    Assert.Equal(0b0000_0001, packed[0]);
    Assert.NotEqual(0, packed[^1]);
}

[Fact]
public void CompressZlib_RoundTripsCrownBody()
{
    var body = new byte[GreenProtocolBytes.CrownInflatedBytes];
    body[0] = 0x39;
    body[^1] = 0x7f;

    var compressed = GreenProtocolBytes.CompressZlib(body);
    using var input = new MemoryStream(compressed);
    using var zlib = new ZLibStream(input, CompressionMode.Decompress);
    using var output = new MemoryStream();
    zlib.CopyTo(output);

    Assert.Equal(body, output.ToArray());
}
```

- [ ] **Step 2: Run the focused tests and confirm failure**

Run: `dotnet test --filter GreenProtocolBytesTests`

Expected: FAIL because `GreenCrownState` and crown methods do not exist.

- [ ] **Step 3: Add `Application/Common/GreenCrownState.cs`**

```csharp
namespace TaikoLocalServer.Application.Common;

public enum GreenCrownState : ushort
{
    None = 0,
    Clear = 1,
    FullCombo = 2,
    Dondaful = 3
}
```

- [ ] **Step 4: Add crown methods to `GreenProtocolBytes`**

```csharp
using System.IO.Compression;

public static ushort BuildGreenCrownValue(
    GreenCrownState easy,
    GreenCrownState normal,
    GreenCrownState hard,
    GreenCrownState oni,
    GreenCrownState uraOni)
{
    return (ushort)(
        (((ushort)easy & 3) << 0) |
        (((ushort)normal & 3) << 2) |
        (((ushort)hard & 3) << 4) |
        (((ushort)oni & 3) << 6) |
        (((ushort)uraOni & 3) << 8));
}

public static byte[] PackGreenCrowns(IReadOnlyList<ushort> songValues)
{
    var result = new byte[CrownInflatedBytes];

    for (var songIndex = 0; songIndex < Math.Min(1024, songValues.Count); songIndex++)
    {
        var value = songValues[songIndex] & 0x03ff;
        var bitOffset = songIndex * 10;

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

public static byte[] CompressZlib(byte[] body)
{
    using var output = new MemoryStream();
    using (var zlib = new ZLibStream(output, CompressionLevel.SmallestSize, leaveOpen: true))
    {
        zlib.Write(body, 0, body.Length);
    }

    return output.ToArray();
}
```

Place `using System.IO.Compression;` at the top of `GreenProtocolBytes.cs`.

- [ ] **Step 5: Run tests and build**

Run:

```bash
dotnet test --filter GreenProtocolBytesTests
dotnet build
```

Expected: both PASS.

- [ ] **Step 6: Commit**

```bash
git add Application/Common/GreenProtocolBytes.cs Application/Common/GreenCrownState.cs Tests/Green/GreenProtocolBytesTests.cs
git commit -m "feat(green): add crown packing and zlib helpers"
```
