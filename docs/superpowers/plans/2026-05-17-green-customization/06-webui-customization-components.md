# 06 - WebUI Customization Components

**Goal:** Replace the inactive Green profile customization block and Nijiiro hard-coded customization controls with reusable era-aware components.

**Files:**
- Create: `TaikoWebUI/Shared/Customize/CustomizeValues.cs`
- Create: `TaikoWebUI/Shared/Customize/CostumePicker.razor`
- Create: `TaikoWebUI/Shared/Customize/TitlePicker.razor`
- Create: `TaikoWebUI/Shared/Customize/NeiroPicker.razor`
- Create: `TaikoWebUI/Shared/Customize/ColorPicker.razor`
- Modify: `TaikoWebUI/_Imports.razor`
- Modify: `TaikoWebUI/Services/IGameDataService.cs`
- Modify: `TaikoWebUI/Services/GameDataService.cs`
- Modify: `TaikoWebUI/Pages/Profile.razor`
- Modify: `TaikoWebUI/Pages/Profile.razor.cs`

## Task 1: Shared Component Values

- [ ] **Step 1: Create value records**

Create `TaikoWebUI/Shared/Customize/CustomizeValues.cs`:

```csharp
namespace TaikoWebUI.Shared.Customize;

public sealed record CostumePickerValue(uint CurrentId, IReadOnlyList<uint> UnlockedIds);

public sealed record TitlePickerValue(
    string Title,
    uint TitlePlateId,
    IReadOnlyList<uint> UnlockedTitleIds);

public sealed record NeiroPickerValue(uint CurrentId, IReadOnlyList<uint> UnlockedIds);

public sealed record ColorPickerValue(uint BodyColor, uint FaceColor, uint LimbColor);
```

- [ ] **Step 2: Add namespace import**

Modify `TaikoWebUI/_Imports.razor` and add:

```razor
@using TaikoWebUI.Shared.Customize
```

## Task 2: Components

- [ ] **Step 1: Create costume picker**

Create `TaikoWebUI/Shared/Customize/CostumePicker.razor`:

```razor
@if (Catalog.Count > 0)
{
    <MudStack Spacing="2">
        <MudSelect T="uint"
                   Value="Value.CurrentId"
                   ValueChanged="SetCurrent"
                   Label="@Label"
                   Disabled="@ReadOnly"
                   AnchorOrigin="Origin.BottomCenter">
            @foreach (var costume in OrderedCatalog)
            {
                <MudSelectItem Value="@costume.CostumeId">@DisplayName(costume)</MudSelectItem>
            }
        </MudSelect>

        @if (!ReadOnly)
        {
            <MudExpansionPanels Elevation="0" Outlined="true">
                <MudExpansionPanel Text="@Localizer["Unlocked"]">
                    <MudStack Spacing="1">
                        @foreach (var costume in OrderedCatalog)
                        {
                            var isUnlocked = IsUnlocked(costume.CostumeId);
                            <MudCheckBox T="bool"
                                         Value="@isUnlocked"
                                         ValueChanged="@(value => ToggleUnlocked(costume.CostumeId, value))"
                                         Label="@DisplayName(costume)" />
                        }
                    </MudStack>
                </MudExpansionPanel>
            </MudExpansionPanels>
        }
    </MudStack>
}

@code {
    [Parameter] public string Era { get; set; } = string.Empty;
    [Parameter] public string Label { get; set; } = string.Empty;
    [Parameter] public IReadOnlyList<Costume> Catalog { get; set; } = [];
    [Parameter] public CostumePickerValue Value { get; set; } = new(0, []);
    [Parameter] public EventCallback<CostumePickerValue> ValueChanged { get; set; }
    [Parameter] public bool ReadOnly { get; set; }

    private IReadOnlyList<Costume> OrderedCatalog => Catalog
        .OrderBy(costume => costume.CostumeType == "unknown" ? 1 : 0)
        .ThenBy(costume => costume.CostumeId)
        .ToArray();

    private bool IsUnlocked(uint id) => Value.UnlockedIds.Contains(id);

    private async Task SetCurrent(uint id)
    {
        var unlocked = Value.UnlockedIds.Contains(id)
            ? Value.UnlockedIds
            : Value.UnlockedIds.Append(id).Distinct().OrderBy(value => value).ToArray();
        await ValueChanged.InvokeAsync(Value with { CurrentId = id, UnlockedIds = unlocked });
    }

    private async Task ToggleUnlocked(uint id, bool enabled)
    {
        var unlocked = enabled
            ? Value.UnlockedIds.Append(id)
            : Value.UnlockedIds.Where(value => value != id);
        var normalized = unlocked.Append(0).Distinct().OrderBy(value => value).ToArray();
        var current = normalized.Contains(Value.CurrentId) ? Value.CurrentId : 0;
        await ValueChanged.InvokeAsync(Value with { CurrentId = current, UnlockedIds = normalized });
    }

    private static string DisplayName(Costume costume)
    {
        var name = string.IsNullOrWhiteSpace(costume.CostumeName)
            ? $"#{costume.CostumeId:D3}"
            : costume.CostumeName;
        return costume.CostumeType == "unknown"
            ? $"{costume.CostumeId} - {name} (Unsorted)"
            : $"{costume.CostumeId} - {name}";
    }
}
```

