# Blue A1 Era Foundation And Adapter Skeleton Spec

**Date:** 2026-05-27
**Status:** approved design; ready for implementation plan after written-spec review
**Scope:** Add Blue as a first-class, enableable game-protocol era with route
ownership and success-shaped controller stubs only.

## Purpose

Stage A1 creates the Blue protocol surface that later Blue stages can fill in.
It should prove that the server can discover Blue routes when Blue is enabled,
hide those routes when Blue is disabled, deserialize direct protobuf requests
without a `Content-Type` header, and return minimal protobuf responses. It must
not implement Blue gameplay, persistence, catalog loading, profile state, shop
state, AdminApi, or WebUI behavior.

A1 follows the A0 evidence spec:

- Blue game endpoints live under `/v10r03/chassis`.
- Startup and version endpoints live under shared `/v01r00/chassis` routes.
- Blue game requests are direct protobuf request bodies unless later cabinet
  traces prove otherwise.
- Blue needs the same scoped missing-content-type workaround as Green.
- `getreitai.php` and `getbanacoininfo.php` are not cabinet-called routes based
  on current evidence.

## Decisions

| Topic | Decision |
|---|---|
| Era identity | Add `GameEra.Blue` as a new stable enum value after Green. |
| Settings | Add `ServerSettings:Eras:Blue` with independent `Enabled` control. |
| Adapter | Create `Adapters.GameProtocol.Blue` as a sibling of Green. |
| Wire types | Generate Blue game wire types from `proto/blue/taiko.proto` into `TaikoLocalServer.Adapters.GameProtocol.Blue.Wire`. |
| Startup/version | Reuse `Adapters.GameProtocol.Shared` `/v01r00/chassis` controllers and shared wire shape. Do not add Blue-specific startup controllers. |
| Game routes | Add Blue stubs for A0-confirmed `/v10r03/chassis/*.php` routes only. |
| Runtime behavior | Stub controllers log request context and return success-shaped protobuf responses without `Mediator.Send`. |
| Battle route | `battleuserdata.php` can exist as an observable stub because A0 confirmed the path; Track B owns battle semantics. |
| Banacoin routes | Confirmed Banacoin/payment route paths can exist as observable stubs, but balance/payment behavior is not implemented. |
| Excluded proto-only messages | Do not scaffold `getreitai.php` or `getbanacoininfo.php` until logs or additional IDA evidence prove cabinet calls. |

## Architecture

### Domain And Settings

`Domain/Enums/GameEra.cs` gains `Blue`. The numeric value should be additive and
stable:

```csharp
public enum GameEra
{
    Nijiiro = 0,
    Green = 1,
    Blue = 2
}
```

`Host/Configurations/ServerSettings.json` gains a `Blue` era entry. The default
checked-in value should be disabled unless the local operator intentionally
enables it. Local developer settings may enable Blue, but the implementation
plan must avoid bundling unrelated local `ServerSettings.json` changes.

`Program.cs` parses enabled eras through the existing `ServerSettings:Eras`
dictionary. Any fatal startup text that names supported eras should include
Blue once the enum exists.

### Host Registration

`Host` references the new Blue adapter project. When Blue is enabled, Host calls
`AddGameProtocolBlue()`. When Blue is disabled, `Program.cs` removes the Blue
adapter assembly from MVC application parts, matching the existing Green/Nijiiro
pattern.

The missing-content-type middleware remains scoped. `ShouldAssumeProtobufRequest`
adds `/v10r03/chassis` and does not become a global fallback for every POST.
`/v01r00/chassis` remains shared for startup/version requests.

### Blue Adapter Project

Create this project layout:

```text
Adapters.GameProtocol.Blue/
  Adapters.GameProtocol.Blue.csproj
  BlueAdapterMarker.cs
  DependencyInjection.cs
  GlobalUsings.cs
  Wire/
    Game.cs
  Controllers/
    BaidController.cs
    BalanceCheckController.cs
    BanacoinErrorLogController.cs
    BanacoinPaymentController.cs
    BattleUserDataController.cs
    BookkeepingController.cs
    ChallengeCompeController.cs
    CoinSettingController.cs
    CrownsDataController.cs
    GetFolderController.cs
    GetItemShopInfoController.cs
    GetTelopController.cs
    HeadClerk2Controller.cs
    HeartbeatController.cs
    InitialDataCheckController.cs
    ItemPurchaseController.cs
    MyDonEntryController.cs
    PlayResultController.cs
    RecommendController.cs
    RewardCardCheckController.cs
    RewardExecutionController.cs
    SelfBestController.cs
    TaikojukuController.cs
    TournamentCheckController.cs
    UserDataController.cs
```

