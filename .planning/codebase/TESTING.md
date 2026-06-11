# Testing Patterns

**Analysis Date:** 2026-06-11

## Test Framework

**Runner:**
- xUnit through `Tests/Tests.csproj`.
- Microsoft.NET.Test.Sdk and xUnit package versions are centralized in `Directory.Packages.props`.
- Config: `Tests/Tests.csproj`, `Directory.Build.props`, and `Directory.Packages.props`.

**Assertion Library:**
- xUnit assertions only; no FluentAssertions, Moq, NSubstitute, or snapshot test framework is detected.

**Run Commands:**
```powershell
dotnet test Tests/Tests.csproj
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Blue"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Green"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Yellow"
dotnet build TaikoLocalServer.slnx
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"
```

## Test File Organization

**Location:**
- Tests live in one project: `Tests/Tests.csproj`.
- Tests are foldered by shared subsystem, era, adapter, or UI surface.

**Naming:**
- Test files use `{Area}{Subject}Tests.cs`: `Tests/Ac15/Ac15NormalPlayWriterTests.cs`, `Tests/Blue/BlueBattlePlayResultHandlerTests.cs`, `Tests/Yellow/YellowUserDataProtocolTests.cs`.
- Fixture files use `{Era}HandlerFixture.cs`: `Tests/Blue/BlueHandlerFixture.cs`, `Tests/Green/GreenHandlerFixture.cs`, `Tests/Yellow/YellowHandlerFixture.cs`.
- Test methods use behavior names with underscores: `SaveAsync_WritesOnlyTheBoundEraTables`, `UserData_Yellow_UsesOnlyYellowPurchasedShopRowsForSongAndToneLocks`.

**Structure:**
```text
Tests/
|-- Ac15/          # Shared AC15 helpers, services, parsers, profile/capability rules
|-- AllnetMucha/   # Mucha protocol behavior
|-- Blue/          # Blue handlers, mappers, catalogs, persistence boundaries, battle/Tokkun
|-- Green/         # Green handlers, mappers, catalogs, ghost behavior, startup/version
|-- Yellow/        # Yellow handlers, mappers, catalogs, route/protocol shape, persistence boundaries
|-- Logging/       # Logging behavior
`-- WebUi/         # Blazor service behavior
```

## Test Value Gate

Every new test must protect a specific evidence-backed behavior or state risk. Do not add tests only because a workflow mentions TDD.

**High-value reasons:**
- A real cabinet/RPCS3/log/proto/IDA observation established the behavior.
- A handler or shared AC15 helper could silently cross-write state across eras or modes.
- The repo owns byte/bit packing, parser behavior, field-presence semantics, catalog filtering, or SQLite persistence/readback logic.
- AdminApi/WebUI behavior affects an operator workflow.
- Build/publish output affects deployed runtime files.

**Low-value reasons to reject:**
- Proving generated protobuf types, generated fields, route inventory, controller attribute lists, DI shape, enum numeric values, migration source text, project-file contents, or private methods.
- Proving a stateless compatibility endpoint returns only `Result = 1` unless the test also protects a meaningful no-persistence, logging, or serialized-shape contract.
- Copy/echo mapper tests where every assertion is a one-to-one assignment and no classification, omission, packing, or field placement is at risk.
- Source-string tests over `.cs`, `.csproj`, `Program.cs`, migrations, controller bodies, `Mediator.Send`, or `SaveChanges`.

## Test Structure

**Suite Organization:**
```csharp
public sealed class Ac15NormalPlayWriterTests
{
    [Fact]
    public async Task SaveAsync_WritesOnlyTheBoundEraTables()
    {
        await using var database = await SchemaDatabase.CreateAsync();
        database.Context.UserData.Add(new UserDatum { Baid = 1 });
        await database.Context.SaveChangesAsync();

        await Ac15NormalPlayWriter.SaveAsync(
            database.Context,
            BlueTables(database.Context),
            request,
            Ac15NormalStagePolicies.Standard,
            CancellationToken.None);

        Assert.Single(await database.Context.SongPlayDataBlue.ToListAsync());
        Assert.Empty(await database.Context.SongPlayDataGreen.ToListAsync());
    }
}
```

**Patterns:**
- Use `[Fact]` for single scenarios and `[Theory]` with `[InlineData]` for compact value matrices: `Tests/Ac15/Ac15DanHelpersTests.cs`, `Tests/Blue/BlueItemShopPurchaseTests.cs`, `Tests/Yellow/YellowPlayResultHandlerTests.cs`.
- Prefer Arrange/Act/Assert separated by whitespace, as in `Tests/Yellow/YellowPlayResultHandlerTests.cs` and `Tests/Ac15/Ac15ItemShopPurchaseTests.cs`.
- Prefer direct handler/service construction over full app hosting when the behavior is inside the handler/service: `Tests/Blue/BluePlayResultHandlerTests.cs`, `Tests/Ac15/Ac15UserDataServiceTests.cs`.
- Use controller construction only when the controller response is the observable contract: `Tests/AllnetMucha/MuchaControllerTests.cs`, `Tests/Yellow/YellowCompatibilityResponseShapeTests.cs`.
- Keep helper methods and mini fixtures private inside the test class unless reused by many era tests: `Tests/Ac15/Ac15DaniCapabilityTests.cs`, `Tests/Blue/BlueHandlerFixture.cs`.

## Mocking

**Framework:** Manual fakes/stubs only.

**Patterns:**
```csharp
await using var fixture = await YellowHandlerFixture.CreateAsync(catalog);
var handler = new UpdatePlayResultCommandHandler(
    fixture.Context,
    fixture.Catalog,
    NullLogger<UpdatePlayResultCommandHandler>.Instance);
