using System.Globalization;

namespace TaikoWebUI.Pages;

public partial class Profile : IDisposable
{
    [Parameter] public int Baid { get; set; }

    private enum Section { Profile, Costume, SongOptions }

    private UserSetting? setting;
    private List<DanBestData> dans = [];
    private Section section = Section.Profile;
    private bool titlesOpen;

    private List<Costume> costumeList = [];
    private List<Title> unlockedTitles = [];
    private List<uint> kigurumiIds = [], headIds = [], bodyIds = [], faceIds = [], puchiIds = [], titlePlateIds = [];

    // Autosave: every change saves after a short debounce; the status shows in the top bar (PageState.Save).
    private CancellationTokenSource? saveDelay;

    private string Culture => CultureInfo.CurrentCulture.ToString();

    protected override async Task OnInitializedAsync()
    {
        setting = await Client.GetFromJsonAsync<UserSetting>($"api/UserSettings/{Baid}");
        setting.ThrowIfNull();
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
        var response = await Client.PostAsJsonAsync($"api/UserSettings/{Baid}", setting, CancellationToken.None);
        if (!response.IsSuccessStatusCode)
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

    private void Set(Action<UserSetting> change)
    {
        change(setting!);
        Changed();
    }

    private void PickTitle((string Title, uint? Plate) pick)
    {
        titlesOpen = false;
        Set(s =>
        {
            s.Title = pick.Title;
            if (pick.Plate is { } plate) s.TitlePlateId = plate;
        });
    }

    private string CostumeName(string type, uint id) => type switch
    {
        "kigurumi" when id == 0 => Localizer["None (use head & body)"],
        "kigurumi" => GameDataService.GetKigurumiTitle(costumeList, id, Culture),
        "head" => GameDataService.GetHeadTitle(costumeList, id, Culture),
        "body" => GameDataService.GetBodyTitle(costumeList, id, Culture),
        "face" => GameDataService.GetFaceTitle(costumeList, id, Culture),
        _ => GameDataService.GetPuchiTitle(costumeList, id, Culture),
    } is { Length: > 0 } name ? name : $"#{id}";

    // Leaving mid-debounce: save now rather than drop the last edit.
    public void Dispose()
    {
        if (saveDelay is null || saveDelay.IsCancellationRequested || Page.Save != PageState.SaveStatus.Saving) return;
        saveDelay.Cancel();
        _ = Client.PostAsJsonAsync($"api/UserSettings/{Baid}", setting);
    }
}
