using System.Globalization;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Common;

public static class BlueBattleStateExtensions
{
    private const int BattleReleaseInfoBytes = 16;
    private const int BattleReleaseStageBytes = 8;
    private const int BattleNpcCostumeBytes = 4;
    private const int BattleNpcSpecialBytes = 16;

    public static async Task AddBlueBattleStageResultsAsync(
        this ITaikoDbContext context,
        uint baid,
        IReadOnlyList<Ac15StageResult> stages,
        uint playMode,
        DateTime playTime,
        DateTime now,
        CancellationToken cancellationToken)
    {
        BlueBattleUserState? userState = null;
        for (var index = 0; index < stages.Count; index++)
        {
            var stage = stages[index];
            var battleStage = stage.BlueBattleStage;
            if (battleStage is null)
            {
                continue;
            }

            var npc = battleStage.NpcData;
            context.BlueBattleStageResults.Add(new BlueBattleStageResult
            {
                Baid = baid,
                CreatedAt = now,
                PlayDatetime = playTime,
                PlayMode = playMode,
                StageMode = stage.StageMode,
                StageIndex = (uint)index,
                SongNo = stage.SongNo,
                Level = Ac15Difficulty.ToProtocol(stage.Level),
                BattleStageId = battleStage.BattleStageId,
                NpcId = npc?.NpcId,
                ResultType = stage.PlayResult,
                BossLife = battleStage.BossLife,
                TotalExp = ParseOptionalUInt32(npc?.TotalExp),
                AcquiredExp = ParseOptionalUInt32(npc?.AcquiredExp),
                Dpn = npc?.Dpn
            });

            userState ??= await context.GetOrCreateBlueBattleUserStateAsync(baid, now, cancellationToken);
            userState.LastBattleStageId = battleStage.BattleStageId;
            userState.LastBossLife = battleStage.BossLife;
            if (npc is not null)
            {
                userState.LastNpcId = npc.NpcId;
                await context.UpsertBlueBattleNpcStateAsync(baid, npc, now, cancellationToken);
            }
        }
    }

    public static async Task ApplyBlueBattleReleaseDataAsync(
        this ITaikoDbContext context,
        uint baid,
        Ac15BlueBattleReleaseData? releaseData,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (releaseData is null)
        {
            return;
        }

        var userState = await context.GetOrCreateBlueBattleUserStateAsync(baid, now, cancellationToken);
        userState.ReleaseInfoFlg = SetBattleBits(
            userState.ReleaseInfoFlg,
            releaseData.ReleaseInfoIds,
            BattleReleaseInfoBytes);
        userState.ReleaseBattleStageFlg = SetBattleBits(
            userState.ReleaseBattleStageFlg,
            releaseData.ReleaseBattleStageIds,
            BattleReleaseStageBytes);
        userState.AssignStageId = releaseData.AssignNextStageId;
        if (userState.LastNpcId is { } lastNpcId)
        {
            var npcState = await context.BlueBattleNpcStates.FindAsync([baid, lastNpcId], cancellationToken);
            if (npcState is not null)
            {
                npcState.NpcCostumeFlg = SetBattleBits(
                    npcState.NpcCostumeFlg,
                    releaseData.ReleaseNpcCostumeIds.Where(id => id > 0),
                    BattleNpcCostumeBytes);
                npcState.ReleaseSpecialFlg = SetBattleBits(
                    npcState.ReleaseSpecialFlg,
                    releaseData.ReleaseNpcSpecialIds.Where(id => id > 0),
                    BattleNpcSpecialBytes);
                npcState.UpdatedAt = now;
            }
        }

        foreach (var token in releaseData.BattleTokenData)
        {
            await context.UpsertBlueBattleTokenStateAsync(baid, token, now, cancellationToken);
        }
    }

    private static async Task<BlueBattleUserState> GetOrCreateBlueBattleUserStateAsync(
        this ITaikoDbContext context,
        uint baid,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var userState = await context.BlueBattleUserStates.FindAsync([baid], cancellationToken);
        if (userState is null)
        {
            userState = new BlueBattleUserState
            {
                Baid = baid,
                CreatedAt = now
            };
            context.BlueBattleUserStates.Add(userState);
        }

        userState.UpdatedAt = now;
        return userState;
    }

    private static async Task UpsertBlueBattleNpcStateAsync(
        this ITaikoDbContext context,
        uint baid,
        Ac15BlueBattleNpcData npc,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var npcState = await context.BlueBattleNpcStates.FindAsync([baid, npc.NpcId], cancellationToken);
        if (npcState is null)
        {
            npcState = new BlueBattleNpcState
            {
                Baid = baid,
                NpcId = npc.NpcId,
                CreatedAt = now
            };
            context.BlueBattleNpcStates.Add(npcState);
        }

        npcState.TotalExp = ParseOptionalUInt32(npc.TotalExp);
        npcState.MaxDpn = Math.Max(npcState.MaxDpn ?? 0, npc.Dpn);
        npcState.NpcCostumeId = npc.NpcCostumeId;
        npcState.NpcCostumeFlg = BlueProtocolBytes.FixedOrZero(npcState.NpcCostumeFlg, BattleNpcCostumeBytes);
        npcState.SelectedSpecialId1 = npc.SpecialId1;
        npcState.SelectedSpecialId2 = npc.SpecialId2;
        npcState.SelectedSpecialId3 = npc.SpecialId3;
        npcState.ReleaseSpecialFlg = SetBattleBits(
            npcState.ReleaseSpecialFlg,
            [npc.SpecialId1, npc.SpecialId2, npc.SpecialId3],
            BattleNpcSpecialBytes);
        npcState.BondsLevel = npc.BondsLv;
        npcState.UpdatedAt = now;
    }

    private static async Task UpsertBlueBattleTokenStateAsync(
        this ITaikoDbContext context,
        uint baid,
        Ac15BlueBattleTokenData token,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var tokenState = await context.BlueBattleTokenStates.FindAsync([baid, token.TokenId], cancellationToken);
        if (tokenState is null)
        {
            tokenState = new BlueBattleTokenState
            {
                Baid = baid,
                TokenId = token.TokenId,
                CreatedAt = now
            };
            context.BlueBattleTokenStates.Add(tokenState);
        }

        tokenState.TokenValue = token.TokenValue;
        tokenState.UpdatedAt = now;
    }

    private static uint? ParseOptionalUInt32(string? value)
        => uint.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;

    private static byte[] SetBattleBits(byte[]? source, IEnumerable<uint> ids, int byteCount)
    {
        var result = BlueProtocolBytes.FixedOrZero(source, byteCount);
        foreach (var id in ids)
        {
            if (id >= byteCount * 8)
            {
                continue;
            }

            result[id >> 3] |= (byte)(1 << ((int)id & 7));
        }

        return result;
    }

}