- [ ] **Step 2: Create title picker**

Create `TaikoWebUI/Shared/Customize/TitlePicker.razor`:

```razor
@if (Catalog.Count > 0)
{
    <MudStack Spacing="3">
        <MudTextField Value="Value.Title"
                      ValueChanged="SetTitle"
                      Label="@Localizer["Title"]"
                      ReadOnly="@ReadOnly" />

        <MudSelect T="uint"
                   Value="Value.TitlePlateId"
                   ValueChanged="SetPlate"
                   Label="@Localizer["Title Plate"]"
                   Disabled="@ReadOnly"
                   AnchorOrigin="Origin.BottomCenter">
            @foreach (var plate in PlateIds)
            {
                <MudSelectItem Value="@plate">@plate</MudSelectItem>
            }
        </MudSelect>

        @if (!ReadOnly)
        {
            <MudExpansionPanels Elevation="0" Outlined="true">
                <MudExpansionPanel Text="@Localizer["Player Titles"]">
                    <MudStack Spacing="1">
                        @foreach (var title in OrderedTitles)
                        {
                            var isUnlocked = Value.UnlockedTitleIds.Contains(title.TitleId);
                            <MudCheckBox T="bool"
                                         Value="@isUnlocked"
                                         ValueChanged="@(value => ToggleUnlocked(title.TitleId, value))"
                                         Label="@DisplayName(title)" />
                        }
                    </MudStack>
                </MudExpansionPanel>
            </MudExpansionPanels>
        }
    </MudStack>
}

@code {
    [Parameter] public string Era { get; set; } = string.Empty;
    [Parameter] public IReadOnlyDictionary<uint, Title> Catalog { get; set; } = new Dictionary<uint, Title>();
    [Parameter] public TitlePickerValue Value { get; set; } = new(string.Empty, 0, []);
    [Parameter] public EventCallback<TitlePickerValue> ValueChanged { get; set; }
    [Parameter] public bool ReadOnly { get; set; }

    private IReadOnlyList<Title> OrderedTitles => Catalog.Values.OrderBy(title => title.TitleId).ToArray();

    private IReadOnlyList<uint> PlateIds => Catalog.Values
        .Select(title => title.TitleRarity)
        .Append(Value.TitlePlateId)
        .Distinct()
        .OrderBy(id => id)
        .ToArray();

    private Task SetTitle(string title)
        => ValueChanged.InvokeAsync(Value with { Title = title });

    private Task SetPlate(uint plate)
        => ValueChanged.InvokeAsync(Value with { TitlePlateId = plate });

    private Task ToggleUnlocked(uint id, bool enabled)
    {
        var ids = enabled
            ? Value.UnlockedTitleIds.Append(id)
            : Value.UnlockedTitleIds.Where(value => value != id);
        return ValueChanged.InvokeAsync(Value with { UnlockedTitleIds = ids.Distinct().OrderBy(value => value).ToArray() });
    }

    private static string DisplayName(Title title)
    {
        var name = string.IsNullOrWhiteSpace(title.TitleName)
            ? $"#{title.TitleId:D3}"
            : title.TitleName;
        return $"{title.TitleId} - {name}";
    }
}
```

