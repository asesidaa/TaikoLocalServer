# 06 — Host wiring and `ServerSettings.Eras` opt-in

**Surface:** Extend `ServerSettings` with per-era opt-in. `Program.cs` reads the enabled set, hard-fails if zero enabled, conditionally registers each adapter's `Add*` method, and conditionally constructs each era's catalog. ApplicationPart filter removes disabled-era adapter assemblies from MVC's controller discovery.

**Why this comes after 05:** Green adapter exists, can be conditionally referenced.

**Verification cadence:** `dotnet build`, then validate startup logic by reading the code (the **user** runs the server in Task 08).

---

## Task 06.1: Extend `ServerSettings` with per-era settings

**Goal:** Add `Eras` dictionary to the `ServerSettings` POCO. Add the corresponding JSON section to `Host/Configurations/ServerSettings.json`.

**Files:**
- Modify: `Application/Settings/ServerSettings.cs` (extend POCO)
- Modify: `Host/Configurations/ServerSettings.json` (add `Eras` section)

**Acceptance Criteria:**
- [ ] `ServerSettings` has `Dictionary<string, EraSettings> Eras { get; set; } = new();`
- [ ] `EraSettings` defines `bool Enabled { get; set; }`.
- [ ] `Host/Configurations/ServerSettings.json` has an `Eras` section with `Nijiiro.Enabled: true` and `Green.Enabled: false`.
- [ ] `dotnet build` succeeds.

**Verify:**
```bash
dotnet build
cat Host/Configurations/ServerSettings.json
```
Expected: build PASSES; JSON shows the new section.

**Steps:**

- [ ] **Step 1: Read the existing `Application/Settings/ServerSettings.cs`**

```bash
cat Application/Settings/ServerSettings.cs
```

Note the existing properties (`EnableMoreSongs`, `MoreSongsSize`, anything else).

- [ ] **Step 2: Extend the POCO**

```csharp
// Application/Settings/ServerSettings.cs
namespace TaikoLocalServer.Application.Settings;

public sealed class ServerSettings
{
    // existing properties preserved verbatim — EnableMoreSongs, MoreSongsSize, etc.
    public bool EnableMoreSongs { get; set; }
    public int MoreSongsSize { get; set; }

    // NEW
    public Dictionary<string, EraSettings> Eras { get; set; } = new();
}

public sealed class EraSettings
{
    public bool Enabled { get; set; }
}
```

- [ ] **Step 3: Extend `Host/Configurations/ServerSettings.json`**

```jsonc
{
  "EnableMoreSongs": false,
  // existing fields preserved
  "Eras": {
    "Nijiiro": { "Enabled": true },
    "Green":   { "Enabled": false }
  }
}
```

- [ ] **Step 4: Build**

Run: `dotnet build`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add Application/Settings/ServerSettings.cs Host/Configurations/ServerSettings.json
git commit -m "feat(host): add ServerSettings.Eras per-era opt-in (Nijiiro on, Green off by default)"
```

---

## Task 06.2: Read enabled eras at startup and hard-fail when none enabled

**Goal:** In `Program.cs`, parse `ServerSettings.Eras` into a `HashSet<GameEra>`. If empty, log Fatal and throw to terminate startup.

**Files:**
- Modify: `Host/Program.cs`

**Acceptance Criteria:**
- [ ] `Program.cs` declares `var enabledEras = ...` after configuration load and before `builder.Services.AddXxx()` calls.
- [ ] If `enabledEras.Count == 0`, the server refuses to start with a Fatal log entry.
- [ ] Existing Nijiiro behavior with default config (Nijiiro enabled, Green disabled) is unchanged.
- [ ] `dotnet build` succeeds.

**Verify:**
```bash
dotnet build
```

**Steps:**

- [ ] **Step 1: Add the era-read block to `Host/Program.cs`**

Insert AFTER `builder.Configuration.AddJsonFile(...)` block and BEFORE the first `builder.Services.Add*` call:

```csharp
// Around line ~76 of Program.cs, after the EnableMoreSongs check
var serverSettingsConfig = builder.Configuration.GetSection("ServerSettings");
var enabledEras = (serverSettingsConfig.GetSection("Eras").GetChildren() ?? Enumerable.Empty<IConfigurationSection>())
    .Where(s => s.GetValue<bool>("Enabled"))
    .Select(s => Enum.Parse<GameEra>(s.Key, ignoreCase: true))
    .ToHashSet();

if (enabledEras.Count == 0)
{
    Log.Fatal("ServerSettings.Eras has no enabled era. At least one era (Nijiiro or Green) must be enabled in Host/Configurations/ServerSettings.json. Refusing to start.");
    throw new InvalidOperationException("No game eras enabled.");
}

