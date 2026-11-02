# 01 - Contracts and Bitsets

**Goal:** Add the shared DTO and bitset primitives required by Green customization without touching persistence schema.

**Files:**
- Modify: `Contracts.AdminApi/ViewModels/Costume.cs`
- Modify: `Contracts.AdminApi/ViewModels/Title.cs`
- Create: `Contracts.AdminApi/ViewModels/Neiro.cs`
- Modify: `Contracts.AdminApi/ViewModels/UserSetting.cs`
- Create: `Contracts.AdminApi/ServerData/Green/Costume.Green.cs`
- Create: `Contracts.AdminApi/ServerData/Green/Title.Green.cs`
- Create: `Contracts.AdminApi/ServerData/Green/Neiro.Green.cs`
- Create: `Application/Common/BitsetCodec.cs`
- Create: `Tests/Green/GreenCustomizationContractTests.cs`
- Create: `Tests/Green/BitsetCodecTests.cs`

## Task 1: DTO Shape

- [ ] **Step 1: Add failing contract tests**

Create `Tests/Green/GreenCustomizationContractTests.cs`:

```csharp
using System.Text.Json;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenCustomizationContractTests
{
    [Fact]
    public void UserSetting_CarriesUnlockedToneIds()
    {
        var setting = new UserSetting
        {
            ToneId = 4,
            UnlockedTone = [0, 4, 7]
        };

        Assert.Equal(new List<uint> { 0, 4, 7 }, setting.UnlockedTone);
    }

    [Fact]
    public void GreenCatalogDtos_CarryOptionalSourceProvenance()
    {
        var costume = new Costume
        {
            CostumeId = 7,
            CostumeType = "unknown",
            CostumeName = string.Empty,
            Source = "ndp"
        };
        var title = new Title
        {
            TitleId = 131,
            TitleName = string.Empty,
            Source = "rewardtitlefiltering"
        };
        var neiro = new Neiro
        {
            NeiroId = 4,
            NeiroName = string.Empty,
            Source = "ndp"
        };

        var json = JsonSerializer.Serialize(new { costume, title, neiro });

        Assert.Contains("ndp", json);
        Assert.Contains("rewardtitlefiltering", json);
        Assert.Contains("\"neiroId\":4", json, StringComparison.OrdinalIgnoreCase);
    }
}
```

- [ ] **Step 2: Run contract tests and confirm failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenCustomizationContractTests"
```

Expected: FAIL because `Neiro`, `UnlockedTone`, and `Source` do not exist.

- [ ] **Step 3: Mark `Costume` partial and keep its current fields**

Modify `Contracts.AdminApi/ViewModels/Costume.cs`:

```csharp
namespace TaikoLocalServer.Contracts.AdminApi.ViewModels;

public partial class Costume
{
    public uint CostumeId { get; set; }

    public string CostumeType { get; init; } = string.Empty;

    public string CostumeName { get; init; } = string.Empty;
    public string CostumeNameEN { get; init; } = string.Empty;
    public string CostumeNameCN { get; init; } = string.Empty;
    public string CostumeNameKO { get; init; } = string.Empty;
}
```

- [ ] **Step 4: Mark `Title` partial and keep its current fields**

Modify `Contracts.AdminApi/ViewModels/Title.cs`:

```csharp
namespace TaikoLocalServer.Contracts.AdminApi.ViewModels;

public partial class Title
{
    public uint TitleId { get; set; }

    public string TitleName { get; init; } = string.Empty;

    public string TitleNameEN { get; init; } = string.Empty;

    public string TitleNameCN { get; init; } = string.Empty;

    public string TitleNameKO { get; init; } = string.Empty;

    public uint TitleRarity { get; init; }

    public override bool Equals(object? obj)
    {
        if (obj is Title title)
        {
            return title.TitleName.Equals(TitleName);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return TitleName.GetHashCode();
    }
}
```

- [ ] **Step 5: Create `Neiro` view model**

Create `Contracts.AdminApi/ViewModels/Neiro.cs`:

```csharp
namespace TaikoLocalServer.Contracts.AdminApi.ViewModels;

public partial class Neiro
{
    public uint NeiroId { get; set; }

    public string NeiroName { get; init; } = string.Empty;

    public string NeiroNameEN { get; init; } = string.Empty;

    public string NeiroNameCN { get; init; } = string.Empty;

