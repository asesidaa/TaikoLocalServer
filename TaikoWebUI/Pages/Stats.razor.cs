using System.Globalization;
using Microsoft.JSInterop;

namespace TaikoWebUI.Pages;

// The player's own numbers come summed from api/Stats/{baid} (their bests and totals, never every play: megabytes for a
// heavy player); the comparisons with other players from api/Stats/{baid}/Server.
public partial class Stats
{
    [Parameter] public int Baid { get; set; }

    private UserSetting? setting;
    private List<DanBestData> dans = [];
    private List<StatsBest> best = [];
    private PlayerStatsSummary own = new();
    private Dictionary<uint, MusicDetail> music = [];
    // Across the whole server (api/Stats/{baid}/Server); null while loading or if it failed.
    private PlayerServerStats? server;
    private bool loaded;

    private static readonly Difficulty[] Courses = [Difficulty.Easy, Difficulty.Normal, Difficulty.Hard, Difficulty.Oni, Difficulty.UraOni];

    // Sequential one-hue ramp in the picked accent for magnitude; step 0 = no plays. Separate light / dark steps (dark
    // brightens with more plays). Only the accent steps every theme defines (100, 300-700).
    internal static readonly string[] Seq =
    [
        "bg-zinc-100 dark:bg-zinc-800", "bg-primary-100 dark:bg-primary-700/45", "bg-primary-300 dark:bg-primary-600/80",
        "bg-primary-500 dark:bg-primary-500", "bg-primary-700 dark:bg-primary-300",
    ];

    // Validated categorical trio for Good / OK / Bad (not the game's judgement colours, which fail CVD separation).
    private static readonly string[] Acc = ["bg-[#eb6834] dark:bg-[#d95926]", "bg-[#1baf7a] dark:bg-[#199e70]", "bg-[#2a78d6] dark:bg-[#3987e5]"];

    protected override async Task OnInitializedAsync()
    {
        Page.SetPlayData(Localizer["Play Data"], Baid, Localizer["Stats"]);
        var settingTask = Client.GetFromJsonAsync<UserSetting>($"api/UserSettings/{Baid}");
        var dansTask = Client.GetFromJsonAsync<DanBestDataResponse>($"api/DanBestData/{Baid}");
        var ownTask = Client.GetFromJsonAsync<PlayerStatsSummary>($"api/Stats/{Baid}");
        music = await GameDataService.GetMusicDetailDictionary();
        setting = await settingTask;
        dans = (await dansTask)?.DanBestDataList ?? [];
        own = await ownTask ?? new();
        best = own.Bests;
        loaded = true;
        StateHasChanged();
        // The server-wide numbers come after the rest is shown.
        server = await Client.GetFromJsonAsync<PlayerServerStats>($"api/Stats/{Baid}/Server");
    }

    protected override Task OnAfterRenderAsync(bool firstRender) => Js.InvokeVoidAsync("ui.hydrateAssets").AsTask();

    private string SongName(uint id) => GameDataService.GetMusicNameBySongId(music, id) is { Length: > 0 } n ? n : $"#{id}";

    // ---- Tiles ----
    private int SongsPlayed => best.Select(b => b.SongId).Distinct().Count();
    private string ClearRate => best.Count == 0 ? "–" : $"{best.Count(b => b.BestCrown >= CrownType.Clear) * 100 / best.Count}%";
    private int FullCombos => best.Count(b => b.BestCrown >= CrownType.Gold);
    private int Donderfuls => best.Count(b => b.BestCrown == CrownType.Dondaful);
    private uint BestCombo => own.BestCombo;
    private int PlaysThisWeek => own.PlaysThisWeek;

    // ---- Scoreboard (the in-game achievement panel): same counting as the game ----
    // None = the highest difficulty with plays; Ura Oni also counts Oni.
    private Difficulty PanelDifficulty
    {
        get
        {
            var d = setting?.AchievementDisplayDifficulty ?? Difficulty.None;
            if (d != Difficulty.None) return d;
            return Courses.LastOrDefault(c => best.Any(b => b.Difficulty == c), Difficulty.Oni);
        }
    }

    private int[] PanelCounts()
    {
        var d = PanelDifficulty;
        var rows = best.Where(b => b.Difficulty == d || (d == Difficulty.UraOni && b.Difficulty == Difficulty.Oni)).ToList();
        int Rank(ScoreRank r) => rows.Count(b => b.BestScoreRank == r);
        int Crown(CrownType c) => rows.Count(b => b.BestCrown == c);
        // Slot order: 極, 雅 gold/pink/purple, 粋 white/bronze/silver, crowns clear/full combo/donderful.
        return [Rank(ScoreRank.Dondaful), Rank(ScoreRank.Gold), Rank(ScoreRank.Sakura), Rank(ScoreRank.Purple),
                Rank(ScoreRank.White), Rank(ScoreRank.Bronze), Rank(ScoreRank.Silver),
                Crown(CrownType.Clear), Crown(CrownType.Gold), Crown(CrownType.Dondaful)];
    }