- [ ] **Step 3: Create tone picker**

Create `TaikoWebUI/Shared/Customize/NeiroPicker.razor`:

```razor
@if (Catalog.Count > 0)
{
    <MudStack Spacing="2">
        <MudSelect T="uint"
                   Value="Value.CurrentId"
                   ValueChanged="SetCurrent"
                   Label="@Localizer["Tone"]"
                   Disabled="@ReadOnly"
                   AnchorOrigin="Origin.BottomCenter">
            @foreach (var neiro in OrderedNeiros)
            {
                <MudSelectItem Value="@neiro.NeiroId">@DisplayName(neiro)</MudSelectItem>
            }
        </MudSelect>

        @if (!ReadOnly)
        {
            <MudExpansionPanels Elevation="0" Outlined="true">
                <MudExpansionPanel Text="@Localizer["Unlocked"]">
                    <MudStack Spacing="1">
                        @foreach (var neiro in OrderedNeiros)
                        {
                            var isUnlocked = Value.UnlockedIds.Contains(neiro.NeiroId);
                            <MudCheckBox T="bool"
                                         Value="@isUnlocked"
                                         ValueChanged="@(enabled => ToggleUnlocked(neiro.NeiroId, enabled))"
                                         Label="@DisplayName(neiro)" />
                        }
                    </MudStack>
                </MudExpansionPanel>
            </MudExpansionPanels>
        }
    </MudStack>
}

@code {
    [Parameter] public string Era { get; set; } = string.Empty;
    [Parameter] public IReadOnlyDictionary<uint, Neiro> Catalog { get; set; } = new Dictionary<uint, Neiro>();
    [Parameter] public NeiroPickerValue Value { get; set; } = new(0, []);
    [Parameter] public EventCallback<NeiroPickerValue> ValueChanged { get; set; }
    [Parameter] public bool ReadOnly { get; set; }

    private IReadOnlyList<Neiro> OrderedNeiros => Catalog.Values.OrderBy(neiro => neiro.NeiroId).ToArray();

    private Task SetCurrent(uint id)
    {
        var unlocked = Value.UnlockedIds.Contains(id)
            ? Value.UnlockedIds
            : Value.UnlockedIds.Append(id).Distinct().OrderBy(value => value).ToArray();
        return ValueChanged.InvokeAsync(Value with { CurrentId = id, UnlockedIds = unlocked });
    }

    private Task ToggleUnlocked(uint id, bool enabled)
    {
        var ids = enabled
            ? Value.UnlockedIds.Append(id)
            : Value.UnlockedIds.Where(value => value != id);
        return ValueChanged.InvokeAsync(Value with { UnlockedIds = ids.Append(0).Distinct().OrderBy(value => value).ToArray() });
    }

    private static string DisplayName(Neiro neiro)
    {
        var name = string.IsNullOrWhiteSpace(neiro.NeiroName)
            ? $"#{neiro.NeiroId:D3}"
            : neiro.NeiroName;
        return $"{neiro.NeiroId} - {name}";
    }
}
```

- [ ] **Step 4: Create color picker**

Create `TaikoWebUI/Shared/Customize/ColorPicker.razor`:

