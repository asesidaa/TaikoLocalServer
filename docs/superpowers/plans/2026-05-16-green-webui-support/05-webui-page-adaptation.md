# Task 5: WebUI Page Adaptation

**Goal:** Update shared WebUI pages to accept `{era}` routes, call era-aware APIs, and hide unsupported Green UI states.

**Files:**
- Modify: `TaikoWebUI/Pages/Profile.razor`
- Modify: `TaikoWebUI/Pages/Profile.razor.cs`
- Modify: `TaikoWebUI/Pages/HighScores.razor`
- Modify: `TaikoWebUI/Pages/HighScores.razor.cs`
- Modify: `TaikoWebUI/Pages/PlayHistory.razor`
- Modify: `TaikoWebUI/Pages/PlayHistory.razor.cs`
- Modify: `TaikoWebUI/Pages/SongList.razor`
- Modify: `TaikoWebUI/Pages/SongList.razor.cs`
- Modify: `TaikoWebUI/Pages/Song.razor`
- Modify: `TaikoWebUI/Pages/Song.razor.cs`
- Modify: `TaikoWebUI/Pages/DaniDojo.razor`
- Modify: `TaikoWebUI/Pages/DaniDojo.razor.cs`

- [ ] **Step 1: Add era routes to gameplay pages**

For each page below, keep the existing route for Nijiiro compatibility and add the era-aware route:

```razor
@page "/Users/{baid:int}/{era}/Profile"
@page "/Users/{baid:int}/{era}/HighScores"
@page "/Users/{baid:int}/{era}/PlayHistory"
@page "/Users/{baid:int}/{era}/Songs"
@page "/Users/{baid:int}/{era}/Songs/{songId:int}"
@page "/Users/{baid:int}/{era}/DaniDojo"
```

In each matching `.razor.cs`, add:

```csharp
[Parameter]
public string? Era { get; set; }

private string CurrentEra => WebUiEra.Normalize(Era);
private bool IsGreen => string.Equals(CurrentEra, "Green", StringComparison.OrdinalIgnoreCase);
```

Add `using TaikoWebUI.Utilities;` where needed.

- [ ] **Step 2: Update API calls**

Replace page API calls:

```csharp
Client.GetFromJsonAsync<SongBestResponse>($"api/PlayData/{Baid}")
Client.GetFromJsonAsync<SongHistoryResponse>($"api/PlayHistory/{Baid}")
Client.GetFromJsonAsync<DanBestDataResponse>($"api/DanBestData/{Baid}")
Client.GetFromJsonAsync<UserSetting>($"api/UserSettings/{Baid}")
Client.PostAsJsonAsync("api/FavoriteSongs", request)
```

with era-aware calls:

```csharp
Client.GetFromJsonAsync<SongBestResponse>(WebUiEra.Api(CurrentEra, $"PlayData/{Baid}"))
Client.GetFromJsonAsync<SongHistoryResponse>(WebUiEra.Api(CurrentEra, $"PlayHistory/{Baid}"))
Client.GetFromJsonAsync<DanBestDataResponse>(WebUiEra.Api(CurrentEra, $"DanBestData/{Baid}"))
Client.GetFromJsonAsync<UserSetting>(WebUiEra.Api(CurrentEra, $"UserSettings/{Baid}"))
Client.PostAsJsonAsync(WebUiEra.Api(CurrentEra, "FavoriteSongs"), request)
```

If `UserSettingsController` is still Nijiiro-only when this task starts, keep Profile using the compatibility `api/UserSettings/{Baid}` for Green and add this exact comment:

```csharp
// TODO Green WebUI: replace this compatibility settings call when Green settings editing is implemented.
```

- [ ] **Step 3: Load era-specific catalogs**

Replace `GameDataService.GetMusicDetailDictionary()` calls in gameplay pages with:

```csharp
musicDetailDictionary = await GameDataService.GetMusicDetailDictionary(CurrentEra);
```

Replace `GameDataService.GetDanMap()` in DaniDojo with:

```csharp
danMap = GameDataService.GetDanMap(CurrentEra);
```

- [ ] **Step 4: Update route links inside pages**

Replace links like:

```razor
<a href="@($"/Users/{Baid}/Songs/{context.SongId}")">
```

with:

```razor
<a href="@(WebUiEra.UserRoute(Baid, CurrentEra, $"Songs/{context.SongId}"))">
```

Update breadcrumb hrefs similarly for Profile, HighScores, PlayHistory, Songs, Song detail, and DaniDojo.

- [ ] **Step 5: Hide Green unsupported score-rank and perfect-crown UI**

In HighScores and PlayHistory `.razor` files:

- Keep rank columns for Nijiiro.
- For Green, either omit the rank column or leave cells empty when `ScoreRank.None`.
- Hide any Dondaful/perfect crown summary counts when `IsGreen` is true.

Use this exact comment at each intentionally hidden Green section:

```razor
@* TODO Green WebUI: enable this control when Green exposes the underlying feature. *@
```

- [ ] **Step 6: Show Green alternate score facet**

In `HighScores.razor`, under the primary best score, add:

```razor
@if (context.AlternateScore is not null)
{
    <MudText Typo="Typo.caption">@context.AlternateScore.Label: @context.AlternateScore.BestScore</MudText>
}
```

If crown display for alternate score fits the existing table without layout churn, include the alternate crown icon next to the alternate score. If not, keep the text-only alternate score in this first slice.

- [ ] **Step 7: Enforce Green favorite cap in WebUI**

In each favorite toggle handler (`HighScores`, `SongList`, `Song`, `PlayHistory`), before posting a Green favorite add:

```csharp
if (IsGreen && !data.IsFavorite && CountCurrentFavorites() >= 5)
{
    await DialogService.ShowMessageBoxAsync(
        Localizer["Error"],
        "Green supports at most 5 favorite songs.",
        Localizer["Dialog OK"]);
    return;
}
```

Implement `CountCurrentFavorites()` locally per page from the page's loaded data source. For pages without easy global favorite state, rely on the server response and leave the toggle unchanged on non-success.

- [ ] **Step 8: Hide Green profile controls with explicit TODO comments**

In `Profile.razor`, wrap unsupported Green costume/title/settings sections:

```razor
@if (!IsGreen)
{
    ... existing Nijiiro controls ...
}
else
{
    @* TODO Green WebUI: add Green costume/settings controls after Green settings persistence is implemented. *@
}
```

Do this for costume selectors, title editing, detailed settings controls, and any rank/perfect-crown-only summary UI.

- [ ] **Step 9: Build WebUI**

Run:

```powershell
dotnet build TaikoWebUI/TaikoWebUI.csproj
```

Expected: build succeeds.

- [ ] **Step 10: Commit**

```powershell
git add -- TaikoWebUI/Pages/Profile.razor TaikoWebUI/Pages/Profile.razor.cs TaikoWebUI/Pages/HighScores.razor TaikoWebUI/Pages/HighScores.razor.cs TaikoWebUI/Pages/PlayHistory.razor TaikoWebUI/Pages/PlayHistory.razor.cs TaikoWebUI/Pages/SongList.razor TaikoWebUI/Pages/SongList.razor.cs TaikoWebUI/Pages/Song.razor TaikoWebUI/Pages/Song.razor.cs TaikoWebUI/Pages/DaniDojo.razor TaikoWebUI/Pages/DaniDojo.razor.cs
git commit -m "Add Green WebUI page routes"
```