# Testing Patterns

**Analysis Date:** 2026-06-09

## Test Framework

**Runner:**
- xUnit via `Tests/Tests.csproj`.
- Microsoft.NET.Test.Sdk via `Directory.Packages.props`.
- Config: `Tests/Tests.csproj` plus shared build settings in `Directory.Build.props`.

**Run Commands:**
```powershell
dotnet test Tests/Tests.csproj
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Green"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Blue"
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~Yellow"
dotnet build TaikoLocalServer.slnx
dotnet build Host/Host.csproj -o "$env:TEMP\TaikoLocalServer-host-build"
```

## Test Value Gate

Every new test must answer: what evidence-backed behavior or state risk does this protect?

Allowed high-value test reasons:
- A real cabinet/RPCS3/log/proto/IDA observation established behavior and the test prevents regression.
- The server could silently corrupt or cross-write state across eras or modes.
- The repo owns nontrivial byte packing, bitset packing, parser behavior, catalog filtering, or persistence/readback logic.
- AdminApi/WebUI behavior would break an operator-visible workflow.
- Build or publish output affects deployed runtime files.

Prohibited low-value test reasons:
- Proving generated protobuf classes, generated fields, serializer tooling, route attributes, controller lists, DI registration shape, enum numeric values, private methods, migration bodies, source text, project files, or static config key existence.
- Proving a stateless compatibility controller returns `Result = 1` without also proving a meaningful no-mutation or logging contract.
- Copy/echo mapper tests where every assertion is a direct one-to-one field assignment and no classification, omission, packing, or evidence-backed placement is at risk.
- Adding tests only because a workflow says "TDD"; no blind test writing.

## Organization

Tests live in a single project under `Tests/Tests.csproj`.

Use folders by product area or era:
- `Tests/Ac15/`: shared AC15 helpers, parsers, protocol bytes, catalog readback services.
- `Tests/AllnetMucha/`: Mucha behavior that affects real client responses.
- `Tests/Blue/`: Blue handler, persistence-boundary, catalog, parser, mapper-classification, and AdminApi behavior.
- `Tests/Green/`: Green handler, persistence-boundary, catalog, parser, protocol-byte, and AdminApi behavior.
- `Tests/Yellow/`: Yellow handler, persistence-boundary, catalog, parser, mapper-classification, and AdminApi behavior.
- `Tests/WebUi/`: WebUI service behavior.

## Patterns

- Use `[Fact]` for single scenarios and `[Theory]` with `[InlineData]` for compact value matrices.
- Prefer Arrange/Act/Assert separated by whitespace.
- Prefer direct handler/service construction when it exercises meaningful behavior without hosting the full app.
- Use controller construction only when the controller response itself drives a real AdminApi/WebUI or external protocol behavior.
- Do not add source-guard, route-inventory, generated-protobuf reflection, private-method reflection, or static-config-existence tests.

## Fixtures

- Use SQLite in-memory through `TaikoDbContext` fixtures for persistence behavior.
- Do not mock EF Core for persistence tests.
- Use in-memory era catalog fixtures for handler tests unless the parser/loader itself is under test.
- Use temp files or checked-in fixture/runtime data only for parser, loader, or build-output behavior.
- Repo-root discovery is acceptable only for runtime data/catalog fixtures or build/publish output verification. Do not use it for source-text assertions.

## What To Test

Useful tests exercise:
- Handler/service state changes and no-write boundaries.
- SQLite persistence and readback when state is consumed later by the cabinet or AdminApi.
- Catalog parser behavior, malformed-file handling, and filtering rules.
- Byte/bit packing owned by this repo.
- Protocol payload classification backed by real captures/proto evidence.
- API responses that drive WebUI behavior or operator workflows.
- Build/publish output when deployed files are affected.

Mapper tests are narrow. They are allowed only when protecting classification, omission, packing, or evidence-backed field placement. Do not add simple copy/echo mapper tests.

## What Not To Test

Do not write tests that prove:
- Protobuf tooling generated a type or property.
- A route list contains exactly the current controller attributes.
- A controller method calls or does not call a specific implementation helper.
- DI registration returns a specific concrete type.
- Enum numeric values or static JSON keys exist.
- Source files contain or do not contain strings.
- EF migrations contain a specific SQL string.
- Private helper methods behave when they are only reachable through reflection.

These checks are brittle and do not prove game compatibility. If the client rejects a response or crashes, the automated tests were not the compatibility proof; real client verification is.

## Verification

For feature work, run the smallest meaningful focused test first, then the broader affected area. Before claiming completion, run the full relevant command fresh and report actual results.

Runtime smoke remains separate from unit/integration tests. For game-facing behavior, record cabinet/RPCS3/client acceptance evidence when the phase requires compatibility proof.