```razor
<MudStack Row="true">
    <MudSelect T="uint" Value="Value.BodyColor" ValueChanged="SetBody" Label="@Localizer["Body Color"]" AnchorOrigin="Origin.BottomCenter">
        @foreach (var option in ColorOptions)
        {
            <MudSelectItem Value="@option.Id">
                <div class="color-box" style="@($"background: {option.Hex}")"></div>
                @option.Id
            </MudSelectItem>
        }
    </MudSelect>
    <MudSelect T="uint" Value="Value.FaceColor" ValueChanged="SetFace" Label="@Localizer["Face Color"]" AnchorOrigin="Origin.BottomCenter">
        @foreach (var option in ColorOptions)
        {
            <MudSelectItem Value="@option.Id">
                <div class="color-box" style="@($"background: {option.Hex}")"></div>
                @option.Id
            </MudSelectItem>
        }
    </MudSelect>
    <MudSelect T="uint" Value="Value.LimbColor" ValueChanged="SetLimb" Label="@Localizer["Limb Color"]" AnchorOrigin="Origin.BottomCenter">
        @foreach (var option in ColorOptions)
        {
            <MudSelectItem Value="@option.Id">
                <div class="color-box" style="@($"background: {option.Hex}")"></div>
                @option.Id
            </MudSelectItem>
        }
    </MudSelect>
</MudStack>

@code {
    [Parameter] public IReadOnlyList<string> Colors { get; set; } = [];
    [Parameter] public ColorPickerValue Value { get; set; } = new(1, 0, 3);
    [Parameter] public EventCallback<ColorPickerValue> ValueChanged { get; set; }

    private IEnumerable<(uint Id, string Hex)> ColorOptions
        => Colors.Select((hex, index) => ((uint)index, hex));

    private Task SetBody(uint id) => ValueChanged.InvokeAsync(Value with { BodyColor = id });

    private Task SetFace(uint id) => ValueChanged.InvokeAsync(Value with { FaceColor = id });

    private Task SetLimb(uint id) => ValueChanged.InvokeAsync(Value with { LimbColor = id });
}
```

## Task 3: Era-Aware GameDataService

- [ ] **Step 1: Extend `IGameDataService`**

Modify `TaikoWebUI/Services/IGameDataService.cs` by adding era-aware methods:

```csharp
public Task<IReadOnlyList<Costume>> GetCostumeList(string? era);

public Task<IReadOnlyDictionary<uint, Title>> GetTitleDictionary(string? era);

public Task<IReadOnlyDictionary<uint, Neiro>> GetNeiroDictionary(string? era);
```

Keep the existing no-era methods for compatibility.

- [ ] **Step 2: Update `GameDataService` caches**

Modify `TaikoWebUI/Services/GameDataService.cs` fields. Remove the old single-era fields `costumeList`, `titleDictionary`, `costumesInitialized`, and `titlesInitialized`; these era-keyed dictionaries replace them:

```csharp
private readonly Dictionary<string, IReadOnlyList<Costume>> costumeLists = new();
private readonly Dictionary<string, IReadOnlyDictionary<uint, Title>> titleDictionaries = new();
private readonly Dictionary<string, IReadOnlyDictionary<uint, Neiro>> neiroDictionaries = new();
```

Add these methods:

```csharp
public async Task<IReadOnlyList<Costume>> GetCostumeList(string? era)
{
    var normalized = WebUiEra.Normalize(era);
    if (!costumeLists.TryGetValue(normalized, out var value))
    {
        value = await client.GetFromJsonAsync<List<Costume>>(WebUiEra.Api(normalized, "customization/costumes"))
                ?? new List<Costume>();
        costumeLists[normalized] = value;
    }

    return value;
}

public async Task<List<Costume>> GetCostumeList()
    => (await GetCostumeList(WebUiEra.Default)).ToList();

public async Task<IReadOnlyDictionary<uint, Title>> GetTitleDictionary(string? era)
{
    var normalized = WebUiEra.Normalize(era);
    if (!titleDictionaries.TryGetValue(normalized, out var value))
    {
        value = await client.GetFromJsonAsync<Dictionary<uint, Title>>(WebUiEra.Api(normalized, "customization/titles"))
                ?? new Dictionary<uint, Title>();
        titleDictionaries[normalized] = value;
    }

    return value;
}

public async Task<Dictionary<uint, Title>> GetTitleDictionary()
    => (await GetTitleDictionary(WebUiEra.Default)).ToDictionary(pair => pair.Key, pair => pair.Value);

public async Task<IReadOnlyDictionary<uint, Neiro>> GetNeiroDictionary(string? era)
{
    var normalized = WebUiEra.Normalize(era);
    if (!neiroDictionaries.TryGetValue(normalized, out var value))
    {
        value = await client.GetFromJsonAsync<Dictionary<uint, Neiro>>(WebUiEra.Api(normalized, "customization/neiros"))
                ?? new Dictionary<uint, Neiro>();
        neiroDictionaries[normalized] = value;
    }

    return value;
}
```

