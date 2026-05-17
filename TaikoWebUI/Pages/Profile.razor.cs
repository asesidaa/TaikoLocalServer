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
    private bool IsGreen => string.Equals(CurrentEra, "Green", StringComparison.OrdinalIgnoreCase);
    private bool CanEditUnlocks => IsGreen && AuthService.AllowFreeProfileEditing;

    private SongBestResponse? songresponse;

    private UserSetting? response;

    private bool isSavingOptions;

    private static readonly string[] CostumeColors =
    {
        "#F84828", "#68C0C0", "#DC1500", "#F8F0E0", "#009687", "#00BF87",
        "#00FF9A", "#66FFC2", "#FFFFFF", "#690000", "#FF0000", "#FF6666",
        "#FFB3B3", "#00BCC2", "#00F7FF", "#66FAFF", "#B3FDFF", "#E4E4E4",
        "#993800", "#FF5E00", "#FF9E78", "#FFCFB3", "#005199", "#0088FF",
        "#66B8FF", "#B3DBFF", "#B9B9B9", "#B37700", "#FFAA00", "#FFCC66",
        "#FFE2B3", "#000C80", "#0019FF", "#6675FF", "#B3BAFF", "#858585",
        "#B39B00", "#FFDD00", "#FFFF00", "#FFFF71", "#2B0080", "#5500FF",
        "#9966FF", "#CCB3FF", "#505050", "#38A100", "#78C900", "#B3FF00",
        "#DCFF8A", "#610080", "#C400FF", "#DC66FF", "#EDB3FF", "#232323",
        "#006600", "#00B800", "#00FF00", "#8AFF9E", "#990059", "#FF0095",
        "#FF66BF", "#FFB3DF", "#000000"
    };

    private static readonly List<string> Masks = new List<string>
    {
        "masks/body-bodymask-0005", "masks/body-bodymask-0019", "masks/body-bodymask-0030",
        "masks/body-bodymask-0063", "masks/body-bodymask-0064", "masks/body-bodymask-0065",
        "masks/body-bodymask-0070", "masks/body-bodymask-0092", "masks/body-bodymask-0121",
        "masks/body-bodymask-0123", "masks/body-bodymask-0127", "masks/body-bodymask-0136",
        "masks/body-bodymask-0153",
        "masks/body-facemask-0005", "masks/body-facemask-0015", "masks/body-facemask-0030",
        "masks/body-facemask-0064", "masks/body-facemask-0065", "masks/body-facemask-0069",
        "masks/body-facemask-0090", "masks/body-facemask-0092", "masks/body-facemask-0136",
        "masks/body-facemask-0151", "masks/body-facemask-0152", "masks/body-facemask-0153",
        "masks/head-bodymask-0113", "masks/head-bodymask-0138",
        "masks/head-facemask-0003", "masks/head-facemask-0113", "masks/head-facemask-0137",
        "masks/head-facemask-0138",
        "masks/kigurumi-bodymask-0052", "masks/kigurumi-bodymask-0109", "masks/kigurumi-bodymask-0110",
        "masks/kigurumi-bodymask-0115", "masks/kigurumi-bodymask-0123",
        "masks/kigurumi-facemask-0052", "masks/kigurumi-facemask-0109", "masks/kigurumi-facemask-0110",
        "masks/kigurumi-facemask-0115", "masks/kigurumi-facemask-0123",
    };

    // Generated using https://codepen.io/sosuke/pen/Pjoqqp
    private static readonly string[] CostumeColorFilters =
    {
        "invert(48%) sepia(61%) saturate(6409%) hue-rotate(347deg) brightness(100%) contrast(95%);",
        "invert(90%) sepia(15%) saturate(1318%) hue-rotate(127deg) brightness(82%) contrast(78%);",
        "invert(15%) sepia(55%) saturate(6677%) hue-rotate(360deg) brightness(96%) contrast(102%);",
        "invert(88%) sepia(26%) saturate(147%) hue-rotate(343deg) brightness(106%) contrast(95%);",
        "invert(36%) sepia(47%) saturate(2136%) hue-rotate(146deg) brightness(97%) contrast(102%);",
        "invert(66%) sepia(50%) saturate(5209%) hue-rotate(124deg) brightness(92%) contrast(104%);",
        "invert(95%) sepia(83%) saturate(2827%) hue-rotate(73deg) brightness(101%) contrast(117%);",
        "invert(80%) sepia(39%) saturate(531%) hue-rotate(96deg) brightness(102%) contrast(101%);",
        "invert(100%);",
        "invert(12%) sepia(33%) saturate(6542%) hue-rotate(349deg) brightness(81%) contrast(117%);",
        "invert(15%) sepia(82%) saturate(6245%) hue-rotate(356deg) brightness(96%) contrast(125%);",
        "invert(51%) sepia(16%) saturate(2006%) hue-rotate(312deg) brightness(100%) contrast(100%);",
        "invert(97%) sepia(17%) saturate(5337%) hue-rotate(290deg) brightness(102%) contrast(100%);",
        "invert(52%) sepia(70%) saturate(2785%) hue-rotate(146deg) brightness(99%) contrast(105%);",
        "invert(84%) sepia(45%) saturate(4224%) hue-rotate(130deg) brightness(104%) contrast(103%);",
        "invert(82%) sepia(59%) saturate(1110%) hue-rotate(159deg) brightness(108%) contrast(104%);",
        "invert(85%) sepia(31%) saturate(483%) hue-rotate(158deg) brightness(104%) contrast(106%);",
        "invert(99%) sepia(69%) saturate(147%) hue-rotate(166deg) brightness(109%) contrast(79%);",
        "invert(17%) sepia(60%) saturate(5053%) hue-rotate(34deg) brightness(92%) contrast(101%);",
        "invert(53%) sepia(81%) saturate(4073%) hue-rotate(357deg) brightness(97%) contrast(112%);",
        "invert(66%) sepia(42%) saturate(573%) hue-rotate(326deg) brightness(101%) contrast(100%);",
        "invert(92%) sepia(9%) saturate(2479%) hue-rotate(306deg) brightness(105%) contrast(101%);",
        "invert(16%) sepia(70%) saturate(3568%) hue-rotate(195deg) brightness(95%) contrast(101%);",
        "invert(45%) sepia(67%) saturate(5622%) hue-rotate(193deg) brightness(104%) contrast(104%);",
        "invert(58%) sepia(74%) saturate(400%) hue-rotate(175deg) brightness(102%) contrast(102%);",
        "invert(78%) sepia(13%) saturate(856%) hue-rotate(177deg) brightness(104%) contrast(104%);",
        "invert(87%) sepia(0%) saturate(228%) hue-rotate(132deg) brightness(98%) contrast(66%);",
        "invert(52%) sepia(87%) saturate(3669%) hue-rotate(24deg) brightness(93%) contrast(100%);",
        "invert(63%) sepia(90%) saturate(1616%) hue-rotate(0deg) brightness(103%) contrast(105%);",
        "invert(74%) sepia(67%) saturate(370%) hue-rotate(346deg) brightness(102%) contrast(101%);",
        "invert(84%) sepia(32%) saturate(380%) hue-rotate(336deg) brightness(102%) contrast(106%);",
        "invert(11%) sepia(62%) saturate(7483%) hue-rotate(240deg) brightness(61%) contrast(117%);",
        "invert(13%) sepia(100%) saturate(4871%) hue-rotate(240deg) brightness(97%) contrast(143%);",
        "invert(68%) sepia(71%) saturate(6691%) hue-rotate(218deg) brightness(103%) contrast(100%);",
        "invert(76%) sepia(23%) saturate(1472%) hue-rotate(193deg) brightness(101%) contrast(103%);",
        "invert(55%) sepia(0%) saturate(428%) hue-rotate(151deg) brightness(96%) contrast(89%);",
        "invert(68%) sepia(42%) saturate(5979%) hue-rotate(24deg) brightness(97%) contrast(100%);",
        "invert(93%) sepia(14%) saturate(7042%) hue-rotate(356deg) brightness(103%) contrast(106%);",
        "invert(95%) sepia(23%) saturate(7492%) hue-rotate(1deg) brightness(107%) contrast(100%);",
        "invert(94%) sepia(94%) saturate(558%) hue-rotate(350deg) brightness(104%) contrast(107%);",
        "invert(15%) sepia(60%) saturate(5776%) hue-rotate(260deg) brightness(54%) contrast(123%);",
        "invert(17%) sepia(93%) saturate(5007%) hue-rotate(257deg) brightness(95%) contrast(157%);",
        "invert(46%) sepia(50%) saturate(3860%) hue-rotate(233deg) brightness(101%) contrast(102%);",
        "invert(71%) sepia(12%) saturate(1421%) hue-rotate(213deg) brightness(103%) contrast(102%);",
        "invert(29%) sepia(2%) saturate(0%) hue-rotate(226deg) brightness(99%) contrast(86%);",
        "invert(41%) sepia(97%) saturate(1327%) hue-rotate(66deg) brightness(94%) contrast(103%);",
        "invert(66%) sepia(70%) saturate(1886%) hue-rotate(40deg) brightness(97%) contrast(102%);",
        "invert(75%) sepia(98%) saturate(877%) hue-rotate(24deg) brightness(107%) contrast(105%);",
        "invert(86%) sepia(25%) saturate(644%) hue-rotate(29deg) brightness(105%) contrast(103%);",
        "invert(16%) sepia(49%) saturate(6851%) hue-rotate(281deg) brightness(59%) contrast(114%);",
        "invert(14%) sepia(97%) saturate(4833%) hue-rotate(282deg) brightness(102%) contrast(125%);",
        "invert(60%) sepia(34%) saturate(5439%) hue-rotate(240deg) brightness(96%) contrast(109%);",
        "invert(79%) sepia(50%) saturate(779%) hue-rotate(200deg) brightness(99%) contrast(106%);",
        "invert(0%) sepia(9%) saturate(62%) hue-rotate(43deg) brightness(108%) contrast(73%);",
        "invert(17%) sepia(45%) saturate(5456%) hue-rotate(101deg) brightness(100%) contrast(104%);",
        "invert(42%) sepia(53%) saturate(3615%) hue-rotate(93deg) brightness(104%) contrast(107%);",
        "invert(50%) sepia(63%) saturate(3097%) hue-rotate(88deg) brightness(124%) contrast(125%);",
        "invert(74%) sepia(96%) saturate(217%) hue-rotate(70deg) brightness(101%) contrast(101%);",
        "invert(17%) sepia(44%) saturate(7325%) hue-rotate(313deg) brightness(65%) contrast(109%);",
        "invert(13%) sepia(81%) saturate(7488%) hue-rotate(318deg) brightness(104%) contrast(102%);",
        "invert(60%) sepia(84%) saturate(2326%) hue-rotate(293deg) brightness(102%) contrast(100%);",
        "invert(88%) sepia(96%) saturate(1040%) hue-rotate(281deg) brightness(104%) contrast(106%);",
        "invert(0%);"
    };


    private static readonly string[] SpeedStrings =
    {
        "1.0", "1.1", "1.2", "1.3", "1.4",
        "1.5", "1.6", "1.7", "1.8", "1.9",
        "2.0", "2.5", "3.0", "3.5", "4.0"
    };

    private static readonly string[] NotePositionStrings = { "-5", "-4", "-3", "-2", "-1", "0", "+1", "+2", "+3", "+4", "+5" };

    private static readonly string[] TitlePlateStrings =
    {
        "Wood", "Rainbow", "Gold", "Purple",
        "AI 1", "AI 2", "AI 3", "AI 4",
        "Onp 1", "Toho Y22 QR", "Toho Y22 1", "Toho Y22 2",
        "Toho Y22 3", "Toho Y22 4", "Toho Y22 5", "AprilFool 1",
        "AprilFool 2", "AprilFool 3", "AprilFool 4", "AprilFool 5",
        "AprilFool 6"
    };

    private static readonly string[] LanguageStrings =
    {
        "Japanese", "English", "Chinese Traditional", "Korean", "Chinese Simplified"
    };

    private static readonly string[] DifficultySettingCourseStrings =
    {
        "None", "Set Up Each Time",
        "Easy", "Normal", "Hard", "Oni", "Ura Oni"
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
    
    private List<Costume> costumeList = new();
    private Dictionary<uint, Title> titleDictionary = new();
    private IReadOnlyDictionary<uint, Neiro> neiroDictionary = new Dictionary<uint, Neiro>();
    private Dictionary<string, List<uint>> lockedCostumeDataDictionary = new();
    private Dictionary<string, List<uint>> lockedTitleDataDictionary = new();
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
        lockedCostumeDataDictionary = IsGreen ? new Dictionary<string, List<uint>>() : await GameDataService.GetLockedCostumeDataDictionary();
        lockedTitleDataDictionary = IsGreen ? new Dictionary<string, List<uint>>() : await GameDataService.GetLockedTitleDataDictionary();
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
        titleValue = new TitlePickerValue(response.Title, response.TitlePlateId, response.UnlockedTitle);
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

        var ids = AuthService.AllowFreeProfileEditing
            ? catalogById.Keys.Concat(unlockedIds)
            : catalogById.Keys.Intersect(unlockedIds).Append(currentId);

        if (!IsGreen && AuthService.AllowFreeProfileEditing &&
            lockedCostumeDataDictionary.TryGetValue(costumeType, out var lockedIds))
        {
            ids = ids.Except(lockedIds);
        }

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
        var lockedTitleIds = !IsGreen && lockedTitleDataDictionary.TryGetValue("title", out var titleIds)
            ? titleIds.ToHashSet()
            : new HashSet<uint>();
        var lockedTitlePlateIds = !IsGreen && lockedTitleDataDictionary.TryGetValue("titlePlate", out var titlePlateIds)
            ? titlePlateIds.ToHashSet()
            : new HashSet<uint>();
        var currentTitleIds = titlesById.Values
            .Where(IsCurrentTitle)
            .Select(title => title.TitleId)
            .ToHashSet();

        var ids = AuthService.AllowFreeProfileEditing
            ? titlesById.Keys.Concat(response.UnlockedTitle)
            : titlesById.Keys.Intersect(response.UnlockedTitle).Concat(currentTitleIds);

        return ids
            .Distinct()
            .Where(id => TitleCanBeShown(id, titlesById, currentTitleIds, lockedTitleIds, lockedTitlePlateIds))
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
        var ids = AuthService.AllowFreeProfileEditing
            ? neirosById.Keys.Concat(unlockedIds)
            : neirosById.Keys.Intersect(unlockedIds).Append(currentId);

        return ids
            .Distinct()
            .OrderBy(id => id)
            .ToDictionary(
                id => id,
                id => neirosById.TryGetValue(id, out var neiro)
                    ? neiro
                    : new Neiro { NeiroId = id });
    }

    private bool TitleCanBeShown(
        uint id,
        IReadOnlyDictionary<uint, Title> titlesById,
        IReadOnlySet<uint> currentTitleIds,
        IReadOnlySet<uint> lockedTitleIds,
        IReadOnlySet<uint> lockedTitlePlateIds)
    {
        if (IsGreen)
        {
            return true;
        }

        if (!titlesById.TryGetValue(id, out var title))
        {
            return !lockedTitleIds.Contains(id);
        }

        var isCurrentTitle = currentTitleIds.Contains(id);
        var hasCurrentPlate = title.TitleRarity == response?.TitlePlateId;
        return (!lockedTitleIds.Contains(id) || isCurrentTitle) &&
               (!lockedTitlePlateIds.Contains(title.TitleRarity) || hasCurrentPlate);
    }

    private bool IsCurrentTitle(Title title)
    {
        response.ThrowIfNull();
        return StringMatchesCurrentTitle(title.TitleName) ||
               StringMatchesCurrentTitle(title.TitleNameEN) ||
               StringMatchesCurrentTitle(title.TitleNameCN) ||
               StringMatchesCurrentTitle(title.TitleNameKO);
    }

    private bool StringMatchesCurrentTitle(string title)
    {
        response.ThrowIfNull();
        return !string.IsNullOrWhiteSpace(response.Title) &&
               string.Equals(title, response.Title, StringComparison.Ordinal);
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
            response.UnlockedTitle = titleValue.UnlockedTitleIds.ToList();
            response.UnlockedTone = neiroValue.UnlockedIds.ToList();
        }
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

    public static string CostumeOrDefault(string file, uint id, string defaultfile)
    {
        var path = "/images/Costumes/";
        var filename = file + "-" + id.ToString().PadLeft(4, '0');
        var imagePath = path + filename + ".webp";
        var imageSrc = Masks.Contains(filename) ? imagePath : path + defaultfile + ".webp";
        return imageSrc;
    }

    private static string GetTitlePlateName(uint id)
    {
        return id < TitlePlateStrings.Length ? TitlePlateStrings[id] : "Wood";
    }
}
