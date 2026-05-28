# Testing Patterns

**Analysis Date:** 2026-05-28

## Test Framework

**Runner:**
- xUnit `2.9.3` via `Tests/Tests.csproj`.
- Microsoft.NET.Test.Sdk `17.14.1` via `Directory.Packages.props`.
- Config: `Tests/Tests.csproj` plus shared build settings in `Directory.Build.props`.

**Assertion Library:**
- xUnit assertions from `Xunit` global using in `Tests/GlobalUsings.cs`.
- No FluentAssertions, Moq, NSubstitute, or FakeItEasy packages are configured in `Directory.Packages.props` or `Tests/Tests.csproj`.

**Run Commands:**
```bash
dotnet test Tests/Tests.csproj              # Run all tests in the test project
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Green"  # Run focused Green-era tests
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Blue"   # Run focused Blue-era tests
dotnet build TaikoLocalServer.slnx          # Compile all projects
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"  # Host build using temp output
```

## Test File Organization

**Location:**
- Tests are in a single test project: `Tests/Tests.csproj`.
- Test files are grouped by product area or era under `Tests/Ac15/`, `Tests/AllnetMucha/`, `Tests/Blue/`, `Tests/Green/`, and `Tests/WebUi/`.
- Shared test imports live in `Tests/GlobalUsings.cs`.

**Naming:**
- Test classes end in `Tests`: `Tests/Green/GreenPlayResultHandlerTests.cs`, `Tests/Blue/BlueAdminApiDaniTests.cs`, `Tests/WebUi/GameDataServiceTests.cs`.
- Fixture classes end in `Fixture`: `Tests/Green/GreenHandlerFixture.cs`, `Tests/Blue/BlueHandlerFixture.cs`.
- Runtime catalog collections use `*TestCollection`: `Tests/Green/GreenRuntimeCatalogTestCollection.cs`.
- Test method names follow `Subject_Condition_ExpectedOutcome`: `UpdatePlayResult_Green_SavesPlayAndBest` in `Tests/Green/GreenPlayResultHandlerTests.cs`, `DanBestData_Blue_MapsClearGradeSubset` in `Tests/Blue/BlueAdminApiDaniTests.cs`.

**Structure:**
```text
Tests/
├── Ac15/          # Shared AC15 catalog-loader tests
├── AllnetMucha/   # Allnet/Mucha controller tests
├── Blue/          # Blue era handler, mapper, route, source-guard, and Admin API tests
├── Green/         # Green era handler, mapper, route, catalog, protocol, and Admin API tests
├── WebUi/         # Blazor/WebUI service and source-level tests
└── GlobalUsings.cs
```

## Test Structure

**Suite Organization:**
```csharp
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
}
```

**Patterns:**
- Use `[Fact]` for single scenario tests and `[Theory]` with `[InlineData]` for compact value matrices: `Tests/Green/GreenPlayResultHandlerTests.cs`, `Tests/Green/GreenStageModeInterpreterTests.cs`, `Tests/Blue/BlueDanHelperTests.cs`.
- Use Arrange/Act/Assert separated by whitespace rather than comments: `Tests/Green/GreenPlayResultHandlerTests.cs`, `Tests/Blue/BlueAdminApiDaniTests.cs`.
- Prefer direct handler/controller construction over a full web host for most tests: `Tests/Green/GreenAdminApiControllerTests.cs`, `Tests/Blue/BlueAdminApiDaniTests.cs`, `Tests/AllnetMucha/MuchaControllerTests.cs`.
- Use source-guard tests for route/wiring invariants that are hard to assert through runtime registration: `Tests/Blue/BlueHostProgramSourceTests.cs`, `Tests/Blue/BlueA5SourceGuardTests.cs`, `Tests/WebUi/GreenCustomizationWebUiTests.cs`.
- Use focused binary/protobuf round-trip tests for wire compatibility: `Tests/Green/StartupAuthRouteTests.cs`, `Tests/Green/GreenPlayResultPayloadDecoderTests.cs`, `Tests/Blue/BlueWireGenerationTests.cs`.