Keep `GetLockedCostumeDataDictionary()` and `GetLockedTitleDataDictionary()` unchanged for legacy callers.

Replace `InitializeCostumesAsync()` and `InitializeTitlesAsync()` with legacy locked-data-only loaders so the old lock endpoints still initialize once:

```csharp
private bool lockedCostumesInitialized;
private bool lockedTitlesInitialized;

private async Task InitializeLockedCostumesAsync()
{
    lockedCostumeDataDictionary = await client.GetFromJsonAsync<Dictionary<string, List<uint>>>("api/GameData/LockedCostumes")
                                  ?? new Dictionary<string, List<uint>>();
    lockedCostumesInitialized = true;
}

private async Task InitializeLockedTitlesAsync()
{
    lockedTitleDataDictionary = await client.GetFromJsonAsync<Dictionary<string, List<uint>>>("api/GameData/LockedTitles")
                                ?? new Dictionary<string, List<uint>>();
    lockedTitlesInitialized = true;
}
```

Update the two locked-data methods to use those flags:

```csharp
public async Task<Dictionary<string, List<uint>>> GetLockedCostumeDataDictionary()
{
    if (!lockedCostumesInitialized)
    {
        await InitializeLockedCostumesAsync();
    }

    return lockedCostumeDataDictionary ?? new Dictionary<string, List<uint>>();
}

public async Task<Dictionary<string, List<uint>>> GetLockedTitleDataDictionary()
{
    if (!lockedTitlesInitialized)
    {
        await InitializeLockedTitlesAsync();
    }

    return lockedTitleDataDictionary ?? new Dictionary<string, List<uint>>();
}
```

## Task 4: Profile Page Composition

- [ ] **Step 1: Add profile fields**

Modify `TaikoWebUI/Pages/Profile.razor.cs` and add these fields:

```csharp
private IReadOnlyDictionary<uint, Neiro> neiroDictionary = new Dictionary<uint, Neiro>();
private List<Costume> kigurumiCatalog = new();
private List<Costume> headCatalog = new();
private List<Costume> bodyCatalog = new();
private List<Costume> faceCatalog = new();
private List<Costume> puchiCatalog = new();

private CostumePickerValue kigurumiValue = new(0, []);
private CostumePickerValue headValue = new(0, []);
private CostumePickerValue bodyValue = new(0, []);
private CostumePickerValue faceValue = new(0, []);
private CostumePickerValue puchiValue = new(0, []);
private TitlePickerValue titleValue = new(string.Empty, 0, []);
private NeiroPickerValue neiroValue = new(0, []);
private ColorPickerValue colorValue = new(1, 0, 3);
```

- [ ] **Step 2: Use era-aware settings and catalogs**

In `OnInitializedAsync`, replace the legacy settings call with:

```csharp
response = await Client.GetFromJsonAsync<UserSetting>(WebUiEra.Api(CurrentEra, $"UserSettings/{Baid}"));
response.ThrowIfNull();
```

Replace customization catalog loading with:

```csharp
costumeList = (await GameDataService.GetCostumeList(CurrentEra)).ToList();
titleDictionary = (await GameDataService.GetTitleDictionary(CurrentEra)).ToDictionary(pair => pair.Key, pair => pair.Value);
neiroDictionary = await GameDataService.GetNeiroDictionary(CurrentEra);
lockedCostumeDataDictionary = IsGreen ? new Dictionary<string, List<uint>>() : await GameDataService.GetLockedCostumeDataDictionary();
lockedTitleDataDictionary = IsGreen ? new Dictionary<string, List<uint>>() : await GameDataService.GetLockedTitleDataDictionary();
InitializeCustomizationValues();
```

Add this method:

