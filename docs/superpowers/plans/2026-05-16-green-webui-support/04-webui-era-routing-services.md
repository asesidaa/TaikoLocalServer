# Task 4: WebUI Era Routing And Services

**Goal:** Add WebUI helpers and service methods so pages can build era-aware URLs and load era-specific catalog/API data.

**Files:**
- Create: `TaikoWebUI/Utilities/WebUiEra.cs`
- Modify: `TaikoWebUI/Services/IGameDataService.cs`
- Modify: `TaikoWebUI/Services/GameDataService.cs`
- Modify: `TaikoWebUI/Components/NavMenu.razor`
- Modify: `TaikoWebUI/Components/UserCard.razor`

- [ ] **Step 1: Add WebUI era helper**

Create `TaikoWebUI/Utilities/WebUiEra.cs`:

```csharp
namespace TaikoWebUI.Utilities;

public static class WebUiEra
{
    public const string Default = "Nijiiro";
    public static readonly string[] Supported = ["Nijiiro", "Green"];

    public static bool IsSupported(string? era)
    {
        return Supported.Any(value => string.Equals(value, era, StringComparison.OrdinalIgnoreCase));
    }

    public static string Normalize(string? era)
    {
        return Supported.FirstOrDefault(value => string.Equals(value, era, StringComparison.OrdinalIgnoreCase)) ?? Default;
    }

    public static string UserRoute(uint baid, string? era, string page)
    {
        return $"Users/{baid}/{Normalize(era)}/{page.TrimStart('/')}";
    }

    public static string UserRoute(int baid, string? era, string page)
    {
        return UserRoute((uint)baid, era, page);
    }

    public static string Api(string? era, string path)
    {
        return $"api/{Normalize(era)}/{path.TrimStart('/')}";
    }
}
```

- [ ] **Step 2: Add era-aware service interface methods**

Modify `TaikoWebUI/Services/IGameDataService.cs`:

```csharp
public Task<Dictionary<uint, MusicDetail>> GetMusicDetailDictionary(string? era);

public ImmutableDictionary<uint, DanData> GetDanMap(string? era);
```

Keep the existing parameterless methods for Nijiiro compatibility.

- [ ] **Step 3: Update GameDataService cache keys and routes**

Modify `TaikoWebUI/Services/GameDataService.cs`:

- Add `using TaikoWebUI.Utilities;`.
- Replace single `danMap`, `musicDetailDictionary`, and `musicDetailInitialized` with per-era dictionaries:

```csharp
private readonly Dictionary<string, ImmutableDictionary<uint, DanData>> danMaps = new();
private readonly Dictionary<string, Dictionary<uint, MusicDetail>> musicDetailDictionaries = new();
```

- Implement:

```csharp
public Task<Dictionary<uint, MusicDetail>> GetMusicDetailDictionary()
    => GetMusicDetailDictionary(WebUiEra.Default);

public async Task<Dictionary<uint, MusicDetail>> GetMusicDetailDictionary(string? era)
{
    var normalized = WebUiEra.Normalize(era);
    if (!musicDetailDictionaries.TryGetValue(normalized, out var value))
    {
        value = await client.GetFromJsonAsync<Dictionary<uint, MusicDetail>>(WebUiEra.Api(normalized, "GameData/MusicDetails"))
                ?? new Dictionary<uint, MusicDetail>();
        musicDetailDictionaries[normalized] = value;
    }

    return value;
}

public ImmutableDictionary<uint, DanData> GetDanMap()
    => GetDanMap(WebUiEra.Default);

public ImmutableDictionary<uint, DanData> GetDanMap(string? era)
{
    var normalized = WebUiEra.Normalize(era);
    return danMaps.TryGetValue(normalized, out var value) ? value : ImmutableDictionary<uint, DanData>.Empty;
}
```

- Update `InitializeAsync` to seed Nijiiro `danMaps[WebUiEra.Default]` from existing `data/dan_data.json` until AdminApi DanData route is wired into startup.

- [ ] **Step 4: Update NavMenu links**

Modify `TaikoWebUI/Components/NavMenu.razor`:

- Add `@using TaikoWebUI.Utilities`.
- Add a local selected era value in `@code`:

```csharp
private string selectedEra = WebUiEra.Default;
```

- Change logged-in links to use `WebUiEra.UserRoute((uint)baid, selectedEra, "Profile")`, `Songs`, `HighScores`, `PlayHistory`, and `DaniDojo`.
- Add a compact `MudSelect` or `MudMenu` near the Play Data group that sets `selectedEra` to `Nijiiro` or `Green` and navigates to the current equivalent page when possible. Keep the first implementation simple: changing the selector only changes subsequent nav links if current route parsing is too involved.

- [ ] **Step 5: Update UserCard links**

Modify `TaikoWebUI/Components/UserCard.razor`:

- Add `@using TaikoWebUI.Utilities`.
- Use `WebUiEra.UserRoute(User.Baid, WebUiEra.Default, "Profile")` for edit profile.
- Add separate Nijiiro and Green entries in the play-data menu:

```razor
<MudMenuItem Href="@(WebUiEra.UserRoute(User.Baid, "Nijiiro", "HighScores"))">Nijiiro - @Localizer["High Scores"]</MudMenuItem>
<MudMenuItem Href="@(WebUiEra.UserRoute(User.Baid, "Green", "HighScores"))">Green - @Localizer["High Scores"]</MudMenuItem>
```

Repeat for Play History, Song List, and Dani Dojo.

- [ ] **Step 6: Build WebUI**

Run:

```powershell
dotnet build TaikoWebUI/TaikoWebUI.csproj
```

Expected: build succeeds.

- [ ] **Step 7: Commit**

```powershell
git add -- TaikoWebUI/Utilities/WebUiEra.cs TaikoWebUI/Services/IGameDataService.cs TaikoWebUI/Services/GameDataService.cs TaikoWebUI/Components/NavMenu.razor TaikoWebUI/Components/UserCard.razor
git commit -m "Add WebUI era routing helpers"
```