    public string NeiroNameKO { get; init; } = string.Empty;
}
```

- [ ] **Step 6: Add Green provenance partials**

Create `Contracts.AdminApi/ServerData/Green/Costume.Green.cs`:

```csharp
namespace TaikoLocalServer.Contracts.AdminApi.ViewModels;

public partial class Costume
{
    public string? Source { get; init; }
}
```

Create `Contracts.AdminApi/ServerData/Green/Title.Green.cs`:

```csharp
namespace TaikoLocalServer.Contracts.AdminApi.ViewModels;

public partial class Title
{
    public string? Source { get; init; }
}
```

Create `Contracts.AdminApi/ServerData/Green/Neiro.Green.cs`:

```csharp
namespace TaikoLocalServer.Contracts.AdminApi.ViewModels;

public partial class Neiro
{
    public string? Source { get; init; }
}
```

- [ ] **Step 7: Add `UnlockedTone` to `UserSetting`**

Modify `Contracts.AdminApi/ViewModels/UserSetting.cs` by inserting this property after `UnlockedTitle`:

```csharp
public List<uint> UnlockedTone { get; set; } = new();
```

- [ ] **Step 8: Run contract tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenCustomizationContractTests"
```

Expected: PASS.

## Task 2: Bitset Codec

- [ ] **Step 1: Add failing bitset tests**

Create `Tests/Green/BitsetCodecTests.cs`:

```csharp
namespace TaikoLocalServer.Tests.Green;

public sealed class BitsetCodecTests
{
    [Fact]
    public void Decode_ReturnsSetIdsInAscendingOrder()
    {
        var bytes = new byte[] { 0b1000_0001, 0b0000_0010 };

        var ids = BitsetCodec.Decode(bytes, byteCount: 2);

        Assert.Equal(new List<uint> { 0, 7, 9 }, ids);
    }

    [Fact]
    public void Encode_IgnoresIdsBeyondCapacity()
    {
        var bytes = BitsetCodec.Encode([0, 7, 8, 1024], byteCount: 2);

        Assert.Equal(new byte[] { 0b1000_0001, 0b0000_0001 }, bytes);
    }

    [Fact]
    public void RoundTrip_HandlesGreenBoundaryIds()
    {
        var titleLast = (uint)(GreenProtocolBytes.TitleFlagBytes * 8 - 1);
        var ids = new uint[] { 0, 1, titleLast };

        var bytes = BitsetCodec.Encode(ids, GreenProtocolBytes.TitleFlagBytes);
        var decoded = BitsetCodec.Decode(bytes, GreenProtocolBytes.TitleFlagBytes);

        Assert.Equal(ids, decoded);
    }

    [Fact]
    public void Normalize_ReturnsFixedWidthCopy()
    {
        var normalized = BitsetCodec.Normalize([1, 2, 3, 4], byteCount: 2);

        Assert.Equal(new byte[] { 1, 2 }, normalized);
    }
}
```

- [ ] **Step 2: Run bitset tests and confirm failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BitsetCodecTests"
```

Expected: FAIL because `BitsetCodec` does not exist.

- [ ] **Step 3: Create `BitsetCodec`**

Create `Application/Common/BitsetCodec.cs`:

```csharp
namespace TaikoLocalServer.Application.Common;

public static class BitsetCodec
{
    public static List<uint> Decode(byte[]? source, int byteCount)
    {
        var bytes = Normalize(source, byteCount);
        var result = new List<uint>();

        for (var byteIndex = 0; byteIndex < bytes.Length; byteIndex++)
        {
            var value = bytes[byteIndex];
            if (value == 0)
            {
                continue;
            }

            for (var bit = 0; bit < 8; bit++)
            {
                if ((value & (1 << bit)) != 0)
                {
                    result.Add((uint)(byteIndex * 8 + bit));
                }
            }
        }

        return result;
    }

    public static byte[] Encode(IEnumerable<uint> ids, int byteCount)
    {
        var result = new byte[byteCount];
        var maxBits = byteCount * 8;

        foreach (var id in ids.Distinct())
        {
            if (id >= maxBits)
            {
                continue;
            }

            result[id >> 3] |= (byte)(1 << ((int)id & 7));
        }

        return result;
    }

    public static byte[] Normalize(byte[]? source, int byteCount)
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

- [ ] **Step 4: Run bitset tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~BitsetCodecTests"
```

Expected: PASS.

- [ ] **Step 5: Run build**

Run:

```powershell
dotnet build
```

Expected: PASS.

- [ ] **Step 6: Commit**

```powershell
git add -- Contracts.AdminApi/ViewModels/Costume.cs Contracts.AdminApi/ViewModels/Title.cs Contracts.AdminApi/ViewModels/Neiro.cs Contracts.AdminApi/ViewModels/UserSetting.cs Contracts.AdminApi/ServerData/Green/Costume.Green.cs Contracts.AdminApi/ServerData/Green/Title.Green.cs Contracts.AdminApi/ServerData/Green/Neiro.Green.cs Application/Common/BitsetCodec.cs Tests/Green/GreenCustomizationContractTests.cs Tests/Green/BitsetCodecTests.cs
git commit -m "Add Green customization contracts and bitset codec"
```
