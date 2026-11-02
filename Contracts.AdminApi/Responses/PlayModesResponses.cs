namespace TaikoLocalServer.Contracts.AdminApi.Responses;

// The older games' extra modes, for their Play Data pages. Stages, partners and levels are numbers: there's no name
// catalogue for them yet.

/// <summary>Blue's Battle mode. Unlocked comes from the game's stage flags (bit n-1 is stage n); Cleared from the results.</summary>
public sealed record BattleResponse(uint StageCount, IReadOnlyList<uint> Cleared, IReadOnlyList<uint> Unlocked,
    uint? LastStageId, uint? LastBossLife, uint? BondsCap, IReadOnlyList<BattlePartner> Partners, IReadOnlyList<BattleToken> Tokens,
    IReadOnlyList<BattleRecord> Recent);

public sealed record BattlePartner(uint NpcId, uint Exp, uint MaxDpn, uint Bonds, IReadOnlyList<uint> Specials);

public sealed record BattleToken(uint TokenId, uint Value);

/// <summary>Difficulty is the result's level (1 Easy … 5 Ura Oni). BossLife is what the boss had left.</summary>
public sealed record BattleRecord(DateTime? At, uint? StageId, uint? SongNo, uint? Difficulty, uint? NpcId, uint? ExpGained,
    uint? Dpn, bool Won, uint? BossLife);

/// <summary>Blue, White and Yellow's Training (Tokkun): one row per session; it doesn't count towards scores.</summary>
public sealed record TrainingSession(DateTime? At, IReadOnlyList<uint> Songs, uint SpeedChanges, uint Autoplays, uint Jumps);

/// <summary>Green's Ghost battles: wins per ghost level, tokens, and recent battles section by section.</summary>
public sealed record GhostResponse(IReadOnlyList<GhostLevel> Levels, IReadOnlyList<BattleToken> Tokens, IReadOnlyList<GhostBattle> Recent);

public sealed record GhostLevel(uint LevelId, uint Wins);

public sealed record GhostBattle(DateTime At, uint SongId, uint Difficulty, uint Score, IReadOnlyList<GhostSection> Sections);

public sealed record GhostSection(uint No, bool Won, uint Good, uint Ok, uint Bad, uint Drumroll);

/// <summary>A player on someone's Green friend list (the cabinet's list). LastPlayed is their last Green play.</summary>
public sealed record GreenFriend(uint Baid, string Name, DateTime? LastPlayed);
