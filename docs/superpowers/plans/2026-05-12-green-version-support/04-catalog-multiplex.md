# 04 — Catalog multiplex: `IGameDataCatalog.For(GameEra)`

**Surface:** Refactor `IGameDataCatalog` from a flat surface (`catalog.MusicInfos`, `catalog.DanData`, etc.) into a multiplex (`catalog.For(GameEra).<thing>`). Introduce `IEraGameDataCatalog` as the per-era interface. Move existing logic into `NijiiroEraGameDataCatalog`. Add a stub `GreenEraGameDataCatalog`.

**Why this comes after 03:** Handlers (now per-era partials from 03.4) are the catalog's main consumer. Refactoring the catalog after the handlers exist means we can update Nijiiro handler partials' catalog calls in one batch.

**Verification cadence:** `dotnet build` after each task. The Nijiiro catalog content is byte-identical — same files, same parsers, same in-memory dictionaries; only the surface API changes.

---

## Task 04.1: Define `IEraGameDataCatalog` and update `IGameDataCatalog`

**Goal:** Introduce the new abstractions. `IGameDataCatalog.For(GameEra)` returns `IEraGameDataCatalog`. The existing flat properties move to a per-era interface.

**Files:**
- Modify: `Application/Abstractions/IGameDataCatalog.cs` — strip flat surface; expose only `For(GameEra)` and `InitializeAsync`
- Create: `Application/Abstractions/IEraGameDataCatalog.cs` — the per-era surface (shared subset only)
- Create: `Application/Abstractions/IMusicInfoEntry.cs`, `Application/Abstractions/IDanEntry.cs`, etc. — minimal cross-era interfaces for entry types (only the things that are common; era-specific entries don't need an interface)

**Acceptance Criteria:**
- [ ] `IGameDataCatalog` has exactly two members: `For(GameEra) -> IEraGameDataCatalog` and `Task InitializeAsync(CancellationToken)`.
- [ ] `IEraGameDataCatalog` has `Era`, `InitializeAsync`, and the genuinely shared lookups (`MusicInfos`, `Telops`, `EventFolders`).
- [ ] `dotnet build` FAILS at this step (consumers like `Handler`s still reference `catalog.MusicInfos` — that's expected; Task 04.4 fixes them).

**Steps:**

- [ ] **Step 1: Read the current `IGameDataCatalog.cs` to capture the existing flat surface**

```bash
cat Application/Abstractions/IGameDataCatalog.cs
```
Note every property — these become per-era surfaces; the Nijiiro versions go on `NijiiroEraGameDataCatalog`, the Green versions on `GreenEraGameDataCatalog`.

- [ ] **Step 2: Rewrite `Application/Abstractions/IGameDataCatalog.cs`**

```csharp
// Application/Abstractions/IGameDataCatalog.cs
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Application.Abstractions;

public interface IGameDataCatalog
{
    IEraGameDataCatalog For(GameEra era);
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
```

- [ ] **Step 3: Create `Application/Abstractions/IEraGameDataCatalog.cs`**

```csharp
// Application/Abstractions/IEraGameDataCatalog.cs
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Application.Abstractions;

public interface IEraGameDataCatalog
{
    GameEra Era { get; }
    Task InitializeAsync(CancellationToken cancellationToken);

    // Genuinely cross-era lookups go here. Era-specific extras (taikojuku for Green,
    // dan-odai for Nijiiro, shop folder for Nijiiro, item shop for Green) live on the
    // concrete era catalog types and are accessed via casting.
    IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos { get; }
}

public interface IMusicInfoEntry
{
    uint SongNo { get; }
}
```

> Keep `IEraGameDataCatalog` thin. Only add to the shared surface what every era genuinely has AND what callers want to look up era-agnostically. The lookups handlers need today (event folders, telops, dan data, shop folder, etc.) are Nijiiro-only in their current shape — leave them on the concrete `NijiiroEraGameDataCatalog`.

- [ ] **Step 4: Build (expected to fail — consumers reference removed members)**

Run: `dotnet build`
Expected: FAIL with errors like `'IGameDataCatalog' does not contain a definition for 'MusicInfos'`. Note these errors — they're the catalog consumers Task 04.4 needs to rewrite.

- [ ] **Step 5: Do NOT commit yet** — leave the working tree broken; Task 04.2/04.3 add the concrete catalogs, then 04.4 fixes consumers and we commit.

---

## Task 04.2: Refactor `FileGameDataCatalog` into multiplex + `NijiiroEraGameDataCatalog`

**Goal:** The current `FileGameDataCatalog` is monolithic — it loads everything for the Nijiiro era. Split it: `FileGameDataCatalog` becomes a thin multiplexer holding `IReadOnlyDictionary<GameEra, IEraGameDataCatalog>`; the existing loader logic moves to `NijiiroEraGameDataCatalog` and its sibling loader files under `Infrastructure/GameDataCatalog/Nijiiro/`.

**Files:**
- Modify: `Infrastructure/GameDataCatalog/FileGameDataCatalog.cs` — slim to multiplex
- Create: `Infrastructure/GameDataCatalog/Nijiiro/NijiiroEraGameDataCatalog.cs` — per-era impl, owns the existing in-memory dictionaries
- Move: every existing `Infrastructure/GameDataCatalog/*Loader.cs` or `*Reader.cs` into `Infrastructure/GameDataCatalog/Nijiiro/` (where the existing Nijiiro file reading logic already lives)
- Modify: `Infrastructure/GameDataCatalog/PathHelper.cs` — add `GetDataPath(GameEra)` returning `<root>/data/<era>/`, `GetSharedDataPath()` returning `<root>/data/shared/`, `GetDataTablePath(GameEra)` returning `<root>/data/<era>/datatable/`

**Acceptance Criteria:**
- [ ] `FileGameDataCatalog` is small (≤50 lines): constructor takes an `IEnumerable<IEraGameDataCatalog>`, builds a dictionary, exposes `For(GameEra)` and `InitializeAsync` (delegates to children).
- [ ] `NijiiroEraGameDataCatalog` exposes the era-specific surface the existing Nijiiro handlers expect (`MusicInfos`, `DanData`, `EventFolders`, `MovieData`, `ShopFolders`, `SongIntroductions`, `Telops`, etc.).
- [ ] `Infrastructure/GameDataCatalog/Nijiiro/` contains the Nijiiro loaders.
- [ ] `PathHelper` exposes the new per-era / shared path helpers.
- [ ] `dotnet build` is still in a known-broken state from 04.1 (consumers haven't been updated yet) — that's fine until Task 04.4.

**Steps:**

- [ ] **Step 1: Slim `FileGameDataCatalog.cs`**

```csharp
// Infrastructure/GameDataCatalog/FileGameDataCatalog.cs
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog;

public sealed class FileGameDataCatalog : IGameDataCatalog
{
    private readonly IReadOnlyDictionary<GameEra, IEraGameDataCatalog> catalogs;

    public FileGameDataCatalog(IEnumerable<IEraGameDataCatalog> eras)
    {
        catalogs = eras.ToDictionary(c => c.Era);
    }

    public IEraGameDataCatalog For(GameEra era)
    {
        if (!catalogs.TryGetValue(era, out var c))
            throw new InvalidOperationException($"Era {era} is not enabled.");
        return c;
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
        => Task.WhenAll(catalogs.Values.Select(c => c.InitializeAsync(cancellationToken)));
}
```

- [ ] **Step 2: Extend `Infrastructure/GameDataCatalog/PathHelper.cs`**

```csharp
public static string GetDataPath(GameEra era)
    => Path.Combine(GetRootPath(), "data", era.ToString().ToLowerInvariant());

public static string GetSharedDataPath()
    => Path.Combine(GetRootPath(), "data", "shared");

public static string GetDataTablePath(GameEra era)
    => Path.Combine(GetDataPath(era), "datatable");
```

(Keep the existing `GetRootPath()` and the legacy methods that point at `wwwroot/data/datatable/` — Task 07 will deprecate them.)

- [ ] **Step 3: Create `Infrastructure/GameDataCatalog/Nijiiro/NijiiroEraGameDataCatalog.cs`**

This class absorbs every public property the old monolithic `FileGameDataCatalog` exposed. Implementation:
- Constructor takes the same dependencies the old `FileGameDataCatalog` did (`IOptions<DataSettings>` for filenames, `ILogger`).
- `InitializeAsync` calls each existing Nijiiro loader (reads `wwwroot/data/nijiiro/dan_data.json`, etc. via the new `PathHelper.GetDataPath(GameEra.Nijiiro)`).
- All the existing in-memory dictionaries (`MusicInfos`, `DanData`, `EventFolders`, `Telops`, `MovieData`, `ShopFolders`, `SongIntroductions`, `GaidenData`, `LockedData`, `SpecialSongs`, `Intros`) become properties on this type.

Where the old code was:
```csharp
// OLD FileGameDataCatalog.cs
public IReadOnlyDictionary<uint, MusicInfoEntry> MusicInfos => musicInfos;
```
becomes:
```csharp
// NEW NijiiroEraGameDataCatalog.cs
public IReadOnlyDictionary<uint, MusicInfoEntry> MusicInfos => musicInfos;
public IReadOnlyDictionary<uint, IMusicInfoEntry> IEraGameDataCatalog.MusicInfos
    => musicInfos.ToDictionary(kv => kv.Key, kv => (IMusicInfoEntry)kv.Value);
// (or have MusicInfoEntry implement IMusicInfoEntry directly to avoid the projection;
//  that's cleaner — add ': IMusicInfoEntry' to MusicInfoEntry and ensure it has uint SongNo)
```

Cleanest: have `Application/Catalog/MusicInfoEntry.cs` declare `: IMusicInfoEntry` and expose `uint SongNo` as a public property. Then `IEraGameDataCatalog.MusicInfos` just returns `(IReadOnlyDictionary<uint, IMusicInfoEntry>)musicInfos`.

- [ ] **Step 4: Move Nijiiro loaders under `Infrastructure/GameDataCatalog/Nijiiro/`**

The existing files in `Infrastructure/GameDataCatalog/` are Nijiiro-specific by definition. Move them en bloc into `Nijiiro/`. Update namespaces (e.g. `TaikoLocalServer.Infrastructure.GameDataCatalog` → `TaikoLocalServer.Infrastructure.GameDataCatalog.Nijiiro`). Keep `FileGameDataCatalog.cs` and `PathHelper.cs` at the parent level; they're era-agnostic.

- [ ] **Step 5: Update file paths inside the moved Nijiiro loaders**

Every existing loader that did `Path.Combine(GetRootPath(), "data", filename)` now does `Path.Combine(PathHelper.GetDataPath(GameEra.Nijiiro), filename)`. Datatable readers go through `PathHelper.GetDataTablePath(GameEra.Nijiiro)`.

This is a no-op at runtime IF the on-disk files have been moved into `wwwroot/data/nijiiro/` — which is Task 07's job. For dev, the user moves the files separately before running. The plan flow is: code first (this task), file move (07), smoke test (08).

- [ ] **Step 6: Build — STILL in the known-broken state from 04.1**

The catalog refactor is consistent within itself but consumers still need updating.

---

## Task 04.3: Add stub `GreenEraGameDataCatalog`

**Goal:** Stub catalog for Green. `InitializeAsync` is a no-op; all entry dictionaries are empty.

**Files:**
- Create: `Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs`
- Create: `Infrastructure/GameDataCatalog/Green/GreenMusicInfoEntry.cs` (stub: implements `IMusicInfoEntry`, has the fields Green's musicinfo will need)
- Create: `Infrastructure/GameDataCatalog/Green/GreenTaikojukuEntry.cs` (stub value object)
- Create: `Infrastructure/GameDataCatalog/Green/GreenItemShopEntry.cs` (stub)
- Create: `Infrastructure/GameDataCatalog/Green/GreenEventFolderEntry.cs` (stub)
- Create: `Infrastructure/GameDataCatalog/Green/GreenTelopEntry.cs` (stub)
- Create: `Infrastructure/GameDataCatalog/Green/GreenGachaEntry.cs` (stub)
- Create: `Infrastructure/GameDataCatalog/Green/GreenTournamentEntry.cs` (stub)
- Create: `Infrastructure/GameDataCatalog/Green/GreenMusicInfoLoader.cs` (stub `Task<IReadOnlyDictionary<uint, GreenMusicInfoEntry>> LoadAsync(CancellationToken)` returning empty)
- Create: `Infrastructure/GameDataCatalog/Green/GreenTaikojukuLoader.cs` (stub)
- Create: `Infrastructure/GameDataCatalog/Green/GreenItemShopLoader.cs` (stub)
- Create: `Infrastructure/GameDataCatalog/Green/GreenEventFolderLoader.cs` (stub)
- Create: `Infrastructure/GameDataCatalog/Green/GreenTelopLoader.cs` (stub)
- Create: `Infrastructure/GameDataCatalog/Green/GreenGachaLoader.cs` (stub)
- Create: `Infrastructure/GameDataCatalog/Green/GreenTournamentLoader.cs` (stub)

**Acceptance Criteria:**
- [ ] `GreenEraGameDataCatalog` implements `IEraGameDataCatalog`.
- [ ] `Era => GameEra.Green`.
- [ ] All dictionary/list properties return empty collections at startup.
- [ ] `InitializeAsync` logs a warning if the `wwwroot/data/green/` directory is missing AND returns successfully (since Green is opt-in — the warning is a friendly heads-up that the operator hasn't dropped Green binaries in yet). Hard-fail behavior on missing data is intentional and handled in Task 06 — but at the catalog level, the no-op stub initialization shouldn't crash even when files are missing.

> Correction to align with spec §5: the spec says enabled-era missing files should hard-fail. For iter 1 stub posture, soften to "log warning at startup if files missing" so dev can proceed without Green binaries on disk. Iter 2's real loader implementation will hard-fail per the spec.

**Steps:**

- [ ] **Step 1: Create entry stubs**

```csharp
// Infrastructure/GameDataCatalog/Green/GreenMusicInfoEntry.cs
using TaikoLocalServer.Application.Abstractions;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenMusicInfoEntry : IMusicInfoEntry
{
    public uint SongNo { get; init; }
    public string Title { get; init; } = string.Empty;
    // TODO iter 2: add Green-specific fields (category ID for the 8 categ counters, etc.)
}
```

```csharp
// Infrastructure/GameDataCatalog/Green/GreenTaikojukuEntry.cs
namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenTaikojukuEntry
{
    public uint DanLevel { get; init; }
    public IReadOnlyList<TaikojukuSong> Songs { get; init; } = [];
}

public sealed class TaikojukuSong
{
    public uint SongNo { get; init; }
    public uint Level  { get; init; }
}
```

(Apply the same minimal-stub pattern to the other entry types.)

- [ ] **Step 2: Create stub loaders**

```csharp
// Infrastructure/GameDataCatalog/Green/GreenMusicInfoLoader.cs
namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

internal sealed class GreenMusicInfoLoader
{
    public Task<IReadOnlyDictionary<uint, GreenMusicInfoEntry>> LoadAsync(CancellationToken ct)
    {
        // TODO iter 2: parse wwwroot/data/green/datatable/musicinfo.bin (Green binary format)
        IReadOnlyDictionary<uint, GreenMusicInfoEntry> empty = new Dictionary<uint, GreenMusicInfoEntry>();
        return Task.FromResult(empty);
    }
}
```

(Apply the same pattern to all eight Green loaders.)

- [ ] **Step 3: Create `GreenEraGameDataCatalog.cs`**

```csharp
// Infrastructure/GameDataCatalog/Green/GreenEraGameDataCatalog.cs
using TaikoLocalServer.Application.Abstractions;
using TaikoLocalServer.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace TaikoLocalServer.Infrastructure.GameDataCatalog.Green;

public sealed class GreenEraGameDataCatalog : IEraGameDataCatalog
{
    private readonly ILogger<GreenEraGameDataCatalog> logger;
    private IReadOnlyDictionary<uint, GreenMusicInfoEntry> musicInfos
        = new Dictionary<uint, GreenMusicInfoEntry>();
    private IReadOnlyDictionary<uint, GreenTaikojukuEntry> taikojuku
        = new Dictionary<uint, GreenTaikojukuEntry>();
    // ... fields for itemShop, eventFolders, telops, gacha, tournament

    public GreenEraGameDataCatalog(ILogger<GreenEraGameDataCatalog> logger)
    {
        this.logger = logger;
    }

    public GameEra Era => GameEra.Green;

    public IReadOnlyDictionary<uint, IMusicInfoEntry> MusicInfos
        => musicInfos.ToDictionary(kv => kv.Key, kv => (IMusicInfoEntry)kv.Value);

    // Era-specific surfaces — handlers cast to GreenEraGameDataCatalog to reach these:
    public IReadOnlyDictionary<uint, GreenTaikojukuEntry> Taikojuku => taikojuku;
    // public ... ItemShop / EventFolders / Telops / Gacha / Tournament — same pattern

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var greenDataPath = PathHelper.GetDataPath(GameEra.Green);
        if (!Directory.Exists(greenDataPath))
        {
            logger.LogWarning("Green data path {Path} does not exist. Green endpoints will return empty data until binaries are dropped in.", greenDataPath);
            return;
        }

        // TODO iter 2: invoke each loader; populate the in-memory dictionaries
        musicInfos = await new GreenMusicInfoLoader().LoadAsync(cancellationToken);
        taikojuku  = await new GreenTaikojukuLoader().LoadAsync(cancellationToken);
        // ...
    }
}
```

- [ ] **Step 4: Do NOT commit yet** — proceed to 04.4 first to restore the build.

---

## Task 04.4: Update consumers to use `catalog.For(GameEra.Nijiiro).<thing>`

**Goal:** Walk every file that referenced the old flat `IGameDataCatalog.<property>` and rewrite it as `catalog.For(GameEra.Nijiiro).<property>` (or, if the property is on the concrete Nijiiro catalog only, `((NijiiroEraGameDataCatalog)catalog.For(GameEra.Nijiiro)).<property>`).

**Files:**
- Modify: every consumer of `IGameDataCatalog` in `Application/Handlers/`, `Adapters.AdminApi/Controllers/`, `Adapters.GameProtocol.WwR08/`, `Adapters.GameProtocol.CnR00/`, `Adapters.AllnetMucha/` (anywhere that injects `IGameDataCatalog`)

**Acceptance Criteria:**
- [ ] `dotnet build` succeeds.
- [ ] Behavior is byte-identical to before for Nijiiro: same dictionaries, same lookups, same values returned.

**Verify:**
```bash
dotnet build
grep -rn "IGameDataCatalog\b" --include="*.cs" Application Adapters.AdminApi Adapters.GameProtocol.WwR08 Adapters.GameProtocol.CnR00 Adapters.AllnetMucha | grep -v "obj/"
```
Expected: build PASSES; every match in the grep is either an injection point or a `For(GameEra.<X>)` call — no direct `.MusicInfos` / `.DanData` accesses on the catalog interface.

**Steps:**

- [ ] **Step 1: Find every consumer**

```bash
grep -rn "IGameDataCatalog\b\|catalog\." --include="*.cs" Application Adapters.AdminApi Adapters.GameProtocol Adapters.AllnetMucha 2>/dev/null | grep -v "obj/"
```

- [ ] **Step 2: For each call site, rewrite**

Pattern:
```csharp
// before
catalog.MusicInfos.TryGetValue(songNo, out var info);
// after
var nijiiro = (NijiiroEraGameDataCatalog)catalog.For(GameEra.Nijiiro);
nijiiro.MusicInfos.TryGetValue(songNo, out var info);
```

Or, when handlers are per-era already (Task 03.4), the per-era partial knows its era and can cast directly:
```csharp
private partial async ValueTask<...> HandleNijiiro(...)
{
    var nijiiro = (NijiiroEraGameDataCatalog)catalog.For(GameEra.Nijiiro);
    // use nijiiro.MusicInfos etc.
}
```

- [ ] **Step 3: Add `using TaikoLocalServer.Infrastructure.GameDataCatalog.Nijiiro;`** to handler files that need to cast. Or: introduce a typed extension method in `Application/Common/CatalogExtensions.cs`:

```csharp
// Application/Common/CatalogExtensions.cs
namespace TaikoLocalServer.Application.Common;

public static class CatalogExtensions
{
    public static NijiiroEraGameDataCatalog Nijiiro(this IGameDataCatalog catalog)
        => (NijiiroEraGameDataCatalog)catalog.For(GameEra.Nijiiro);

    public static GreenEraGameDataCatalog Green(this IGameDataCatalog catalog)
        => (GreenEraGameDataCatalog)catalog.For(GameEra.Green);
}
```

Wait — these extensions would require `Application` to reference `Infrastructure`, which inverts the dependency. Either:
- (a) Move the extension to `Infrastructure/GameDataCatalog/CatalogExtensions.cs`, or
- (b) Add `INijiiroCatalog` / `IGreenCatalog` interfaces under `Application/Abstractions/` and have the concrete types implement them; the extension casts to the interface.

Option (b) is cleaner. Add:
```csharp
// Application/Abstractions/INijiiroCatalog.cs
public interface INijiiroCatalog : IEraGameDataCatalog
{
    IReadOnlyDictionary<uint, MusicInfoEntry> MusicInfos { get; }
    // ... every Nijiiro surface previously on IGameDataCatalog
}

// Application/Abstractions/IGreenCatalog.cs
public interface IGreenCatalog : IEraGameDataCatalog
{
    IReadOnlyDictionary<uint, GreenMusicInfoEntry> GreenMusicInfos { get; }
    IReadOnlyDictionary<uint, GreenTaikojukuEntry> Taikojuku { get; }
    // ...
}
```

Then `NijiiroEraGameDataCatalog : INijiiroCatalog` and `GreenEraGameDataCatalog : IGreenCatalog`. The extension method:
```csharp
public static INijiiroCatalog Nijiiro(this IGameDataCatalog c) => (INijiiroCatalog)c.For(GameEra.Nijiiro);
public static IGreenCatalog   Green  (this IGameDataCatalog c) => (IGreenCatalog)c.For(GameEra.Green);
```
lives in `Application/Common/`.

Type references like `MusicInfoEntry` need to live in `Application/Catalog/` (already do today). The Green entry types (`GreenMusicInfoEntry`, `GreenTaikojukuEntry`, etc.) need to either:
- Move to `Application/Catalog/Green/` for clean reference, OR
- Stay in `Infrastructure/GameDataCatalog/Green/` and have `IGreenCatalog` expose them via an interface that's also in Application.

Cleanest: move Green entry types to `Application/Catalog/Green/`. Loaders stay in Infrastructure. This mirrors the existing `Application/Catalog/MusicInfoEntry.cs` pattern.

- [ ] **Step 4: Rewrite call sites using the new extension methods**

```csharp
// before
var musicInfo = catalog.MusicInfos.GetValueOrDefault(songNo);
// after
var musicInfo = catalog.Nijiiro().MusicInfos.GetValueOrDefault(songNo);
```

- [ ] **Step 5: Build**

Run: `dotnet build`
Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add Application Infrastructure Adapters.AdminApi Adapters.GameProtocol.WwR08 Adapters.GameProtocol.CnR00 Adapters.AllnetMucha
git commit -m "refactor(catalog): multiplex IGameDataCatalog.For(GameEra); move Nijiiro logic into NijiiroEraGameDataCatalog; add Green stub catalog"
```

---

## Task 04.5: DI registration for the multiplex

**Goal:** Wire up `INijiiroCatalog` → `NijiiroEraGameDataCatalog` and `IGreenCatalog` → `GreenEraGameDataCatalog` in DI. `IGameDataCatalog` is a singleton resolving its children. Wiring is conditional in Task 06 — for now, register both unconditionally so the build runs.

**Files:**
- Modify: `Infrastructure/DependencyInjection.cs`

**Acceptance Criteria:**
- [ ] `AddInfrastructure` registers `NijiiroEraGameDataCatalog` and `GreenEraGameDataCatalog` as singletons.
- [ ] `IGameDataCatalog` is registered as a singleton factory that constructs `FileGameDataCatalog` with both eras (conditional registration in Task 06).
- [ ] `INijiiroCatalog` and `IGreenCatalog` are registered as singleton aliases of the concrete types.
- [ ] `dotnet build` succeeds.

**Steps:**

- [ ] **Step 1: Replace `services.AddSingleton<IGameDataCatalog, FileGameDataCatalog>();`**

```csharp
services.AddSingleton<NijiiroEraGameDataCatalog>();
services.AddSingleton<INijiiroCatalog>(sp => sp.GetRequiredService<NijiiroEraGameDataCatalog>());
services.AddSingleton<IEraGameDataCatalog>(sp => sp.GetRequiredService<NijiiroEraGameDataCatalog>());

services.AddSingleton<GreenEraGameDataCatalog>();
services.AddSingleton<IGreenCatalog>(sp => sp.GetRequiredService<GreenEraGameDataCatalog>());
services.AddSingleton<IEraGameDataCatalog>(sp => sp.GetRequiredService<GreenEraGameDataCatalog>());

services.AddSingleton<IGameDataCatalog>(sp => new FileGameDataCatalog(
    sp.GetServices<IEraGameDataCatalog>()));
```

> Note: `sp.GetServices<IEraGameDataCatalog>()` resolves all registrations of that service type. Both Nijiiro and Green register as `IEraGameDataCatalog` here, so `FileGameDataCatalog` receives both. Task 06 will gate these registrations behind the `Eras` settings — only enabled eras register as `IEraGameDataCatalog`.

- [ ] **Step 2: Build**

Run: `dotnet build`
Expected: PASS.

- [ ] **Step 3: Commit**

```bash
git add Infrastructure/DependencyInjection.cs
git commit -m "feat(infra): register Nijiiro + Green catalogs in DI (both unconditional; opt-in gating arrives in 06)"
```