## Mocking

**Framework:** Hand-written fakes and test fixtures. No mocking library is installed.

**Patterns:**
```csharp
private sealed class RecordingHandler : HttpMessageHandler
{
    public List<string> RequestPaths { get; } = [];

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        RequestPaths.Add(request.RequestUri?.PathAndQuery.TrimStart('/') ?? string.Empty);
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]")
        });
    }
}
```

**What to Mock:**
- Mock HTTP with custom `HttpMessageHandler` implementations for WebUI service tests: `Tests/WebUi/GameDataServiceTests.cs`.
- Mock catalogs with in-memory `IGreenCatalog` and `IBlueCatalog` implementations nested in fixtures: `Tests/Green/GreenHandlerFixture.cs`, `Tests/Blue/BlueHandlerFixture.cs`.
- Mock logging with `NullLogger<T>.Instance` unless log output is the behavior under test: `Tests/Green/GreenPlayResultHandlerTests.cs`, `Tests/Green/GreenItemShopPurchaseTests.cs`.
- Mock request services with `ServiceCollection` and `DefaultHttpContext` when controller base classes need DI: `Tests/Blue/BlueAdminApiDaniTests.cs`, `Tests/Green/StartupAuthRouteTests.cs`.

**What NOT to Mock:**
- Do not mock EF Core for persistence behavior. Use SQLite in-memory through `TaikoDbContext` fixtures in `Tests/Green/GreenHandlerFixture.cs` and `Tests/Blue/BlueHandlerFixture.cs`.
- Do not mock protocol serializers when testing wire compatibility. Serialize/deserialize real protobuf DTOs in `Tests/Green/StartupAuthRouteTests.cs` and decoder tests in `Tests/Green/GreenPlayResultPayloadDecoderTests.cs`.
- Do not mock catalog parsers when loader behavior is under test. Use temp files or checked-in fixture data in `Tests/Ac15/Ac15CatalogLoaderTests.cs`, `Tests/Green/GreenCatalogLoaderTests.cs`, and `Tests/Blue/BlueCatalogLoaderTests.cs`.

## Fixtures and Factories

**Test Data:**
```csharp
await using var fixture = await GreenHandlerFixture.CreateAsync();
fixture.Context.UserData.Add(new UserDatum { Baid = 1, MyDonName = "DON" });
fixture.Context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(1));
await fixture.Context.SaveChangesAsync();
```

**Location:**
- Green handler fixture: `Tests/Green/GreenHandlerFixture.cs`.
- Blue handler fixture: `Tests/Blue/BlueHandlerFixture.cs`.
- Catalog fixture data is nested inside fixture classes: `GreenHandlerFixture.TestGreenCatalog` in `Tests/Green/GreenHandlerFixture.cs`, `BlueHandlerFixture.TestBlueCatalog` in `Tests/Blue/BlueHandlerFixture.cs`.
- Temp-file fixture generation lives inside the test that needs it: `CreateTuningBin(...)` in `Tests/Green/GreenCatalogLoaderTests.cs`, JSON heredocs in `Tests/Ac15/Ac15OptionalCatalogLoaderTests.cs`.
- Repo-root helpers are local private methods in source-level and runtime catalog tests: `FindRepoRoot()` in `Tests/Green/GreenCatalogLoaderTests.cs`, `FindRepoFile(...)` in `Tests/Blue/BlueHostProgramSourceTests.cs`.

## Coverage

**Requirements:** None enforced. No `coverlet`, `.runsettings`, coverage threshold, or CI coverage step is detected in `Tests/Tests.csproj`, `Directory.Packages.props`, or `.github/workflows/publishTLS.yml`.

**View Coverage:**
```bash
dotnet test Tests/Tests.csproj --collect "XPlat Code Coverage"  # Not configured by repo packages; install/configure collector first if needed
```

## Test Types