Log.Information("Enabled game eras: {Eras}", string.Join(", ", enabledEras));
```

Add `using TaikoLocalServer.Domain.Enums;` to the top of `Program.cs` if not already in global usings.

- [ ] **Step 2: Build**

Run: `dotnet build`
Expected: PASS.

- [ ] **Step 3: Commit**

```bash
git add Host/Program.cs
git commit -m "feat(host): read ServerSettings.Eras at startup; hard-fail if none enabled"
```

---

## Task 06.3: Conditional adapter registration in `Program.cs`

**Goal:** Wrap each game-protocol adapter's `Add*` call in an `if (enabledEras.Contains(GameEra.X))` block.

**Files:**
- Modify: `Host/Program.cs`

**Acceptance Criteria:**
- [ ] `AddGameProtocolWwR08()` and `AddGameProtocolCnR00()` are gated by `enabledEras.Contains(GameEra.Nijiiro)`.
- [ ] `AddGameProtocolGreen()` is gated by `enabledEras.Contains(GameEra.Green)`.
- [ ] Other adapters (`AddAdminApi`, `AddAllnetMucha`) keep their unconditional registration — they're era-agnostic.
- [ ] `dotnet build` succeeds.

**Steps:**

- [ ] **Step 1: Wrap the adapter registrations**

Locate the block in `Program.cs`:
```csharp
builder.Services.AddGameProtocolWwR08();
builder.Services.AddGameProtocolCnR00();
```

Replace with:
```csharp
if (enabledEras.Contains(GameEra.Nijiiro))
{
    builder.Services.AddGameProtocolWwR08();
    builder.Services.AddGameProtocolCnR00();
}
if (enabledEras.Contains(GameEra.Green))
{
    builder.Services.AddGameProtocolGreen();
}
```

Add `using TaikoLocalServer.Adapters.GameProtocol.Green;` to the top of `Program.cs` (alongside the existing `WwR08` / `CnR00` usings).

- [ ] **Step 2: Build**

Run: `dotnet build`
Expected: PASS.

- [ ] **Step 3: Commit**

```bash
git add Host/Program.cs
git commit -m "feat(host): gate game-protocol adapter registration on ServerSettings.Eras"
```

---

## Task 06.4: Filter disabled-era adapter assemblies from MVC's ApplicationPartManager

**Goal:** Even when an adapter's `Add*` isn't called, its assembly is loaded and its controllers may be auto-discovered by ASP.NET Core. Explicitly remove disabled-era adapter assemblies from `ApplicationPartManager.ApplicationParts` so their controllers don't get routed.

**Files:**
- Modify: `Host/Program.cs`

**Acceptance Criteria:**
- [ ] The `AddControllers().AddProtoBufNet().ConfigureApplicationPartManager(...)` chain removes Green's adapter assembly when Green is disabled.
- [ ] Same for the Nijiiro adapters (WwR08 + CnR00) when Nijiiro is disabled — even though this is unusual (Nijiiro enabled by default).
- [ ] `dotnet build` succeeds.

**Steps:**

- [ ] **Step 1: Update the `AddControllers` call**

Locate:
```csharp
builder.Services.AddControllers().AddProtoBufNet();
```

Replace with:
```csharp
builder.Services.AddControllers()
    .AddProtoBufNet()
    .ConfigureApplicationPartManager(apm =>
    {
        // Default ASP.NET Core behavior: adapter assemblies referenced by Host are
        // auto-discovered as ApplicationParts. To honor per-era opt-in, we explicitly
        // REMOVE disabled-era adapter assemblies so their controllers aren't routed.
        if (!enabledEras.Contains(GameEra.Green))
        {
            RemoveApplicationPart(apm, "Adapters.GameProtocol.Green");
        }
        if (!enabledEras.Contains(GameEra.Nijiiro))
        {
            RemoveApplicationPart(apm, "Adapters.GameProtocol.WwR08");
            RemoveApplicationPart(apm, "Adapters.GameProtocol.CnR00");
        }
    });