`Adapters.GameProtocol.Blue.csproj` should mirror Green's package and project
references unless a dependency is unused. It should reference
`Adapters.GameProtocol.Shared`, `Application`, and any contracts package needed
by existing adapter conventions. Controllers inherit
`BaseProtocolController<TController>`.

`DependencyInjection.cs` should be intentionally small:

```csharp
public static class DependencyInjection
{
    public const GameEra Era = GameEra.Blue;

    public static IServiceCollection AddGameProtocolBlue(this IServiceCollection services)
    {
        return services;
    }
}
```

The adapter should not reference Green. Shared behavior belongs in
`Adapters.GameProtocol.Shared` only when it is truly shared and needed now.

### Generated Wire Types

Generate `Adapters.GameProtocol.Blue/Wire/Game.cs` from
`proto/blue/taiko.proto` using the repo's existing protobuf-net `protogen`
flow. The namespace must be:

```csharp
TaikoLocalServer.Adapters.GameProtocol.Blue.Wire
```

Do not manually edit generated message bodies except for namespace or
formatting adjustments required by the existing local generation workflow.

Do not generate or use a Blue `VsInterface.cs` in A1. A0 proved
`proto/blue/vsinterface.proto` is byte-for-byte identical to Green's
`vsinterface.proto`, and startup/version endpoints already belong to shared
`/v01r00/chassis` controllers.

### Route Ownership

A1 owns these `/v10r03/chassis` game routes:

| Route | Request | Response | A1 behavior |
|---|---|---|---|
| `initialdatacheck.php` | `InitialdatacheckRequest` | `InitialdatacheckResponse` | success-shaped empty/default response |
| `tournamentcheck.php` | `TournamentcheckRequest` | `TournamentcheckResponse` | success-shaped empty/default response |
| `bookkeeping.php` | `BookKeepingRequest` | `BookKeepingResponse` | `Result = 1` |
| `coinsetting.php` | `CoinsettingRequest` | `CoinsettingResponse` | `Result = 1` |
| `gettelop.php` | `GettelopRequest` | `GettelopResponse` | success-shaped empty/default response |
| `getfolder.php` | `GetfolderRequest` | `GetfolderResponse` | success-shaped empty/default response |
| `taikojuku.php` | `TaikojukuRequest` | `TaikojukuResponse` | success-shaped empty/default response |
| `getitemshopinfo.php` | `GetitemshopinfoRequest` | `GetitemshopinfoResponse` | success-shaped empty/default response |
| `headclerk2.php` | `HeadClerk2Request` | `HeadClerk2Response` | `Result = 1`; no CSV/gameplay semantics |
| `playresult.php` | `PlayResultRequest` | `PlayResultResponse` | `Result = 1`; no persistence |
| `banacoinerrorlog.php` | `BanacoinerrorlogRequest` | `BanacoinerrorlogResponse` | log and return success-shaped response |
| `baidcheck.php` | `BAIDRequest` | `BAIDResponse` | success-shaped no-persistence response only |
| `mydonentry.php` | `MydonEntryRequest` | `MydonEntryResponse` | success-shaped no-persistence response only |
| `userdata.php` | `UserDataRequest` | `UserDataResponse` | success-shaped empty/default response |
| `challengecompe.php` | `ChallengeCompeRequest` | `ChallengeCompeResponse` | success-shaped empty/default response |
| `balancecheck.php` | `BalancecheckRequest` | `BalancecheckResponse` | log and return success-shaped response; no balance state |
| `banacoinpayment.php` | `BanacoinpaymentRequest` | `BanacoinpaymentResponse` | log and return success-shaped response; no payment state |
| `crownsdata.php` | `CrownsDataRequest` | `CrownsDataResponse` | success-shaped empty/default response |
| `recommend.php` | `RecommendRequest` | `RecommendResponse` | success-shaped empty/default response |
| `selfbest.php` | `SelfBestRequest` | `SelfBestResponse` | success-shaped empty/default response |
| `heartbeat.php` | `HeartBeatRequest` | `HeartBeatResponse` | `Result = 1` and server status fields healthy |
| `itempurchase.php` | `ItempurchaseRequest` | `ItempurchaseResponse` | success-shaped response; no unlock state |
| `battleuserdata.php` | `BattleUserDataRequest` | `BattleUserDataResponse` | observable stub only; Track B owns semantics |
| `rewardcardcheck.php` | `RewardcardcheckRequest` | `RewardcardcheckResponse` | success-shaped empty/default response |
| `rewardexecution.php` | `RewardexecutionRequest` | `RewardexecutionResponse` | success-shaped response; no unlock state |