```csharp
private void InitializeCustomizationValues()
{
    response.ThrowIfNull();
    kigurumiCatalog = BuildCostumeCatalog("kigurumi");
    headCatalog = BuildCostumeCatalog("head");
    bodyCatalog = BuildCostumeCatalog("body");
    faceCatalog = BuildCostumeCatalog("face");
    puchiCatalog = BuildCostumeCatalog("puchi");
    kigurumiValue = new CostumePickerValue(response.Kigurumi, response.UnlockedKigurumi);
    headValue = new CostumePickerValue(response.Head, response.UnlockedHead);
    bodyValue = new CostumePickerValue(response.Body, response.UnlockedBody);
    faceValue = new CostumePickerValue(response.Face, response.UnlockedFace);
    puchiValue = new CostumePickerValue(response.Puchi, response.UnlockedPuchi);
    titleValue = new TitlePickerValue(response.Title, response.TitlePlateId, response.UnlockedTitle);
    neiroValue = new NeiroPickerValue(response.ToneId, response.UnlockedTone);
    colorValue = new ColorPickerValue(response.BodyColor, response.FaceColor, response.LimbColor);
}
```

Add this method:

```csharp
private List<Costume> BuildCostumeCatalog(string costumeType)
{
    return costumeList
        .Where(costume => costume.CostumeType == costumeType || costume.CostumeType == "unknown")
        .OrderBy(costume => costume.CostumeType == "unknown" ? 1 : 0)
        .ThenBy(costume => costume.CostumeId)
        .ToList();
}
```

Add this method:

```csharp
private void ApplyCustomizationValues()
{
    response.ThrowIfNull();
    response.Kigurumi = kigurumiValue.CurrentId;
    response.UnlockedKigurumi = kigurumiValue.UnlockedIds.ToList();
    response.Head = headValue.CurrentId;
    response.UnlockedHead = headValue.UnlockedIds.ToList();
    response.Body = bodyValue.CurrentId;
    response.UnlockedBody = bodyValue.UnlockedIds.ToList();
    response.Face = faceValue.CurrentId;
    response.UnlockedFace = faceValue.UnlockedIds.ToList();
    response.Puchi = puchiValue.CurrentId;
    response.UnlockedPuchi = puchiValue.UnlockedIds.ToList();
    response.Title = titleValue.Title;
    response.TitlePlateId = titleValue.TitlePlateId;
    response.UnlockedTitle = titleValue.UnlockedTitleIds.ToList();
    response.ToneId = neiroValue.CurrentId;
    response.UnlockedTone = neiroValue.UnlockedIds.ToList();
    response.BodyColor = colorValue.BodyColor;
    response.FaceColor = colorValue.FaceColor;
    response.LimbColor = colorValue.LimbColor;
}
```

In `SaveOptions`, call `ApplyCustomizationValues()` before posting and replace the POST URL with:

```csharp
await Client.PostAsJsonAsync(WebUiEra.Api(CurrentEra, $"UserSettings/{Baid}"), response);
```

- [ ] **Step 3: Replace customization tab in `Profile.razor`**

Replace the Nijiiro-only costume tab and inactive Green block with:

```razor
@if (costumeList.Count > 0 || titleDictionary.Count > 0 || neiroDictionary.Count > 0)
{
    <MudTabPanel Text="@Localizer["Costume"]">
        <MudStack Spacing="4">
            <MudText Typo="Typo.h6">@Localizer["Costume Options"]</MudText>

            <CostumePicker Era="@CurrentEra"
                           Label="@Localizer["Kigurumi"]"
                           Catalog="@kigurumiCatalog"
                           Value="@kigurumiValue"
                           ValueChanged="@(value => kigurumiValue = value)"
                           ReadOnly="@(!AuthService.AllowFreeProfileEditing)" />

            <CostumePicker Era="@CurrentEra"
                           Label="@Localizer["Head"]"
                           Catalog="@headCatalog"
                           Value="@headValue"
                           ValueChanged="@(value => headValue = value)"
                           ReadOnly="@(!AuthService.AllowFreeProfileEditing)" />

            <CostumePicker Era="@CurrentEra"
                           Label="@Localizer["Body"]"
                           Catalog="@bodyCatalog"
                           Value="@bodyValue"
                           ValueChanged="@(value => bodyValue = value)"
                           ReadOnly="@(!AuthService.AllowFreeProfileEditing)" />

            <CostumePicker Era="@CurrentEra"
                           Label="@Localizer["Face"]"
                           Catalog="@faceCatalog"
                           Value="@faceValue"
                           ValueChanged="@(value => faceValue = value)"
                           ReadOnly="@(!AuthService.AllowFreeProfileEditing)" />

            <CostumePicker Era="@CurrentEra"
                           Label="@Localizer["Puchi"]"
                           Catalog="@puchiCatalog"
                           Value="@puchiValue"
                           ValueChanged="@(value => puchiValue = value)"
                           ReadOnly="@(!AuthService.AllowFreeProfileEditing)" />

            <TitlePicker Era="@CurrentEra"
                         Catalog="@titleDictionary"
                         Value="@titleValue"
                         ValueChanged="@(value => titleValue = value)"
                         ReadOnly="@(!AuthService.AllowFreeProfileEditing)" />

            <NeiroPicker Era="@CurrentEra"
                         Catalog="@neiroDictionary"
                         Value="@neiroValue"
                         ValueChanged="@(value => neiroValue = value)"
                         ReadOnly="@(!AuthService.AllowFreeProfileEditing)" />

            <ColorPicker Colors="@CostumeColors"
                         Value="@colorValue"
                         ValueChanged="@(value => colorValue = value)" />
        </MudStack>
    </MudTabPanel>
}
```

Remove the duplicated inactive Green comments around the tabs.

- [ ] **Step 4: Keep existing player preview values in sync**

In each picker callback in `Profile.razor`, update `response` after assigning the local value:

```razor
ValueChanged="@(value => { kigurumiValue = value; ApplyCustomizationValues(); })"
```

Apply the same callback form to `headValue`, `bodyValue`, `faceValue`, `puchiValue`, `titleValue`, `neiroValue`, and `colorValue`.

- [ ] **Step 5: Remove duplicated legacy title and tone controls**

In the `Profile` tab, delete the legacy title grid that starts with:

```razor
<MudGrid>
    <MudItem xs="12" md="8">
        @if (AuthService.AllowFreeProfileEditing)
```

and ends after this title-plate branch:

```razor
    @if (AuthService.AllowFreeProfileEditing)
    {
        <MudItem xs="12" md="4">
            <MudSelect @bind-Value="@response.TitlePlateId" Label=@Localizer["Title Plate"] AnchorOrigin="Origin.BottomCenter">
                @foreach (var index in titlePlateIdList)
                {
                    <MudSelectItem Value="@index">@TitlePlateStrings[index]</MudSelectItem>
                }
            </MudSelect>
        </MudItem>
    }
</MudGrid>
```

In the `Song Options` tab, delete the legacy tone select:

```razor
<MudSelect @bind-Value="@response.ToneId" Label=@Localizer["Tone"] AnchorOrigin="Origin.BottomCenter">
    @for (uint i = 0; i < ToneStrings.Length; i++)
    {
        var index = i;
        <MudSelectItem Value="@i">@Localizer[ToneStrings[index]]</MudSelectItem>
    }
</MudSelect>
```

The new `TitlePicker` and `NeiroPicker` now own title and tone editing for both Nijiiro and Green. After deleting the title button, remove the now-unused `OpenChooseTitleDialog()` method, the `unlockedTitles` field, and the `using TaikoWebUI.Pages.Dialogs;` import.

## Task 5: Build Verification

- [ ] **Step 1: Build WebUI**

Run:

```powershell
dotnet build TaikoWebUI/TaikoWebUI.csproj
```

Expected: PASS.

- [ ] **Step 2: Build solution**

Run:

```powershell
dotnet build
```

Expected: PASS.

- [ ] **Step 3: Commit**

```powershell
git add -- TaikoWebUI/Shared/Customize TaikoWebUI/_Imports.razor TaikoWebUI/Services/IGameDataService.cs TaikoWebUI/Services/GameDataService.cs TaikoWebUI/Pages/Profile.razor TaikoWebUI/Pages/Profile.razor.cs
git commit -m "Add era-aware WebUI customization controls"
```