```

**What to Mock:**
- Use in-memory test catalogs for handler tests: `Tests/Blue/BlueHandlerFixture.cs`, `Tests/Green/GreenHandlerFixture.cs`, `Tests/Yellow/YellowHandlerFixture.cs`.
- Use custom `HttpMessageHandler` for WebUI service route tests: `Tests/WebUi/GameDataServiceTests.cs`.
- Use local recording loggers only when log output is the assertion target: `Tests/Blue/BlueCatalogLoaderTests.cs`, `Tests/Blue/BluePlayResultHandlerTests.cs`.
- Use `NullLogger<T>.Instance` or `NullLogger.Instance` for normal handler/service tests.

**What NOT to Mock:**
- Do not mock EF Core for persistence behavior. Use SQLite in-memory through `TaikoDbContext`.
- Do not mock Mapperly-generated methods; test the behavioral surface that depends on them when classification, omission, or field placement matters.
- Do not mock generated protobuf classes; create representative wire DTOs when the adapter mapper or controller shape is the behavior under test.

## Fixtures and Factories

**Test Data:**
```csharp
private sealed class SchemaDatabase(SqliteConnection connection) : IAsyncDisposable
{
    public TaikoDbContext Context { get; } = CreateContext(connection);

    public static async Task<SchemaDatabase> CreateAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var database = new SchemaDatabase(connection);
        await database.Context.Database.EnsureCreatedAsync();
        return database;
    }
}
```

**Location:**
- Shared AC15 SQLite mini-fixtures live inside individual tests when not reused: `Tests/Ac15/Ac15DaniCapabilityTests.cs`, `Tests/Ac15/Ac15NormalPlayWriterTests.cs`.
- Era handler fixtures live in era folders and expose `Context` plus `IGameDataCatalog`: `Tests/Blue/BlueHandlerFixture.cs`, `Tests/Yellow/YellowHandlerFixture.cs`.
- Parser/loader tests write temp files and clean them in `finally`: `Tests/Ac15/Ac15CatalogLoaderTests.cs`, `Tests/Blue/BlueItemShopLoaderTests.cs`.
- Runtime catalog tests that require local operator data skip by returning early when files are absent and share a collection for process-root-sensitive work: `Tests/Green/GreenRuntimeCatalogTestCollection.cs`, `Tests/Blue/BlueCatalogLoaderTests.cs`.

## Coverage

**Requirements:** No numeric coverage target or coverage tool is configured in `Tests/Tests.csproj` or `Directory.Packages.props`.

**View Coverage:**
```powershell
# Not configured. Add coverage tooling only when a phase explicitly requires it.
dotnet test Tests/Tests.csproj
```

## Test Types

**Unit Tests:**
- Shared AC15 service/helper tests exercise canonical behavior without full route hosting: `Tests/Ac15/Ac15UserDataServiceTests.cs`, `Tests/Ac15/Ac15InitialDataServiceTests.cs`, `Tests/Ac15/Ac15DanHelpersTests.cs`.
- Parser/packing tests target repo-owned parsing or byte layout: `Tests/Ac15/Ac15CatalogLoaderTests.cs`, `Tests/Ac15/Ac15ProtocolBytesTests.cs`, `Tests/Green/BitsetCodecTests.cs`.

**Integration Tests:**
- Handler + SQLite tests prove persistence/readback and no-cross-era boundaries: `Tests/Yellow/YellowPlayResultHandlerTests.cs`, `Tests/Blue/BlueBattlePlayResultHandlerTests.cs`, `Tests/Ac15/Ac15NormalPlayWriterTests.cs`.
- Controller response tests prove protocol/API behavior when response shape or HTTP status is the contract: `Tests/AllnetMucha/MuchaControllerTests.cs`, `Tests/Yellow/YellowCompatibilityResponseShapeTests.cs`.
- AdminApi/WebUI tests prove era route behavior consumed by the UI: `Tests/Yellow/YellowAdminApiTests.cs`, `Tests/WebUi/GameDataServiceTests.cs`.

**E2E Tests:**
- No automated browser or cabinet E2E framework is detected.
- Game-facing automated tests are regression guards, not compatibility proof. Cabinet/RPCS3/client smoke remains the compatibility gate when required by a phase.

## AC15 Shared-Core Testing

**Shared Behavior:**
- Put value-identical Blue/Green/Yellow logic under `Tests/Ac15/`: Dan helper rules, profile/counter mutation, normal-play writer behavior, item-shop purchase behavior, catalog readback, initial-data composition, userdata composition.
- Test the shared helper through typed table/capability bindings, not through implementation-string assertions: `Tests/Ac15/Ac15DaniCapabilityTests.cs`, `Tests/Ac15/Ac15NormalPlayWriterTests.cs`.
- Prove shared helpers write only the bound era tables when generic EF helpers are used: `Tests/Ac15/Ac15DaniCapabilityTests.cs`, `Tests/Ac15/Ac15NormalPlayWriterTests.cs`.

**Era Behavior:**
- Keep era-owned behavior in era folders even when it uses shared AC15 services: `Tests/Blue/BlueBattlePlayResultHandlerTests.cs`, `Tests/Green/GreenAiBattlePlayResultTests.cs`, `Tests/Yellow/YellowTokkunPersistenceTests.cs`.
- For new Yellow/Blue/Green work, include no-cross-era assertions when the risk is persistence bleed: `Tests/Yellow/YellowPlayResultHandlerTests.cs`, `Tests/Yellow/YellowUserDataProtocolTests.cs`.
- Keep era-only features local: Blue battle tests in `Tests/Blue/`, Green ghost tests in `Tests/Green/`, Yellow absence/compatibility tests in `Tests/Yellow/`.

## Mapper Testing

**Allowed Mapper Tests:**
- Classification and mode routing: Tokkun/battle classification in `Tests/Blue/BluePlayResultMapperTests.cs`, Yellow Tokkun shape in `Tests/Yellow/YellowUserDataProtocolTests.cs`.
- Wire field placement and presence: `Tests/Yellow/YellowCompatibilityResponseShapeTests.cs`, `Tests/Green/GreenUserDataMapperTests.cs`.
- Omission/default semantics that affect cabinet parsing: `Tests/Yellow/YellowUserDataProtocolTests.cs`, `Tests/Blue/BlueBattlePlayResultMapperTests.cs`.

**Avoid Mapper Tests For:**
- One-to-one copies with no classifier, omission, packed-byte, or evidence-backed field-placement risk.
- Generated `Wire/` type/property existence.
- Mapperly source-generation behavior itself.

## Common Patterns

**Async Testing:**
```csharp
await using var fixture = await BlueHandlerFixture.CreateAsync();
fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
await fixture.Context.SaveChangesAsync();

