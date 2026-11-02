using System.Globalization;
using System.Text.Json;

namespace TaikoLocalServer.Adapters.AdminApi.Controllers;

/// <summary>
/// The older games' extra modes for their Play Data pages: Blue's Battle mode, Training (Blue, White, Yellow), Green's
/// Ghost battles, and Green's friend list, which the web can add to and remove from (the cabinet gets it with the
/// player's user data on their next card tap).
/// </summary>
[ApiController]
[Authorize]
[ApiScope(ApiScopes.ReadScores, ApiScopes.AdminUsers)]
public class PlayModesController(ITaikoDbContext context) : BaseAdminController<PlayModesController>
{
    private const int Recent = 50;

    // shortcut: the cabinet's friend list size isn't known (30 is the prototype's guess); raise it once confirmed.
    private const int MaxFriends = 30;

    [HttpGet("/api/Blue/Battle/{baid}")]
    public async Task<ActionResult<BattleResponse>> Battle(uint baid)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;
        var ct = HttpContext.RequestAborted;
        var user = await context.BlueBattleUserStates.AsNoTracking().FirstOrDefaultAsync(u => u.Baid == baid, ct);
        var npcs = await context.BlueBattleNpcStates.AsNoTracking().Where(n => n.Baid == baid).OrderBy(n => n.NpcId).ToListAsync(ct);
        var tokens = await context.BlueBattleTokenStates.AsNoTracking().Where(t => t.Baid == baid).OrderBy(t => t.TokenId).ToListAsync(ct);
        var results = await context.BlueBattleStageResults.AsNoTracking().Where(r => r.Baid == baid)
            .OrderByDescending(r => r.Id).ToListAsync(ct);

