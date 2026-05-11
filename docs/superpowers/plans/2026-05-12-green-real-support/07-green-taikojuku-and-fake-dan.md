# 07 - Green Taikojuku And Fake Dan

**Surface:** Return Taikojuku pack data from `musicmedleyinfo.xml` when possible and use deterministic fake packs when cabinet requests do not match medley IDs. First fake Dan grant is already implemented by baid login; this task completes readback endpoints around it.

**Why after 02/03/04:** Needs parsed medley data, fake Dan helper, and real identity flow.

**Files:**
- Modify: `Application/Handlers/GetTaikojukuQuery.Green.cs`
- Modify: `Adapters.GameProtocol.Green/Controllers/TaikojukuController.cs`
- Modify: `Adapters.GameProtocol.Green/Mappers/TaikojukuMappers.cs`
- Modify: `Application/Dtos/CommonTaikojukuResponse.cs`
- Modify: `Application/Handlers/GetInitialDataQuery.Green.cs`
- Create: `Tests/Green/GreenTaikojukuTests.cs`

---

## Task 07.1: Implement Taikojuku Response DTO And Handler

**Acceptance Criteria:**
- [ ] Requested `get_dan` IDs first match medley `UniqueId`.
- [ ] If no unique IDs match, requested IDs match `ChallengeLevel`.
- [ ] If neither matches, handler returns deterministic fake packs from first parsed songs.

**Steps:**

- [ ] **Step 1: Add tests**

Create `Tests/Green/GreenTaikojukuTests.cs`:

```csharp
using Microsoft.Extensions.Logging.Abstractions;
using TaikoLocalServer.Application.Handlers;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenTaikojukuTests
{
    [Fact]
    public async Task GetTaikojuku_ReturnsRequestedUniqueIdPack()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var handler = new GetTaikojukuQueryHandler(
            fixture.Catalog,
            NullLogger<GetTaikojukuQueryHandler>.Instance);

        var response = await handler.Handle(new GetTaikojukuQuery([20001]), CancellationToken.None);

        Assert.Equal((uint)1, response.Result);
        Assert.NotEmpty(response.Packs);
    }

    [Fact]
    public async Task GetTaikojuku_ReturnsDeterministicFallbackWhenNoPackMatches()
    {
        await using var fixture = await GreenHandlerFixture.CreateAsync();
        var handler = new GetTaikojukuQueryHandler(
            fixture.Catalog,
            NullLogger<GetTaikojukuQueryHandler>.Instance);

        var response = await handler.Handle(new GetTaikojukuQuery([999999]), CancellationToken.None);

        Assert.Equal((uint)1, response.Result);
        Assert.NotEmpty(response.Packs);
        Assert.NotEmpty(response.Packs[0].Songs);
    }
}
```

Ensure the shared test fixture has at least one `GreenTaikojukuEntry` with `UniqueId = 20001`, `ChallengeLevel = 1`, and three songs.

- [ ] **Step 2: Run focused test and confirm failure**

Run: `dotnet test --filter GreenTaikojukuTests`

Expected: FAIL because handler is still a stub or response DTO lacks `Packs`.

- [ ] **Step 3: Update `CommonTaikojukuResponse.cs`**

```csharp
namespace TaikoLocalServer.Application.Dtos;

public sealed class CommonTaikojukuResponse
{
    public uint Result { get; set; }
    public List<Pack> Packs { get; set; } = [];

    public sealed class Pack
    {
        public uint GetDan { get; set; }
        public uint VerupNo { get; set; }
        public List<Song> Songs { get; set; } = [];
    }

    public sealed class Song
    {
        public uint SongNo { get; set; }
        public uint Level { get; set; }
    }
}
```

- [ ] **Step 4: Implement `GetTaikojukuQuery.Green.cs`**

Update `GetTaikojukuQuery.cs` so the primary constructor injects `IGameDataCatalog`:

```csharp
public partial class GetTaikojukuQueryHandler(
    IGameDataCatalog gameDataService,
    ILogger<GetTaikojukuQueryHandler> logger)
    : IRequestHandler<GetTaikojukuQuery, CommonTaikojukuResponse>
```

