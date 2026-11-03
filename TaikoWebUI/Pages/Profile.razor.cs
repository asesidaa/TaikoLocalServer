using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.JSInterop;

namespace TaikoWebUI.Pages;

public partial class Profile : IDisposable
{
    [Parameter] public int Baid { get; set; }

    private enum Section { Profile, Costume, SongOptions, Card }

    private UserSetting? setting;
    private List<DanBestData> dans = [];
    private Section section = Section.Profile;
    private bool titlesOpen;

    private List<Costume> costumeList = [];
    private List<Title> unlockedTitles = [];
    private List<uint> kigurumiIds = [], headIds = [], bodyIds = [], faceIds = [], puchiIds = [], titlePlateIds = [];

    // Autosave: every change saves after a short debounce; the status shows in the top bar (PageState.Save).
    // Only the fields changed since the last save are sent (PATCH), so two open pages don't undo each other's edits.
    private CancellationTokenSource? saveDelay;
    private JsonObject? saved;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private string Culture => CultureInfo.CurrentCulture.ToString();

    protected override async Task OnInitializedAsync()
    {
        setting = await Client.GetFromJsonAsync<UserSetting>($"api/UserSettings/{Baid}");
        setting.ThrowIfNull();
        saved = Snapshot();
        Page.Set(Localizer["Profile"]);
        Page.SetSave(PageState.SaveStatus.Idle);

        costumeList = await GameDataService.GetCostumeList();
        var titles = await GameDataService.GetTitleDictionary();
        var lockedCostumes = await GameDataService.GetLockedCostumeDataDictionary();
        var lockedTitles = await GameDataService.GetLockedTitleDataDictionary();
        InitializeCostumes(lockedCostumes);
        InitializeTitles(titles, lockedTitles);

        dans = (await Client.GetFromJsonAsync<DanBestDataResponse>($"api/DanBestData/{Baid}"))?.DanBestDataList ?? [];
    }

    // Same unlock rules as before: free editing = everything except locked items (plus anything unlocked);
    // otherwise only what the player has unlocked.
    private void InitializeCostumes(Dictionary<string, List<uint>> locked)
    {
        List<uint> Ids(string type, List<uint> unlocked)
        {
            var all = costumeList.Where(c => c.CostumeType == type).Select(c => c.CostumeId);
            var ids = AuthService.AllowFreeProfileEditing
                ? all.Concat(unlocked).Except(locked.GetValueOrDefault(type) ?? [])
                : all.Intersect(unlocked);
            return ids.Distinct().Order().ToList();
        }

        var s = setting!;
        kigurumiIds = Ids("kigurumi", s.UnlockedKigurumi);
        headIds = Ids("head", s.UnlockedHead);
        bodyIds = Ids("body", s.UnlockedBody);
        faceIds = Ids("face", s.UnlockedFace);
        puchiIds = Ids("puchi", s.UnlockedPuchi);
        // Keep the current value selectable even if it isn't in the list (e.g. an id the catalog doesn't name).
        foreach (var (ids, v) in new[] { (kigurumiIds, s.Kigurumi), (headIds, s.Head), (bodyIds, s.Body), (faceIds, s.Face), (puchiIds, s.Puchi) })
            if (!ids.Contains(v)) { ids.Add(v); ids.Sort(); }
    }

    private void InitializeTitles(Dictionary<uint, Title> titles, Dictionary<string, List<uint>> locked)
    {
        var lockedPlates = locked.GetValueOrDefault("titlePlate") ?? [];
        titlePlateIds = titles.Values.Select(t => t.TitleRarity).Where(id => id < GameData.TitlePlates.Length)
            .Except(lockedPlates).Distinct().Order().ToList();
        if (!titlePlateIds.Contains(setting!.TitlePlateId)) { titlePlateIds.Add(setting.TitlePlateId); titlePlateIds.Sort(); }

        var unlocked = setting.UnlockedTitle;
        IEnumerable<uint> ids;
        if (AuthService.AllowFreeProfileEditing)
        {
            var lockedIds = (locked.GetValueOrDefault("title") ?? []).Except(unlocked)
                .Concat(titles.Values.Where(t => lockedPlates.Contains(t.TitleRarity)).Select(t => t.TitleId));
            ids = titles.Keys.Except(lockedIds);
        }
        else
        {
            ids = titles.Keys.Intersect(unlocked);
        }
        var idSet = ids.ToHashSet();
        unlockedTitles = titles.Values.Where(t => idSet.Contains(t.TitleId)).ToList();
    }

    private void Changed()
    {
        Page.SetSave(PageState.SaveStatus.Saving);
        saveDelay?.Cancel();
        saveDelay = new CancellationTokenSource();
        _ = SaveSoon(saveDelay.Token);
    }

    private async Task SaveSoon(CancellationToken token)
    {
        try { await Task.Delay(700, token); }
        catch (TaskCanceledException) { return; }
        if (!await SaveChanges())
        {
            Page.SetSave(PageState.SaveStatus.Idle);
            Toast.Show(Localizer["Couldn't save your changes."], isError: true);
        }
        else
        {
            Page.SetSave(PageState.SaveStatus.Saved);
        }
        await InvokeAsync(StateHasChanged);
    }

    private JsonObject Snapshot() => JsonSerializer.SerializeToNode(setting, Json)!.AsObject();

    /// <summary>Sends the fields that differ from the last saved state; true when there was nothing to send.</summary>
    private async Task<bool> SaveChanges()
    {
        var now = Snapshot();
        var changes = new JsonObject();
        foreach (var (key, value) in now)
        {
            if (saved is null || !JsonNode.DeepEquals(value, saved[key]))
                changes[key] = value?.DeepClone();
        }
        if (changes.Count == 0) return true;
        var response = await Client.PatchAsJsonAsync($"api/UserSettings/{Baid}", changes, CancellationToken.None);
        if (response.IsSuccessStatusCode) saved = now;
        return response.IsSuccessStatusCode;
    }

    private void Set(Action<UserSetting> change)
    {
        change(setting!);
        Changed();
    }

    private void PickTitle((string Title, Title? Picked) pick)
    {
        titlesOpen = false;
        Set(s =>
        {
            s.Title = pick.Title;
            if (pick.Picked is { } picked) s.TitlePlateId = picked.TitleRarity;
        });
    }

    // Song option icons come from api/Assets (data-asset), which needs the token.
    protected override Task OnAfterRenderAsync(bool firstRender) => Js.InvokeVoidAsync("ui.hydrateAssets").AsTask();

    private string CostumeName(string type, uint id) => type switch
    {
        "kigurumi" when id == 0 => Localizer["None (use head & body)"],
        _ => GameDataService.GetCostumeName(costumeList, type, id, Culture),
    } is { Length: > 0 } name ? name : $"#{id}";

    // Leaving mid-debounce: save now rather than drop the last edit.
    public void Dispose()
    {
        if (saveDelay is null || saveDelay.IsCancellationRequested || Page.Save != PageState.SaveStatus.Saving) return;
        saveDelay.Cancel();
        _ = SaveChanges();
    }
}