        var unlocked = Bits(user?.ReleaseBattleStageFlg);
        var cleared = results.Where(r => r.ClearFlag == 1 && r.BattleStageId is not null).Select(r => r.BattleStageId!.Value).Distinct().Order().ToList();
        var highest = unlocked.Concat(cleared).DefaultIfEmpty(0u).Max();
        return new BattleResponse(Math.Max(10, (highest + 9) / 10 * 10), cleared, unlocked, user?.LastBattleStageId, user?.LastBossLife,
            user?.BattleBondsLvCap,
            npcs.Select(n => new BattlePartner(n.NpcId, n.TotalExp ?? 0, n.MaxDpn ?? 0, n.BondsLevel ?? 0,
                new[] { n.SelectedSpecialId1, n.SelectedSpecialId2, n.SelectedSpecialId3 }.OfType<uint>().Where(s => s > 0).ToList())).ToList(),
            tokens.Select(t => new BattleToken(t.TokenId, t.TokenValue ?? 0)).ToList(),
            results.Take(Recent).Select(r => new BattleRecord(r.PlayDatetime ?? r.CreatedAt, r.BattleStageId, r.SongNo, r.Level, r.NpcId,
                r.AcquiredExp, r.Dpn, r.ClearFlag == 1, r.BossLife)).ToList());
    }

    [HttpGet("/api/{era}/Training/{baid}")]
    public async Task<ActionResult<IReadOnlyList<TrainingSession>>> Training(string era, uint baid)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;
        if (!EraRoute.TryParse(era, out var gameEra) || gameEra is not (GameEra.Blue or GameEra.White or GameEra.Yellow))
            return BadRequest(new { message = $"{era} has no Training mode." });
        var ct = HttpContext.RequestAborted;
        var rows = gameEra switch
        {
            GameEra.Blue => await context.BlueTokkunStageResults.AsNoTracking().Where(r => r.Baid == baid).OrderByDescending(r => r.Id)
                .Select(r => new { r.PlayDatetime, r.TookunSongnoesJson, r.TokkunSpeedchangeCnt, r.TokkunAutoplayCnt, r.TokkunJumpCnt }).ToListAsync(ct),
            GameEra.White => await context.WhiteTokkunStageResults.AsNoTracking().Where(r => r.Baid == baid).OrderByDescending(r => r.Id)
                .Select(r => new { r.PlayDatetime, r.TookunSongnoesJson, r.TokkunSpeedchangeCnt, r.TokkunAutoplayCnt, r.TokkunJumpCnt }).ToListAsync(ct),
            _ => await context.YellowTokkunStageResults.AsNoTracking().Where(r => r.Baid == baid).OrderByDescending(r => r.Id)
                .Select(r => new { r.PlayDatetime, r.TookunSongnoesJson, r.TokkunSpeedchangeCnt, r.TokkunAutoplayCnt, r.TokkunJumpCnt }).ToListAsync(ct),
        };
        return Ok(rows.Select(r => new TrainingSession(PlayTime(r.PlayDatetime), Songs(r.TookunSongnoesJson),
            r.TokkunSpeedchangeCnt, r.TokkunAutoplayCnt, r.TokkunJumpCnt)).ToList());
    }

    [HttpGet("/api/Green/Ghost/{baid}")]
    public async Task<ActionResult<GhostResponse>> Ghost(uint baid)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;
        var ct = HttpContext.RequestAborted;
        var levels = await context.GreenGhostWinnings.AsNoTracking().Where(w => w.Baid == baid).OrderBy(w => w.LevelId)
            .Select(w => new GhostLevel(w.LevelId, w.Winnings)).ToListAsync(ct);
        var tokens = await context.GreenGhostTokens.AsNoTracking().Where(t => t.Baid == baid).OrderBy(t => t.TokenId)
            .Select(t => new BattleToken(t.TokenId, t.TokenValue)).ToListAsync(ct);
        // The player's plays that were ghost battles (they have section results), newest first.
        var sections = await context.GhostStageSectionDataGreen.AsNoTracking()
            .Join(context.SongPlayDataGreen.Where(p => p.Baid == baid), s => s.PlayId, p => p.Id, (s, p) => new { s, p })
            .OrderByDescending(x => x.p.Id).ThenBy(x => x.s.SectionNo)
            .Take(Recent * 4).ToListAsync(ct);
        var recent = sections.GroupBy(x => x.p.Id).Take(Recent).Select(g =>
        {
            var p = g.First().p;
            return new GhostBattle(p.PlayTime, p.SongId, (uint)p.Difficulty, p.Score,
                g.Select(x => new GhostSection(x.s.SectionNo, x.s.IsWin, x.s.GoodCount, x.s.OkCount, x.s.NgCount, x.s.PoundCount)).ToList());
        }).ToList();
        return new GhostResponse(levels, tokens, recent);
    }

    [HttpGet("/api/Green/Friends/{baid}")]
    public async Task<ActionResult<IReadOnlyList<GreenFriend>>> Friends(uint baid)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;
        return Ok(await FriendList(baid));
    }

    /// <summary>Adds a player by the access code on their card. Only someone who has played Green can be added.</summary>
    [HttpPost("/api/Green/Friends/{baid}")]
    public async Task<IActionResult> AddFriend(uint baid, [FromBody] AddFriendRequest request)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;
        var ct = HttpContext.RequestAborted;
        var code = new string(request.AccessCode.Where(char.IsAsciiDigit).ToArray());
        var card = await context.Cards.AsNoTracking().FirstOrDefaultAsync(c => c.AccessCode == code, ct);
        if (card is null)
            return BadRequest(new { message = "No card has that access code." });
        if (card.Baid == baid)
            return BadRequest(new { message = "That's your own card." });
        var save = await context.UserSaveDataGreen.AsNoTracking().FirstOrDefaultAsync(s => s.Baid == card.Baid, ct);
        if (save is null)
            return BadRequest(new { message = "That player hasn't played Green yet." });
        var mine = await context.GreenFriends.Where(f => f.Baid == baid).ToListAsync(ct);
        if (mine.Any(f => f.FriendBaid == card.Baid))
            return BadRequest(new { message = "They're already on your list." });
        if (mine.Count >= MaxFriends)
            return BadRequest(new { message = $"The list is full ({MaxFriends})." });

        var name = (await context.UserData.FindAsync([card.Baid], ct))?.MyDonName ?? "";
        context.GreenFriends.Add(new GreenFriends { Baid = baid, FriendBaid = card.Baid, FriendName = name });
        this.LogIfSomeoneElse(context, "friends.add", baid, $"#{card.Baid}");
        await context.SaveChangesAsync(ct);
        return Ok(await FriendList(baid));
    }

    [HttpDelete("/api/Green/Friends/{baid}/{friendBaid}")]
    public async Task<IActionResult> RemoveFriend(uint baid, uint friendBaid)
    {
        if (this.AuthorizeOwnerOrAdmin(baid) is { } forbid)
            return forbid;
        var row = await context.GreenFriends.FindAsync([baid, friendBaid], HttpContext.RequestAborted);
        if (row is null)
            return NotFound();
        context.GreenFriends.Remove(row);
        this.LogIfSomeoneElse(context, "friends.remove", baid, $"#{friendBaid}");
        await context.SaveChangesAsync(HttpContext.RequestAborted);
        return NoContent();
    }

    private async Task<List<GreenFriend>> FriendList(uint baid)
    {
        var ct = HttpContext.RequestAborted;
        var friends = await context.GreenFriends.AsNoTracking().Where(f => f.Baid == baid).ToListAsync(ct);
        var ids = friends.Select(f => f.FriendBaid).ToList();
        var last = await context.UserSaveDataGreen.AsNoTracking().Where(s => ids.Contains(s.Baid))
            .ToDictionaryAsync(s => s.Baid, s => s.LastPlayDatetime, ct);
        var names = await context.UserData.AsNoTracking().Where(u => ids.Contains(u.Baid)).ToDictionaryAsync(u => u.Baid, u => u.MyDonName, ct);
        return friends.Select(f => new GreenFriend(f.FriendBaid, names.GetValueOrDefault(f.FriendBaid) ?? f.FriendName,
            last.TryGetValue(f.FriendBaid, out var t) ? t : null)).OrderBy(f => f.Name).ToList();
    }

    // The bits set in a flag array, as 1-based numbers.
    private static List<uint> Bits(byte[]? flags) =>
        flags is null ? [] : Enumerable.Range(0, flags.Length * 8).Where(i => (flags[i / 8] & (1 << (i % 8))) != 0).Select(i => (uint)i + 1).ToList();

    private static DateTime? PlayTime(string value) =>
        DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d
        : DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out d) ? d : null;

    private static List<uint> Songs(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<uint>>(json)?.Where(s => s > 0).ToList() ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}