```csharp
namespace TaikoLocalServer.Application.Handlers;

public partial class GetTaikojukuQueryHandler
{
    public partial ValueTask<CommonTaikojukuResponse> Handle(
        GetTaikojukuQuery request,
        CancellationToken cancellationToken)
    {
        var green = gameDataService.Green();
        var requested = request.RequestedDans.ToHashSet();

        var packs = green.TaikojukuFileOrder
            .Where(pack => requested.Count == 0 || requested.Contains(pack.UniqueId))
            .ToList();

        if (packs.Count == 0)
        {
            packs = green.TaikojukuFileOrder
                .Where(pack => requested.Contains(pack.ChallengeLevel))
                .ToList();
        }

        if (packs.Count == 0)
        {
            packs = CreateFallbackPacks(green);
        }

        return ValueTask.FromResult(new CommonTaikojukuResponse
        {
            Result = 1,
            Packs = packs.Select(ToCommonPack).ToList()
        });
    }

    private static List<GreenTaikojukuEntry> CreateFallbackPacks(IGreenCatalog green)
    {
        var songs = green.MusicInfoFileOrder.Take(6).ToArray();
        if (songs.Length == 0)
        {
            return [];
        }

        return
        [
            new GreenTaikojukuEntry
            {
                UniqueId = 1,
                ChallengeLevel = 1,
                Songs = songs.Take(3).Select(song => new GreenTaikojukuSong
                {
                    SongNo = song.SongNo,
                    Level = 0,
                    MusicId = song.MusicId
                }).ToArray()
            },
            new GreenTaikojukuEntry
            {
                UniqueId = 2,
                ChallengeLevel = 2,
                Songs = songs.Skip(3).Take(3).DefaultIfEmpty(songs[0]).Select(song => new GreenTaikojukuSong
                {
                    SongNo = song.SongNo,
                    Level = 1,
                    MusicId = song.MusicId
                }).ToArray()
            }
        ];
    }

    private static CommonTaikojukuResponse.Pack ToCommonPack(GreenTaikojukuEntry entry)
    {
        return new CommonTaikojukuResponse.Pack
        {
            GetDan = entry.UniqueId != 0 ? entry.UniqueId : entry.ChallengeLevel,
            VerupNo = entry.VerupNo,
            Songs = entry.Songs.Select(song => new CommonTaikojukuResponse.Song
            {
                SongNo = song.SongNo,
                Level = song.Level
            }).ToList()
        };
    }
}
```

- [ ] **Step 5: Run focused tests**

Run: `dotnet test --filter GreenTaikojukuTests`

Expected: PASS.

---

## Task 07.2: Wire Taikojuku Controller And Mapper

**Acceptance Criteria:**
- [ ] `taikojuku.php` returns `JukupackData` rows with song rows.

**Steps:**

- [ ] **Step 1: Update `TaikojukuMappers.cs`**

```csharp
namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

public static class TaikojukuMappers
{
    public static TaikojukuResponse Map(CommonTaikojukuResponse common)
    {
        var response = new TaikojukuResponse { Result = common.Result };

        foreach (var pack in common.Packs)
        {
            var wirePack = new TaikojukuResponse.JukupackData
            {
                GetDan = pack.GetDan,
                VerupNo = pack.VerupNo
            };

            foreach (var song in pack.Songs)
            {
                wirePack.AryJukusongDatas.Add(new TaikojukuResponse.JukupackData.JukusongData
                {
                    SongNo = song.SongNo,
                    Level = song.Level
                });
            }

            response.AryJukupackDatas.Add(wirePack);
        }

        return response;
    }
}
```

The generated collection names in `Wire/Game.cs` are `AryJukupackDatas` and `AryJukusongDatas`.

- [ ] **Step 2: Update `TaikojukuController.cs`**

```csharp
[HttpPost]
[Produces("application/protobuf")]
public async Task<IActionResult> Taikojuku([FromBody] TaikojukuRequest request)
{
    Logger.LogInformation("Green Taikojuku request: {Request}", request.Stringify());
    var common = await Mediator.Send(
        new GetTaikojukuQuery(request.GetDans),
        HttpContext.RequestAborted);
    return Ok(TaikojukuMappers.Map(common));
}
```

- [ ] **Step 3: Ensure InitialData advertises pack IDs**

In `GetInitialDataQuery.Green.cs`, ensure `AryGreenTaikojukuDatas` uses the same `GetDan` IDs that `TaikojukuMappers` will accept:

```csharp
AryGreenTaikojukuDatas = green.TaikojukuFileOrder.Take(3)
    .Select(entry => new CommonInitialDataCheckResponse.InformationData
    {
        InfoId = entry.UniqueId != 0 ? entry.UniqueId : entry.ChallengeLevel,
        VerupNo = entry.VerupNo
    })
    .ToList()
```

- [ ] **Step 4: Run tests and build**

Run:

```bash
dotnet test --filter GreenTaikojukuTests
dotnet build
```

Expected: both PASS.

- [ ] **Step 5: Commit**

```bash
git add Application/Handlers Application/Dtos Adapters.GameProtocol.Green Tests/Green/GreenTaikojukuTests.cs
git commit -m "feat(green): return taikojuku packs and fake dan data"
```