    // Number centres on the 328×192 panel (from the game's player_board layout).
    private static readonly (int X, int Y)[] Slots = [(278, 35), (88, 74), (180, 74), (278, 74), (88, 112), (180, 112), (278, 112), (88, 155), (180, 155), (278, 155)];
    private static readonly int[][] DigitOffsets = [[0], [-7, 7], [-14, 0, 14], [-21, -7, 7, 21]];

    private static string PanelName(Difficulty d) => d switch
    {
        Difficulty.Easy => "easy", Difficulty.Normal => "normal", Difficulty.Hard => "hard", Difficulty.UraOni => "oniura", _ => "oni",
    };

    private static string Pct(double v, double of) => FormattableString.Invariant($"{v / of * 100:0.###}%");

    // ---- Crowns by difficulty ----
    private (int Played, int Clear, int Full, int Donderful) Crowns(Difficulty d)
    {
        var rows = best.Where(b => b.Difficulty == d).ToList();
        return (rows.Count, rows.Count(b => b.BestCrown >= CrownType.Clear), rows.Count(b => b.BestCrown >= CrownType.Gold), rows.Count(b => b.BestCrown == CrownType.Dondaful));
    }

    // ---- Clears by star level, for the picked difficulty: cleared vs available ----
    private Difficulty starsDifficulty = Difficulty.Oni;

    private (int Cleared, int Available)[] StarLevels()
    {
        var result = new (int, int)[10];
        foreach (var id in music.Keys)
        {
            var star = GameDataService.GetMusicStarLevel(music, id, starsDifficulty);
            if (star is >= 1 and <= 10) result[star - 1].Item2++;
        }
        foreach (var b in best.Where(b => b.Difficulty == starsDifficulty && b.BestCrown >= CrownType.Clear))
        {
            var star = GameDataService.GetMusicStarLevel(music, b.SongId, starsDifficulty);
            if (star is >= 1 and <= 10) result[star - 1].Item1++;
        }
        return result;
    }

    // ---- Activity: the last year, one column per week (Monday first), up to today ----
    private const int ActivityWeeks = 53;

    private (DateTime Day, int Plays)[] Activity()
    {
        var today = DateTime.Now.Date;
        var start = today.AddDays(-(((int)today.DayOfWeek + 6) % 7) - 7 * (ActivityWeeks - 1));
        var counts = own.Days.ToDictionary(d => d.Day.Date, d => d.Plays);
        return Enumerable.Range(0, (today - start).Days + 1).Select(i => start.AddDays(i)).Select(d => (d, counts.GetValueOrDefault(d))).ToArray();
    }

    private static int Bucket(int v) => v switch { 0 => 0, <= 2 => 1, <= 4 => 2, <= 6 => 3, _ => 4 };

    private static int LongestStreak((DateTime Day, int Plays)[] days)
    {
        int longest = 0, run = 0;
        foreach (var (_, plays) in days) longest = Math.Max(longest, run = plays > 0 ? run + 1 : 0);
        return longest;
    }

    // ---- Next goals (the closest places come from the server) ----

    // Charts without a full combo, by the fewest Bads in any play of them (that play's OKs alongside): from the server.
    private List<FullComboGoal> ClosestFullCombos() => own.ClosestFullCombos;

    // The Oni star level closest to half cleared (of those you've cleared something at but not half): the fewest clears
    // to go, the harder level on a tie.
    private (Difficulty Difficulty, int Star, int Cleared, int Available, int Need)? StarTarget()
    {
        var d = Difficulty.Oni;
        var available = new int[11];
        var cleared = new int[11];
        foreach (var id in music.Keys)
            if (GameDataService.GetMusicStarLevel(music, id, d) is >= 1 and <= 10 and var s) available[s]++;
        foreach (var b in best.Where(b => b.Difficulty == d && b.BestCrown >= CrownType.Clear))
            if (GameDataService.GetMusicStarLevel(music, b.SongId, d) is >= 1 and <= 10 and var s) cleared[s]++;
        (Difficulty, int, int, int, int)? target = null;
        for (var star = 10; star >= 1; star--)
        {
            var need = (available[star] + 1) / 2 - cleared[star];
            if (cleared[star] > 0 && need > 0 && (target is null || need < target.Value.Item5)) target = (d, star, cleared[star], available[star], need);
        }
        return target;
    }

    // ---- Recent personal bests (the last 14 days', from the server) ----
    private List<RecentBest> RecentBests() => own.RecentBests;

    // ---- Genres, accuracy, most played (plays per song and judgement totals from the server) ----
    private List<(SongGenre Genre, int Plays)> Genres() => own.Songs
        .GroupBy(s => GameDataService.GetMusicGenreBySongId(music, s.SongId)).Select(g => (g.Key, g.Sum(s => s.Plays)))
        .OrderByDescending(g => g.Item2).ToList();

    private (string Label, double Pct)[] Accuracy()
    {
        double good = own.Good, ok = own.Ok, bad = own.Bad;
        var total = good + ok + bad;
        if (total == 0) return [];
        return [(Localizer["Good"], good / total * 100), (Localizer["OK"], ok / total * 100), (Localizer["Bad"], bad / total * 100)];
    }

    private List<(uint SongId, int Plays)> MostPlayed() => own.Songs.OrderByDescending(s => s.Plays).Take(5).Select(s => (s.SongId, s.Plays)).ToList();
}