static void RemoveApplicationPart(ApplicationPartManager apm, string assemblyName)
{
    var part = apm.ApplicationParts.FirstOrDefault(p =>
        p is AssemblyPart a && a.Assembly.GetName().Name == assemblyName);
    if (part is not null)
    {
        apm.ApplicationParts.Remove(part);
    }
}
```

Required usings (add to top of file if not in global usings):
```csharp
using Microsoft.AspNetCore.Mvc.ApplicationParts;
```

- [ ] **Step 2: Build**

Run: `dotnet build`
Expected: PASS.

- [ ] **Step 3: Commit**

```bash
git add Host/Program.cs
git commit -m "feat(host): filter disabled-era adapter assemblies from ApplicationPartManager"
```

---

## Task 06.5: Conditional catalog registration in `Infrastructure.AddInfrastructure`

**Goal:** Only register a per-era catalog (`NijiiroEraGameDataCatalog`, `GreenEraGameDataCatalog`) when its era is enabled. `FileGameDataCatalog` receives only the enabled children via `sp.GetServices<IEraGameDataCatalog>()`.

**Files:**
- Modify: `Infrastructure/DependencyInjection.cs` — accept the enabled-era set as a parameter
- Modify: `Host/Program.cs` — pass `enabledEras` to `AddInfrastructure`

**Acceptance Criteria:**
- [ ] `AddInfrastructure` signature is `AddInfrastructure(this IServiceCollection services, IConfiguration configuration, ISet<GameEra> enabledEras)`.
- [ ] Nijiiro catalog services are only registered when Nijiiro is enabled.
- [ ] Green catalog services are only registered when Green is enabled.
- [ ] `IGameDataCatalog` factory uses `sp.GetServices<IEraGameDataCatalog>()` to enumerate registered catalogs.
- [ ] `dotnet build` succeeds.

**Steps:**

- [ ] **Step 1: Update `Infrastructure/DependencyInjection.cs`**

```csharp
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration,
    ISet<GameEra> enabledEras)
{
    // ... existing settings + persistence + identity + time registrations preserved verbatim ...

    // Game data catalog — conditional per enabled era
    if (enabledEras.Contains(GameEra.Nijiiro))
    {
        services.AddSingleton<NijiiroEraGameDataCatalog>();
        services.AddSingleton<INijiiroCatalog>(sp => sp.GetRequiredService<NijiiroEraGameDataCatalog>());
        services.AddSingleton<IEraGameDataCatalog>(sp => sp.GetRequiredService<NijiiroEraGameDataCatalog>());
    }
    if (enabledEras.Contains(GameEra.Green))
    {
        services.AddSingleton<GreenEraGameDataCatalog>();
        services.AddSingleton<IGreenCatalog>(sp => sp.GetRequiredService<GreenEraGameDataCatalog>());
        services.AddSingleton<IEraGameDataCatalog>(sp => sp.GetRequiredService<GreenEraGameDataCatalog>());
    }
    services.AddSingleton<IGameDataCatalog>(sp =>
        new FileGameDataCatalog(sp.GetServices<IEraGameDataCatalog>()));

    return services;
}
```

Add `using TaikoLocalServer.Domain.Enums;` if not already in global usings.

- [ ] **Step 2: Update `Host/Program.cs` call site**

```csharp
// before
builder.Services.AddInfrastructure(builder.Configuration);
// after
builder.Services.AddInfrastructure(builder.Configuration, enabledEras);
```

- [ ] **Step 3: Build**

Run: `dotnet build`
Expected: PASS.

- [ ] **Step 4: Commit**

```bash
git add Infrastructure/DependencyInjection.cs Host/Program.cs
git commit -m "feat(infra): gate catalog registration on enabled eras"
```

---

## Task 06.6: Update catalog `InitializeAsync` to hard-fail on missing data files (Nijiiro)

**Goal:** Per spec §5 — enabled eras with missing data files should hard-fail at startup. Nijiiro's existing loaders mostly do this (file-not-found throws). Ensure the propagation reaches `Program.cs`.

**Files:**
- Modify: `Host/Program.cs` — the `await catalog.InitializeAsync()` call should be inside the main try-block so Serilog logs the failure as Fatal.

**Acceptance Criteria:**
- [ ] The existing `await gameDataCatalog.InitializeAsync()` in `Program.cs` remains inside the outer try-block.
- [ ] Any exception thrown from a per-era catalog's `InitializeAsync` propagates and is caught by the outer `catch (Exception ex) when (...)` Fatal-log block.
- [ ] `dotnet build` succeeds.

**Steps:**

- [ ] **Step 1: Verify the existing flow**

Read `Program.cs`:
```csharp
var gameDataCatalog = app.Services.GetService<IGameDataCatalog>();
gameDataCatalog.ThrowIfNull();
await gameDataCatalog.InitializeAsync();
```

This is already inside the outer try-block. Exceptions propagate. No change needed at the Host level.

- [ ] **Step 2: Confirm Nijiiro catalog raises on missing files**

In `Infrastructure/GameDataCatalog/Nijiiro/NijiiroEraGameDataCatalog.cs`, the loaders open files with `File.ReadAllBytes(...)` / `File.OpenRead(...)`. If a required file is missing, `FileNotFoundException` propagates. Confirm by code reading; no change unless the existing code silently swallows file errors (in which case un-swallow them now).

- [ ] **Step 3: Green stub catalog logs warning on missing dir (Task 04.3 already wired this)**

The stub `GreenEraGameDataCatalog.InitializeAsync` logs a warning and returns when `wwwroot/data/green/` doesn't exist. This is intentionally lenient for iter 1 since Green is stub-only. The real loader in iter 2 will hard-fail.

- [ ] **Step 4: No code change — this is a verification step**

Commit nothing (no diff). Move on.

---

## Task 06.7: Confirm-startup sanity (build-only — user runs the server)

**Goal:** Final verification that the wired-up startup builds cleanly. Real run happens in Task 08 under user supervision.

**Steps:**

- [ ] **Step 1: Solution-wide build**

Run: `dotnet build`
Expected: PASS with zero new warnings.

- [ ] **Step 2: Inspect Program.cs final state**

```bash
grep -n "enabledEras\|AddGameProtocol\|ConfigureApplicationPartManager\|AddInfrastructure" Host/Program.cs
```

Expected: the era-gated registrations, the part filter, and the per-era catalog wiring are all visible and reference `enabledEras` consistently.

- [ ] **Step 3: Commit a no-op marker if anything was missed**

If you found any inconsistency in Steps 1–2 of the file's earlier tasks, fix and commit. Otherwise nothing to do.

> Real startup verification happens in Task 08. The **user** runs the server.