var result = await handler.Handle(command, CancellationToken.None);

Assert.Equal(1u, result);
```

**Error Testing:**
```csharp
var ex = Assert.Throws<InvalidDataException>(() => LoadInvalidCatalog());
Assert.Contains("item shop", ex.Message, StringComparison.Ordinal);
```

**Persistence Boundary Testing:**
- Seed other era rows, execute the target era behavior, and assert the other era rows are unchanged or empty: `Tests/Yellow/YellowPlayResultHandlerTests.cs`, `Tests/Yellow/YellowUserDataProtocolTests.cs`.
- Assert no-write boundaries for guest/unknown users, Tokkun uploads, battle uploads, and stateless compatibility endpoints: `Tests/Yellow/YellowPlayResultHandlerTests.cs`, `Tests/Blue/BlueBattlePlayResultHandlerTests.cs`, `Tests/Yellow/YellowTokkunPersistenceTests.cs`.

**Protocol Field-Presence Testing:**
- Reflection over `ShouldSerialize*` is allowed only for serialized field-presence contracts, not for source-shape assertions: `Tests/Yellow/YellowCompatibilityResponseShapeTests.cs`.
- Prefer comparing serialized presence between known-compatible era responses when preserving Blue/Yellow response shape: `Tests/Yellow/YellowCompatibilityResponseShapeTests.cs`.

**Catalog/Runtime Data Testing:**
- For pure parser behavior, create minimal temp XML/JSON/bin fixtures in the test and delete them: `Tests/Ac15/Ac15CatalogLoaderTests.cs`.
- For local operator data smoke, locate files under `Host/wwwroot/data/<era>/data`, return early when absent, and avoid making those tests the only proof of a parser rule: `Tests/Blue/BlueCatalogLoaderTests.cs`.

## What To Test

- Observable handler/service state changes and readback consumed by the cabinet or AdminApi.
- SQLite persistence, no-cross-era persistence, and no-cross-mode persistence.
- Shared AC15 capability helpers when they centralize behavior previously duplicated across Blue/Green/Yellow.
- Protocol payload classification backed by real captures/proto evidence.
- Byte/bit packing and parser behavior owned by this repo.
- AdminApi/WebUI route behavior that affects operator workflows.
- Build/publish output when deployed runtime files are affected.

## What Not To Test

- Generated protobuf output, generated `Wire/` type/property existence, or serializer tooling internals.
- Route inventory, controller attribute lists, DI registration shape, enum numeric values, static config key presence, source text, project files, migrations, private methods, `Mediator.Send`, or `SaveChanges`.
- Simple stateless `Result = 1` echoes unless paired with a meaningful no-mutation/logging/shape contract.
- Copy/echo Mapperly mappings with no behavioral risk.
- Tests written only to satisfy TDD process language.

## Verification

- For focused changes, run the smallest meaningful filter first, for example `dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Ac15NormalPlayWriterTests"`.
- For AC15 shared-core changes, run both shared and affected era filters: `FullyQualifiedName~Ac15`, plus `Blue`, `Green`, or `Yellow` as applicable.
- Before declaring a phase done, run `dotnet test Tests/Tests.csproj` and the relevant build command fresh.
- For game-facing behavior, automated tests are regression guards. Record cabinet/RPCS3/client acceptance separately when compatibility is the phase gate.

---

*Testing analysis: 2026-06-11*
