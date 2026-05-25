using TaikoWebUI.Utilities;
using System.Collections.Generic;
using TaikoWebUI.Shared.Customize;

namespace TaikoWebUI.Pages;

public partial class Profile
{
    [Parameter]
    public int Baid { get; set; }

    [Parameter]
    public string? Era { get; set; }

    private string CurrentEra => WebUiEra.Normalize(Era);
    private bool IsGreen => WebUiEra.IsGreen(CurrentEra);
    private bool CanEditUnlocks => IsGreen && AuthService.AllowFreeProfileEditing;
    private TitleSelectionMode CurrentTitleSelectionMode => IsGreen
        ? TitleSelectionMode.TitleId
        : TitleSelectionMode.TitlePlate;

    private SongBestResponse? songresponse;

    private UserSetting? response;

    private bool isSavingOptions;

    private static readonly string[] SpeedStrings =
    {
        "1.0", "1.1", "1.2", "1.3", "1.4",
        "1.5", "1.6", "1.7", "1.8", "1.9",
        "2.0", "2.5", "3.0", "3.5", "4.0"
    };

    private static readonly string[] NotePositionStrings = { "-5", "-4", "-3", "-2", "-1", "0", "+1", "+2", "+3", "+4", "+5" };

    private static readonly string[] LanguageStrings =
    {
        "Japanese", "English", "Chinese Traditional", "Korean", "Chinese Simplified"
    };

    private static readonly string[] DifficultySettingCourseStrings =
    {
        "None", "Set Up Each Time",
        "Easy", "Normal", "Hard", "Oni", "Ura Oni"
    };

    private static readonly string[] GreenLocalRankingDifficultyStrings =
    {
        "No Fixed Course", "Easy", "Normal", "Hard", "Oni"
    };

    private static readonly string[] DifficultySettingStarStrings =
    {
        "None", "Set Up Each Time",
        "1 Star", "2 Star", "3 Star", "4 Star", "5 Star", "6 Star", "7 Star", "8 Star", "9 Star", "10 Star"
    };

    private static readonly string[] DifficultySettingSortStrings =
    {
        "None", "Set Up Each Time", "Default",
        "Not Cleared", "Not Full Combo", "Not Donderful Combo"
    };

    private Dictionary<Difficulty, List<SongBestData>> songBestDataMap = new();

    private Difficulty highestDifficulty = Difficulty.Easy;
    private IReadOnlyDictionary<uint, DanData> danDictionary = new Dictionary<uint, DanData>();
    
    private List<Costume> costumeList = new();
    private Dictionary<uint, Title> titleDictionary = new();
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

    private int[] scoresArray = new int[10];
    
    private Dictionary<uint, MusicDetail> musicDetailDictionary = new();

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        isSavingOptions = false;
        response = await Client.GetFromJsonAsync<UserSetting>(WebUiEra.Api(CurrentEra, $"UserSettings/{Baid}"));
        response.ThrowIfNull();
        
        musicDetailDictionary = await GameDataService.GetMusicDetailDictionary(CurrentEra);
        danDictionary = GameDataService.GetDanMap(CurrentEra);