A1 does not add:

- `/v10r03/chassis/getreitai.php`
- `/v10r03/chassis/getbanacoininfo.php`

Those message types exist in the proto, but A0 found no route string evidence.

### Stub Response Rules

Every stub controller should:

- use `[ApiController]`, `[HttpPost]`, and `[Produces("application/protobuf")]`;
- accept the generated Blue request type through `[FromBody]`;
- log the endpoint and high-signal identifiers such as chassis id, shop id,
  BAID, access code, or requested ids when present;
- return the generated Blue response type;
- avoid `Mediator.Send`, database access, catalog access, and Green adapter
  calls.

Success-shaped means enough fields to serialize a response without server
errors. Every generated required response member must be populated. Where the
response has a required `result`, set it to `1`. For required string fields,
echo a matching request value when the response has the same concept, such as
`personid` on Banacoin stubs; otherwise use an explicit empty string as a stub
value and log that the endpoint is not implemented. Where A0 did not prove a
safe non-empty payload shape, return empty repeated fields and default optional
fields rather than inventing data.

### Out Of Scope

- Blue catalog or data loader design; that is A2.
- Blue card registration, identity persistence, default save state, or profile
  readback; that is A3.
- Blue play-result persistence, crowns, self-best, rewards, or unlock state;
  that is A4.
- Blue Dani behavior; that is A5.
- Blue item shop state and season data; that is A6.
- Blue AdminApi or WebUI surfaces; that is A7.
- Cabinet smoke hardening; that is A8.
- Blue battle semantics; that is Track B.
- Tokkun behavior.
- Banacoin balance/payment behavior.
- Refactoring Green into shared abstractions before Blue behavior is proven.

## Testing And Verification

The implementation plan should include:

- generated wire sanity checks that `Wire/Game.cs` contains the Blue-only
  messages `Coinsetting*`, `Balancecheck*`, `Banacoinpayment*`,
  `Banacoinerrorlog*`, and `BattleUserData*`;
- a build of `Adapters.GameProtocol.Blue`;
- a build of `Host/Host.csproj`;
- route discovery checks proving Blue routes are present when Blue is enabled;
- route absence checks proving Blue routes are removed when Blue is disabled,
  preferably through the same application-part pattern used for Green;
- a middleware check that a POST to `/v10r03/chassis/...` without
  `Content-Type` is treated as protobuf, while unrelated POSTs are not;
- a check that no A1 stub controller calls `Mediator.Send`;
- a check that no `getreitai.php` or `getbanacoininfo.php` Blue route exists.

If `Host/bin` is locked during verification, use a temporary output directory
for the Host build rather than treating the lock as a source failure.

## Acceptance Criteria

- `GameEra.Blue` exists and can be parsed from `ServerSettings:Eras`.
- Blue can be enabled or disabled independently of Green and Nijiiro.
- `Adapters.GameProtocol.Blue` is part of the solution and referenced by Host.
- Blue generated game wire types compile under a Blue namespace.
- Shared `/v01r00/chassis` startup/version route ownership remains unchanged.
- `/v10r03/chassis` game routes are routed only when Blue is enabled.
- `/v10r03/chassis` missing-content-type protobuf posts reach controllers.
- Blue route stubs return protobuf success-shaped responses without
  persistence, catalog, or Mediator behavior.
- Proto-only `getreitai.php` and `getbanacoininfo.php` routes remain absent.

## Handoff To A2 And Later

A1 intentionally leaves every response semantically shallow. A2 should start
from the Blue data layout confirmed in A0 and design `IBlueCatalog` plus source
file expectations. A3 and later stages should replace individual stubs with
real handler calls only after their own specs decide the data and persistence
contracts.