**Unit Tests:**
- Shared utility tests cover pure transformations and encodings: `Tests/Green/GreenProtocolBytesTests.cs`, `Tests/Green/BitsetCodecTests.cs`, `Tests/Blue/BlueDanHelperTests.cs`.
- Mapper tests verify common DTO/protocol conversions without persistence: `Tests/Green/GreenPlayResultMapperTests.cs`, `Tests/Blue/BluePlayResultMapperTests.cs`, `Tests/Green/GreenSelfBestMapperTests.cs`.
- Loader tests verify parsing, filtering, missing-file behavior, and malformed-file exceptions: `Tests/Ac15/Ac15CatalogLoaderTests.cs`, `Tests/Ac15/Ac15OptionalCatalogLoaderTests.cs`, `Tests/Green/GreenMovieLoaderTests.cs`.

**Integration Tests:**
- Handler persistence tests use SQLite in-memory and real `TaikoDbContext`: `Tests/Green/GreenPlayResultHandlerTests.cs`, `Tests/Green/GreenAiBattlePlayResultTests.cs`, `Tests/Blue/BluePlayResultHandlerTests.cs`.
- Admin API controller tests instantiate controllers with real context/catalog services and assert `IActionResult` shapes: `Tests/Green/GreenAdminApiControllerTests.cs`, `Tests/Blue/BlueAdminApiDaniTests.cs`.
- Startup/route tests inspect controller attributes and real service registrations without running a full server: `Tests/Green/StartupAuthRouteTests.cs`, `Tests/Blue/BlueRouteSkeletonTests.cs`.

**E2E Tests:**
- Not used. There is no Playwright, Selenium, WebApplicationFactory, or browser-driven test project detected in `Tests/Tests.csproj` or `Directory.Packages.props`.

## Common Patterns

**Async Testing:**
```csharp
[Fact]
public async Task UpdatePlayResult_Green_GuestBaidDoesNotSave()
{
    await using var fixture = await GreenHandlerFixture.CreateAsync();
    var handler = new UpdatePlayResultCommandHandler(
        fixture.Context,
        fixture.Catalog,
        NullLogger<UpdatePlayResultCommandHandler>.Instance);

    var result = await handler.Handle(command, CancellationToken.None);

    Assert.Equal(1u, result);
    Assert.Empty(await fixture.Context.SongPlayDataGreen.ToListAsync());
}
```

**Error Testing:**
```csharp
[Fact]
public void Decode_RejectsNegativeByteCount()
{
    Assert.Throws<ArgumentOutOfRangeException>(() => BitsetCodec.Decode([], byteCount: -1));
}
```

**File-Based Testing:**
- Use `Path.GetTempPath()`, `Guid.NewGuid().ToString("N")`, `try/finally`, and `File.Delete`/`Directory.Delete` for generated fixtures: `Tests/Ac15/Ac15OptionalCatalogLoaderTests.cs`, `Tests/Green/GreenCatalogLoaderTests.cs`, `Tests/Green/GreenCustomizationExtractorTests.cs`.
- Use repo-root discovery only when tests intentionally exercise checked-in runtime data or source text: `Tests/Green/GreenCatalogLoaderTests.cs`, `Tests/Blue/BlueDocsTests.cs`, `Tests/WebUi/DaniDojoTitleTests.cs`.

**Controller Testing:**
- Set `ControllerContext.HttpContext` when authorization, request abort tokens, or request services are required: `Tests/Blue/BlueAdminApiDaniTests.cs`, `Tests/Green/StartupAuthRouteTests.cs`.
- Assert concrete result types before asserting response payloads: `OkObjectResult`, `BadRequestObjectResult`, `StatusCodeResult` in `Tests/Green/GreenAdminApiControllerTests.cs`, `Tests/AllnetMucha/MuchaControllerTests.cs`.

**Source Guards:**
- Use source guards for high-value architecture boundaries, not as a substitute for behavior tests: `Tests/Blue/BlueHostProgramSourceTests.cs`, `Tests/Blue/BlueA3SourceGuardTests.cs`, `Tests/Blue/BlueA5SourceGuardTests.cs`.
- Keep source guard searches specific and assert both positive and negative strings when guarding route scopes: `Tests/Blue/BlueHostProgramSourceTests.cs`.

---

*Testing analysis: 2026-05-28*
