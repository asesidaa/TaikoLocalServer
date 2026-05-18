# 05 - Protocol Unlock Verification

**Goal:** Prove Green tone/title unlocks saved through AdminApi are visible to the running game through the correct Green protocol response.

**Files:**
- Modify: `Tests/Green/GreenIdentityHandlerTests.cs`
- Modify: `Tests/Green/GreenUserDataMapperTests.cs`

## Task 1: Handler-Level Unlock Flags

- [ ] **Step 1: Add UserDataQuery regression test**

Append this test to `Tests/Green/GreenIdentityHandlerTests.cs`:

```csharp
[Fact]
public async Task UserData_Green_ReturnsPersistedToneAndTitleUnlockFlags()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
    var save = UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1);
    save.ToneFlg = BitsetCodec.Encode([0, 4], GreenProtocolBytes.ToneFlagBytes);
    save.TitleFlg = BitsetCodec.Encode([10, 131], GreenProtocolBytes.TitleFlagBytes);
    fixture.Context.UserSaveDataGreen.Add(save);
    await fixture.Context.SaveChangesAsync();

    var handler = new UserDataQueryHandler(
        fixture.Context,
        fixture.Catalog,
        NullLogger<UserDataQueryHandler>.Instance,
        Options.Create(new ServerSettings()));

    var response = await handler.Handle(new UserDataQuery(1, GameEra.Green), CancellationToken.None);

    Assert.Equal(GreenProtocolBytes.ToneFlagBytes, response.ToneFlg.Length);
    Assert.Equal(GreenProtocolBytes.TitleFlagBytes, response.TitleFlg.Length);
    Assert.True(BitIsSet(response.ToneFlg, 4));
    Assert.True(BitIsSet(response.TitleFlg, 10));
    Assert.True(BitIsSet(response.TitleFlg, 131));
}
```

- [ ] **Step 2: Run handler test**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~UserData_Green_ReturnsPersistedToneAndTitleUnlockFlags"
```

Expected: PASS. If this fails, repair `Application/Handlers/UserDataQuery.Green.cs` so it assigns:

```csharp
ToneFlg = GreenProtocolBytes.FixedOrZero(saveData.ToneFlg, GreenProtocolBytes.ToneFlagBytes),
TitleFlg = GreenProtocolBytes.FixedOrZero(saveData.TitleFlg, GreenProtocolBytes.TitleFlagBytes),
```

## Task 2: Wire-Level Unlock Fields

- [ ] **Step 1: Add mapper serialization test**

Append this test to `Tests/Green/GreenUserDataMapperTests.cs`:

```csharp
[Fact]
public void UserData_SerializesToneAndTitleFlagsOnWire()
{
    var response = UserDataMappers.Map(new CommonUserDataResponse
    {
        Result = 1,
        ToneFlg = BitsetCodec.Encode([0, 4], GreenProtocolBytes.ToneFlagBytes),
        TitleFlg = BitsetCodec.Encode([10, 131], GreenProtocolBytes.TitleFlagBytes)
    });

    var payload = Serialize(response);

    Assert.True(response.ShouldSerializeToneFlg());
    Assert.True(response.ShouldSerializeTitleFlg());
    Assert.True(ContainsField(payload, 13));
    Assert.True(ContainsField(payload, 14));
}
```

- [ ] **Step 2: Run mapper test**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~UserData_SerializesToneAndTitleFlagsOnWire"
```

Expected: PASS. If this fails, repair `Adapters.GameProtocol.Green/Mappers/UserDataMappers.cs` so it assigns:

```csharp
ToneFlg = common.ToneFlg,
TitleFlg = common.TitleFlg,
```

## Task 3: BAID Non-Field Guard

- [ ] **Step 1: Confirm generated BAIDResponse field shape**

Run:

```powershell
rg -n "message BAIDResponse|tone_flg|title_flg|costume_flg" proto/green/green.proto
```

Expected key lines:

```text
message BAIDResponse
optional bytes costume_flg_1 = 20;
optional bytes costume_flg_2 = 21;
optional bytes costume_flg_3 = 22;
optional bytes costume_flg_4 = 23;
optional bytes costume_flg_5 = 24;
optional bytes tone_flg = 13;
optional bytes title_flg = 14;
```

Interpretation: the `tone_flg` and `title_flg` hits are in `UserDataResponse`, not `BAIDResponse`.

- [ ] **Step 2: Confirm IDA evidence document**

Run:

```powershell
rg -n "BAIDResponse|UserDataResponse|tone_flg|title_flg" proto/green/ida-byte-field-findings.md
```

Expected: `BAIDResponse` documents costume flags and dan/content bytes, while `UserDataResponse` documents `tone_flg` and `title_flg`.

- [ ] **Step 3: Run focused protocol tests**

Run:

```powershell
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~UserData_Green_ReturnsPersistedToneAndTitleUnlockFlags|FullyQualifiedName~UserData_SerializesToneAndTitleFlagsOnWire"
```

Expected: PASS.

- [ ] **Step 4: Build**

Run:

```powershell
dotnet build
```

Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add -- Tests/Green/GreenIdentityHandlerTests.cs Tests/Green/GreenUserDataMapperTests.cs
git commit -m "Verify Green tone and title unlock protocol fields"
```
