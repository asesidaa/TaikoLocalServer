# Task 2: Green Dan Helper Functions

**Files:**
- Create: `Application/Common/GreenDanHelpers.cs`
- Test: `Tests/Green/GreenPlayResultHandlerTests.cs`

- [ ] **Step 1: Write failing helper tests**

Add these tests to `Tests/Green/GreenPlayResultHandlerTests.cs`:

```csharp
[Fact]
public void GreenDanHelpers_ClassifiesNormalAndExtraDanIds()
{
    Assert.True(GreenDanHelpers.IsNormalDanId(1));
    Assert.True(GreenDanHelpers.IsNormalDanId(25));
    Assert.False(GreenDanHelpers.IsNormalDanId(26));

    Assert.True(GreenDanHelpers.IsExtraDanId(101));
    Assert.True(GreenDanHelpers.IsExtraDanId(128));
    Assert.False(GreenDanHelpers.IsExtraDanId(100));
}

[Fact]
public void GreenDanHelpers_PacksTwoBitClearGrades()
{
    var flags = GreenDanHelpers.SetPackedGrade(new byte[GreenProtocolBytes.DanFlagBytes], 0, GreenDanClearGrade.NormalClear);
    flags = GreenDanHelpers.SetPackedGrade(flags, 1, GreenDanClearGrade.GoldClear);

    Assert.Equal(GreenDanClearGrade.NormalClear, GreenDanHelpers.GetPackedGrade(flags, 0));
    Assert.Equal(GreenDanClearGrade.GoldClear, GreenDanHelpers.GetPackedGrade(flags, 1));
    Assert.Equal(GreenDanClearGrade.NotClear, GreenDanHelpers.GetPackedGrade(flags, 2));
}

[Fact]
public void GreenDanHelpers_ComputesNextUnclearedNormalDan()
{
    var grades = new Dictionary<uint, GreenDanClearGrade>
    {
        [1] = GreenDanClearGrade.NormalClear,
        [2] = GreenDanClearGrade.GoldClear
    };

    Assert.Equal(3u, GreenDanHelpers.GetNextUnclearedNormalDan(grades));

    for (uint dan = 3; dan <= 25; dan++)
    {
        grades[dan] = GreenDanClearGrade.NormalClear;
    }

    Assert.Equal(25u, GreenDanHelpers.GetNextUnclearedNormalDan(grades));
}
```

- [ ] **Step 2: Run tests and verify they fail**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenDanHelpers"
```

Expected: compile failure because `GreenDanHelpers` does not exist.

- [ ] **Step 3: Add helper implementation**

Create `Application/Common/GreenDanHelpers.cs`:

```csharp
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Application.Common;

public static class GreenDanHelpers
{
    public const uint MinNormalDanId = 1;
    public const uint MaxNormalDanId = 25;
    public const uint MinExtraDanId = 101;
    public const uint MaxKnownExtraDanId = 128;

    public static bool IsNormalDanId(uint danId)
        => danId is >= MinNormalDanId and <= MaxNormalDanId;

    public static bool IsExtraDanId(uint danId)
        => danId is >= MinExtraDanId and <= MaxKnownExtraDanId;

    public static bool IsKnownGreenDanId(uint danId)
        => IsNormalDanId(danId) || IsExtraDanId(danId);

    public static bool IsClear(GreenDanClearGrade grade)
        => grade is GreenDanClearGrade.NormalClear or GreenDanClearGrade.GoldClear;

    public static GreenDanClearGrade ClampGrade(uint value)
        => value >= (uint)GreenDanClearGrade.GoldClear
            ? GreenDanClearGrade.GoldClear
            : (GreenDanClearGrade)value;

    public static uint GetPackedIndex(uint danId)
    {
        if (IsNormalDanId(danId))
        {
            return danId - MinNormalDanId;
        }

        if (IsExtraDanId(danId))
        {
            return danId - MinExtraDanId;
        }

        throw new ArgumentOutOfRangeException(nameof(danId), danId, "Green Dan id must be normal 1..25 or extra 101..128.");
    }

    public static byte[] SetPackedGrade(byte[] source, uint packedIndex, GreenDanClearGrade grade)
    {
        var result = source.ToArray();
        var value = (uint)ClampGrade((uint)grade);
        var bitOffset = (int)packedIndex * 2;

        for (var bit = 0; bit < 2; bit++)
        {
            var absoluteBit = bitOffset + bit;
            var byteIndex = absoluteBit >> 3;
            if ((uint)byteIndex >= (uint)result.Length)
            {
                return result;
            }

            var mask = (byte)(1 << (absoluteBit & 7));
            if ((value & (1u << bit)) != 0)
            {
                result[byteIndex] |= mask;
            }
            else
            {
                result[byteIndex] &= (byte)~mask;
            }
        }

        return result;
    }

    public static GreenDanClearGrade GetPackedGrade(byte[] source, uint packedIndex)
    {
        uint value = 0;
        var bitOffset = (int)packedIndex * 2;
        for (var bit = 0; bit < 2; bit++)
        {
            var absoluteBit = bitOffset + bit;
            var byteIndex = absoluteBit >> 3;
            if ((uint)byteIndex >= (uint)source.Length)
            {
                break;
            }

            if ((source[byteIndex] & (1 << (absoluteBit & 7))) != 0)
            {
                value |= 1u << bit;
            }
        }

        return ClampGrade(value);
    }

    public static uint GetGotDanMax(IReadOnlyDictionary<uint, GreenDanClearGrade> normalGrades)
        => normalGrades
            .Where(row => IsNormalDanId(row.Key) && IsClear(row.Value))
            .Select(row => row.Key)
            .DefaultIfEmpty(0)
            .Max();

    public static uint GetNextUnclearedNormalDan(IReadOnlyDictionary<uint, GreenDanClearGrade> normalGrades)
    {
        for (uint dan = MinNormalDanId; dan <= MaxNormalDanId; dan++)
        {
            if (!normalGrades.TryGetValue(dan, out var grade) || !IsClear(grade))
            {
                return dan;
            }
        }

        return MaxNormalDanId;
    }

    public static uint NormalizeDisplayDan(uint savedDisplayDan, IReadOnlyDictionary<uint, GreenDanClearGrade> normalGrades)
    {
        if (!IsNormalDanId(savedDisplayDan)
            || (normalGrades.TryGetValue(savedDisplayDan, out var grade) && IsClear(grade)))
        {
            return GetNextUnclearedNormalDan(normalGrades);
        }

        return savedDisplayDan;
    }
}
```

- [ ] **Step 4: Run helper tests and verify they pass**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~GreenDanHelpers"
```

Expected: PASS.

- [ ] **Step 5: Commit Task 2**

```powershell
git add Application/Common/GreenDanHelpers.cs Tests/Green/GreenPlayResultHandlerTests.cs
git commit -m "Add Green Dan helper functions"
```