        BreadcrumbsStateContainer.breadcrumbs.Clear();
        if (AuthService.IsLoggedIn && !AuthService.IsAdmin)
        {
            BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem(Localizer["Dashboard"], href: "/"));
        }
        else
        {
            BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem(Localizer["Users"], href: "/Users"));
        }
        BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem($"{response.MyDonName}", href: null, disabled: true));
        BreadcrumbsStateContainer.breadcrumbs.Add(new BreadcrumbItem(Localizer["Profile"], href: WebUiEra.UserRoute(Baid, CurrentEra, "Profile"), disabled: false));
        BreadcrumbsStateContainer.NotifyStateChanged();

        costumeList = (await GameDataService.GetCostumeList(CurrentEra)).ToList();
        titleDictionary = (await GameDataService.GetTitleDictionary(CurrentEra)).ToDictionary(pair => pair.Key, pair => pair.Value);
        neiroDictionary = await GameDataService.GetNeiroDictionary(CurrentEra);
        InitializeCustomizationValues();

        songresponse = await Client.GetFromJsonAsync<SongBestResponse>(WebUiEra.Api(CurrentEra, $"PlayData/{Baid}"));
        songresponse.ThrowIfNull();

        songresponse.SongBestData.ForEach(data =>
        {
            var songId = data.SongId;
            data.Genre = GameDataService.GetMusicGenreBySongId(musicDetailDictionary, songId);
            data.MusicName = GameDataService.GetMusicNameBySongId(musicDetailDictionary, songId);
            data.MusicArtist = GameDataService.GetMusicArtistBySongId(musicDetailDictionary, songId);
        });

        songBestDataMap = songresponse.SongBestData.GroupBy(data => data.Difficulty)
            .ToDictionary(data => data.Key,
                          data => data.ToList());
        foreach (var songBestDataList in songBestDataMap.Values)
        {
            songBestDataList.Sort((data1, data2) => GameDataService.GetMusicIndexBySongId(musicDetailDictionary, data1.SongId)
                                      .CompareTo(GameDataService.GetMusicIndexBySongId(musicDetailDictionary, data2.SongId)));
        }

        for (var i = 0; i <= (int)Difficulty.UraOni; i++)
            if (songBestDataMap.ContainsKey((Difficulty)i) && songBestDataMap[(Difficulty)i].Count > 0)
            {
                highestDifficulty = (Difficulty)i;
            }
        
        if (response != null) UpdateScores(response.AchievementDisplayDifficulty);
    }

    private void InitializeCustomizationValues()
    {
        response.ThrowIfNull();
        kigurumiCatalog = BuildCostumeCatalog("kigurumi", response.Kigurumi, response.UnlockedKigurumi);
        headCatalog = BuildCostumeCatalog("head", response.Head, response.UnlockedHead);
        bodyCatalog = BuildCostumeCatalog("body", response.Body, response.UnlockedBody);
        faceCatalog = BuildCostumeCatalog("face", response.Face, response.UnlockedFace);
        puchiCatalog = BuildCostumeCatalog("puchi", response.Puchi, response.UnlockedPuchi);
        titleDictionary = BuildTitleCatalog();
        neiroDictionary = BuildNeiroCatalog(response.ToneId, response.UnlockedTone);
        kigurumiValue = new CostumePickerValue(response.Kigurumi, response.UnlockedKigurumi);
        headValue = new CostumePickerValue(response.Head, response.UnlockedHead);
        bodyValue = new CostumePickerValue(response.Body, response.UnlockedBody);
        faceValue = new CostumePickerValue(response.Face, response.UnlockedFace);
        puchiValue = new CostumePickerValue(response.Puchi, response.UnlockedPuchi);
        var titleText = IsGreen && string.IsNullOrWhiteSpace(response.Title)
            ? TitlePickerCatalog.ResolveSelectedTitleText(titleDictionary, response.TitlePlateId, response.Title)
            : response.Title;
        if (IsGreen)
        {
            response.Title = titleText;
        }

        titleValue = new TitlePickerValue(titleText, response.TitlePlateId, response.UnlockedTitle);
        neiroValue = new NeiroPickerValue(response.ToneId, response.UnlockedTone);
        colorValue = new ColorPickerValue(response.BodyColor, response.FaceColor, response.LimbColor);
    }

    private List<Costume> BuildCostumeCatalog(string costumeType, uint currentId, IReadOnlyCollection<uint> unlockedIds)
    {
        var catalogById = costumeList
            .Where(costume => costume.CostumeType == costumeType || costume.CostumeType == "unknown")
            .GroupBy(costume => costume.CostumeId)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(costume => costume.CostumeType == "unknown" ? 1 : 0).First());

        var ids = catalogById.Keys.Concat(unlockedIds).Append(currentId);

        return ids
            .Distinct()
            .OrderBy(id => id)
            .Select(id => catalogById.TryGetValue(id, out var costume)
                ? costume
                : new Costume { CostumeId = id, CostumeType = costumeType })
            .Where(costume => costume.CostumeType == costumeType || costume.CostumeType == "unknown")
            .OrderBy(costume => costume.CostumeType == "unknown" ? 1 : 0)
            .ThenBy(costume => costume.CostumeId)
            .ToList();
    }

    private Dictionary<uint, Title> BuildTitleCatalog()
    {
        response.ThrowIfNull();

        var titlesById = titleDictionary;
        var ids = titlesById.Keys
            .Concat(response.UnlockedTitle)
            .Append(response.TitlePlateId);

        return ids
            .Distinct()
            .OrderBy(id => id)
            .ToDictionary(
                id => id,
                id => titlesById.TryGetValue(id, out var title)
                    ? title
                    : new Title { TitleId = id });
    }

    private IReadOnlyDictionary<uint, Neiro> BuildNeiroCatalog(uint currentId, IReadOnlyCollection<uint> unlockedIds)
    {
        var neirosById = neiroDictionary;
        var ids = neirosById.Keys.Concat(unlockedIds).Append(currentId);

        return ids
            .Distinct()
            .OrderBy(id => id)
            .ToDictionary(
                id => id,
                id => neirosById.TryGetValue(id, out var neiro)
                    ? neiro
                    : new Neiro { NeiroId = id });
    }

    private void ApplyCustomizationValues()
    {
        response.ThrowIfNull();
        response.Kigurumi = kigurumiValue.CurrentId;
        response.Head = headValue.CurrentId;
        response.Body = bodyValue.CurrentId;
        response.Face = faceValue.CurrentId;
        response.Puchi = puchiValue.CurrentId;
        response.Title = titleValue.Title;
        response.TitlePlateId = titleValue.TitlePlateId;
        response.ToneId = neiroValue.CurrentId;
        response.BodyColor = colorValue.BodyColor;
        response.FaceColor = colorValue.FaceColor;
        response.LimbColor = colorValue.LimbColor;

        if (CanEditUnlocks)
        {
            response.UnlockedKigurumi = kigurumiValue.UnlockedIds.ToList();
            response.UnlockedHead = headValue.UnlockedIds.ToList();
            response.UnlockedBody = bodyValue.UnlockedIds.ToList();
            response.UnlockedFace = faceValue.UnlockedIds.ToList();
            response.UnlockedPuchi = puchiValue.UnlockedIds.ToList();
            response.UnlockedTitle = (IsGreen
                    ? titleValue.UnlockedTitleIds.Append(response.TitlePlateId)
                    : titleValue.UnlockedTitleIds)
                .Distinct()
                .OrderBy(id => id)
                .ToList();
            response.UnlockedTone = neiroValue.UnlockedIds.ToList();
        }
    }

    private async Task HandleTitleChanged(TitlePickerValue value)
    {
        titleValue = value;
        ApplyCustomizationValues();
        await UpdateTitle();
    }
    
    private async Task SaveOptions()
    {
        isSavingOptions = true;
        ApplyCustomizationValues();
        await Client.PostAsJsonAsync(WebUiEra.Api(CurrentEra, $"UserSettings/{Baid}"), response);
        isSavingOptions = false;

        // Adjust breadcrumb if name is changed
        if (response != null)
        {
            BreadcrumbsStateContainer.breadcrumbs[^2] = new BreadcrumbItem($"{response.MyDonName}", href: null, disabled: true);
        }
    }

    private void UpdateScores(Difficulty difficulty)
    {
        response.ThrowIfNull();
        response.AchievementDisplayDifficulty = difficulty;
        scoresArray = new int[10];

        if (difficulty == Difficulty.None) difficulty = highestDifficulty;

        if (!songBestDataMap.TryGetValue(difficulty, out var values))
        {
            if (difficulty == Difficulty.UraOni)
            {
                difficulty = Difficulty.Oni;
                if (!songBestDataMap.TryGetValue(difficulty, out values)) return;
            }
            else return;
        }

        var valuesList = new List<SongBestData>(values);

        if (difficulty == Difficulty.UraOni)
        {
            // Also include Oni scores
            if (songBestDataMap.TryGetValue(Difficulty.Oni, out var oniValues))
            {
                valuesList.AddRange(oniValues);
            }
        }

        foreach (var value in valuesList)
        {
            switch (value.BestScoreRank)
            {
                case ScoreRank.Dondaful:
                    scoresArray[0]++;
                    break;
                case ScoreRank.Gold:
                    scoresArray[1]++;
                    break;
                case ScoreRank.Sakura:
                    scoresArray[2]++;
                    break;
                case ScoreRank.Purple:
                    scoresArray[3]++;
                    break;
                case ScoreRank.White:
                    scoresArray[4]++;
                    break;
                case ScoreRank.Bronze:
                    scoresArray[5]++;
                    break;
                case ScoreRank.Silver:
                    scoresArray[6]++;
                    break;
            }

            switch (value.BestCrown)
            {
                case CrownType.Clear:
                    scoresArray[7]++;
                    break;
                case CrownType.Gold:
                    scoresArray[8]++;
                    break;
                case CrownType.Dondaful:
                    scoresArray[9]++;
                    break;
            }
        }
    }

}
