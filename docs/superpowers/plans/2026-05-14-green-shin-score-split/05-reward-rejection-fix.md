# Task 5: Reward Rejection Fix

**Files:**
- Modify: `Tests/Green/GreenPlayResultHandlerTests.cs`
- Modify: `Application/Handlers/UpdatePlayResultCommand.Green.cs`

## Steps

- [ ] **Step 1: Add failing reward acceptance and rejection tests**

Append these tests to `Tests/Green/GreenPlayResultHandlerTests.cs`:

```csharp
    [Fact]
    public async Task UpdatePlayResult_Green_AcceptsNewlyAwardedRewardIds()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                GetToneNoes = [4],
                GetCostumeNo1s = [1, 43, 3, 44],
                GetTitleNoes = [106, 132, 144, 151, 158, 181],
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 1,
                        StageMode = 0,
                        PlayResult = 1,
                        PlayScore = 229170,
                        GoodCnt = 49,
                        OkCnt = 12,
                        NgCnt = 1,
                        PoundCnt = 66,
                        ComboCnt = 54,
                        HitCnt = 127,
                        OptionFlg = [0, 0],
                        ToneFlg = new byte[16]
                    }
                ]
            }),
            CancellationToken.None);

        var save = await fixture.Context.UserSaveDataGreen.FindAsync(1u);

        Assert.Equal(1u, result);
        Assert.True(BitIsSet(save!.ToneFlg, 4));
        Assert.True(BitIsSet(save.CostumeFlg1, 1));
        Assert.True(BitIsSet(save.CostumeFlg1, 43));
        Assert.True(BitIsSet(save.CostumeFlg1, 44));
        Assert.True(BitIsSet(save.TitleFlg, 106));
        Assert.True(BitIsSet(save.TitleFlg, 181));
    }

    [Fact]
    public async Task UpdatePlayResult_Green_RejectsOutOfRangeRewardIds()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
        fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
        await fixture.Context.SaveChangesAsync();

        var handler = new UpdatePlayResultCommandHandler(
            fixture.Context,
            fixture.Catalog,
            NullLogger<UpdatePlayResultCommandHandler>.Instance);

        var result = await handler.Handle(new UpdatePlayResultCommand(
            1,
            GameEra.Green,
            new CommonPlayResultData
            {
                Baid = 1,
                GetTitleNoes = [(uint)GreenProtocolBytes.TitleFlagBytes * 8],
                AryStageInfoes =
                [
                    new CommonPlayResultData.StageData
                    {
                        SongNo = 101,
                        Level = 1,
                        StageMode = 0,
                        PlayResult = 1,
                        PlayScore = 1000
                    }
                ]
            }),
            CancellationToken.None);

        Assert.Equal(0u, result);
        Assert.Empty(await fixture.Context.SongPlayDataGreen.ToListAsync());
    }

    private static bool BitIsSet(byte[] source, uint id)
    {
        return (source[id >> 3] & (1 << ((int)id & 7))) != 0;
    }
```

- [ ] **Step 2: Run tests and confirm failure**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~UpdatePlayResult_Green_AcceptsNewlyAwardedRewardIds|FullyQualifiedName~UpdatePlayResult_Green_RejectsOutOfRangeRewardIds"
```

Expected: first test fails because the handler requires reward IDs to already be unlocked.

- [ ] **Step 3: Replace already-unlocked reward guard with range guard**

In `Application/Handlers/UpdatePlayResultCommand.Green.cs`, replace this condition:

```csharp
            || !HasOnlyKnownUnlockRewards(saveData, playResultData)
```

with:

```csharp
            || !HasOnlyInRangeUnlockRewards(playResultData)
```

Replace the `HasOnlyKnownUnlockRewards` method with:

```csharp
    private static bool HasOnlyInRangeUnlockRewards(CommonPlayResultData playResultData)
    {
        return AllWithinRange(playResultData.GetToneNoes, GreenProtocolBytes.ToneFlagBytes)
            && AllWithinRange(playResultData.GetCostumeNo1s, GreenProtocolBytes.CostumeFlagBytes)
            && AllWithinRange(playResultData.GetCostumeNo2s, GreenProtocolBytes.CostumeFlagBytes)
            && AllWithinRange(playResultData.GetCostumeNo3s, GreenProtocolBytes.CostumeFlagBytes)
            && AllWithinRange(playResultData.GetCostumeNo4s, GreenProtocolBytes.CostumeFlagBytes)
            && AllWithinRange(playResultData.GetCostumeNo5s, GreenProtocolBytes.CostumeFlagBytes)
            && AllWithinRange(playResultData.GetTitleNoes, GreenProtocolBytes.TitleFlagBytes);
    }

    private static bool AllWithinRange(IEnumerable<uint> ids, int byteCount)
    {
        var maxBits = (uint)(byteCount * 8);
        return ids.All(id => id < maxBits);
    }
```

Keep `HasBit`, `AllAlreadyUnlocked`, and `IsValidCurrentCostume` only if they are still used by the current costume validation. Remove `AllAlreadyUnlocked` if no callers remain.

- [ ] **Step 4: Run reward tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~UpdatePlayResult_Green_AcceptsNewlyAwardedRewardIds|FullyQualifiedName~UpdatePlayResult_Green_RejectsOutOfRangeRewardIds"
```

Expected: pass.

- [ ] **Step 5: Run the captured-shape handler tests together**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~UpdatePlayResult_Green_SavesNormalAndShinBestSeparately|FullyQualifiedName~UpdatePlayResult_Green_AcceptsNewlyAwardedRewardIds"
```

Expected: pass.

- [ ] **Step 6: Commit reward fix**

Run:

```powershell
git add -- Tests/Green/GreenPlayResultHandlerTests.cs Application/Handlers/UpdatePlayResultCommand.Green.cs
git commit -m "Accept valid Green playresult rewards"
```
