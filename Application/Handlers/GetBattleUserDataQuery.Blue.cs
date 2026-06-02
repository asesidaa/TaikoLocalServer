using System.Globalization;

namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetBattleUserDataQuery(uint Baid) : IRequest<CommonBattleUserDataResponse>;

public sealed class GetBattleUserDataQueryHandler(
    ITaikoDbContext context,
    IGameDataCatalog gameDataService,
    ILogger<GetBattleUserDataQueryHandler> logger)
    : IRequestHandler<GetBattleUserDataQuery, CommonBattleUserDataResponse>
{
    private const uint BattleNpcCostumeConsumedBits = BlueProtocolBytes.BattleNpcCostumeFlagBytes * 8;

    public async ValueTask<CommonBattleUserDataResponse> Handle(
        GetBattleUserDataQuery request,
        CancellationToken cancellationToken)
    {
        _ = logger;
        _ = gameDataService;
        _ = await context.UserData.FindAsync([request.Baid], cancellationToken)
            ?? throw new InvalidOperationException($"User not found for Blue battleuserdata baid {request.Baid}.");

        var userState = await context.BlueBattleUserStates
            .AsNoTracking()
            .SingleOrDefaultAsync(row => row.Baid == request.Baid, cancellationToken);
        var npcStates = await context.BlueBattleNpcStates
            .AsNoTracking()
            .Where(row => row.Baid == request.Baid
                          && row.TotalExp != null
                          && row.MaxDpn != null
                          && row.NpcCostumeId != null
                          && row.SelectedSpecialId1 != null
                          && row.SelectedSpecialId2 != null
                          && row.SelectedSpecialId3 != null)
            .OrderBy(row => row.NpcId)
            .ToListAsync(cancellationToken);
        var npcs = npcStates
            .Select(row => new CommonBattleUserDataResponse.BattleUserNpcData
            {
                NpcId = row.NpcId,
                TotalExp = row.TotalExp!.Value.ToString(CultureInfo.InvariantCulture),
                MaxDpn = row.MaxDpn!.Value,
                NpcCostumeId = row.NpcCostumeId!.Value,
                NpcCostumeFlg = BuildNpcCostumeFlg(row.NpcCostumeFlg, row.NpcCostumeId.Value),
                LastSelectSpecial1 = row.SelectedSpecialId1!.Value,
                LastSelectSpecial2 = row.SelectedSpecialId2!.Value,
                LastSelectSpecial3 = row.SelectedSpecialId3!.Value,
                ReleaseSpecialFlg = BlueProtocolBytes.CreateBattleSpecialBitset(GetNpcSpecialIds(row))
            })
            .ToList();
        var tokens = await context.BlueBattleTokenStates
            .AsNoTracking()
            .Where(row => row.Baid == request.Baid && row.TokenValue != null)
            .OrderBy(row => row.TokenId)
            .Select(row => new CommonBattleUserDataResponse.BattleUserTokenData
            {
                TokenId = row.TokenId,
                TokenValue = row.TokenValue!.Value
            })
            .ToListAsync(cancellationToken);
        EnsureBattleIntroTokenSlot(tokens);
        var useStarterState = userState is null;
        return new CommonBattleUserDataResponse
        {
            Result = 1,
            ReleaseInfoFlg = BuildReleaseInfoFlg(userState),
            ReleaseBattleStageFlg = BuildBattleStageFlg(userState, useStarterState),
            LastBattleStageId = userState?.LastBattleStageId ?? 0,
            LastBossLife = userState?.LastBossLife ?? 0,
            LastNpcId = userState?.LastNpcId ?? npcs.FirstOrDefault()?.NpcId ?? 0,
            NpcDatas = npcs.Count == 0 ? [CreateSafeStarterNpcData()] : npcs,
            AryTokenDatas = tokens,
            AssignStageId = userState?.AssignStageId ?? 1
        };
    }

    private static IEnumerable<uint> GetNpcSpecialIds(BlueBattleNpcState row)
    {
        if (row.ReleaseSpecialFlg is not null)
        {
            for (uint id = 0; id < BlueProtocolBytes.BattleSpecialFlagBytes * 8; id++)
            {
                if ((row.ReleaseSpecialFlg[(int)(id >> 3)] & (1 << ((int)id & 7))) != 0)
                {
                    yield return id;
                }
            }
        }

        if (row.SelectedSpecialId1 is > 0)
        {
            yield return row.SelectedSpecialId1.Value;
        }

        if (row.SelectedSpecialId2 is > 0)
        {
            yield return row.SelectedSpecialId2.Value;
        }

        if (row.SelectedSpecialId3 is > 0)
        {
            yield return row.SelectedSpecialId3.Value;
        }
    }

    private static byte[] BuildNpcCostumeFlg(byte[]? source, uint selectedCostumeId)
    {
        var result = BlueProtocolBytes.FixedOrZero(source, BlueProtocolBytes.BattleNpcCostumeFlagBytes);
        if (selectedCostumeId < BattleNpcCostumeConsumedBits)
        {
            result[selectedCostumeId >> 3] |= (byte)(1 << ((int)selectedCostumeId & 7));
        }

        return result;
    }

    // token_id is the battletokeninfo.xml <id> and is used verbatim as a flat_map key by the
    // battle runtime (NORMAL token = id 1, the always-available entry). The battle-intro storyboard
    // selector (EBOOT sub_EEF48) builds the EVENT-token view (kind 0), whose default key is 0, then
    // does an UNGUARDED flat_map::at(0) (no contains-probe) and dereferences it. If the response has
    // no token_id 0 row, at() throws "flat_map::at key not found" -> unhandled on PS3 -> crash on
    // entering battle. The song-select display path is guarded, so a missing key there only yields 0
    // ("not reflected"). So: always include a token_id 0 row, and echo real ids (never -1).
    // See .tools/blue/battleuserdata-response-xrefs.md.
    private static void EnsureBattleIntroTokenSlot(List<CommonBattleUserDataResponse.BattleUserTokenData> tokens)
    {
        if (tokens.Any(token => token.TokenId == BlueProtocolBytes.BattleIntroTokenId))
        {
            return;
        }

        tokens.Insert(0, new CommonBattleUserDataResponse.BattleUserTokenData
        {
            TokenId = BlueProtocolBytes.BattleIntroTokenId,
            TokenValue = 0
        });
    }

    // release_info_flg has INVERTED semantics versus release_battle_stage_flg: it is the
    // game's "intro/result/chat clip already seen" suppression mask. The battle-intro storyboard
    // selector (EBOOT sub_EEF48) plays a clip only when its clip-id bit here is CLEAR; a set bit
    // hides the clip. The prologue/tutorial (intro/01 + JINGLE_BTLPROLOG) is clip-id 1, so setting
    // bit 1 suppresses the first-time battle tutorial. PlayResult release_info ids are how the
    // client reports newly-seen clips, which we persist and echo back here. A brand-new user has
    // seen nothing, so the starter mask must be all zeros (never seed bits). See
    // .tools/blue/battleuserdata-response-xrefs.md.
    private static byte[] BuildReleaseInfoFlg(BlueBattleUserState? userState)
        => BlueProtocolBytes.FixedOrZero(
            userState?.ReleaseInfoFlg,
            BlueProtocolBytes.BattleInfoFlagBytes);

    private static byte[] BuildBattleStageFlg(BlueBattleUserState? userState, bool useStarterState)
    {
        var result = BlueProtocolBytes.FixedOrZero(
            userState?.ReleaseBattleStageFlg,
            BlueProtocolBytes.BattleStageFlagBytes);
        if (useStarterState)
        {
            SetBitIfInRange(result, BlueProtocolBytes.BattleDefaultStageId);
            SetBitIfInRange(result, 1);
        }

        SetBitIfInRange(result, userState?.LastBattleStageId);
        SetBitIfInRange(result, userState?.AssignStageId);
        return result;
    }

    private static void SetBitIfInRange(byte[] result, uint? id)
    {
        if (id is not { } value || value == 0 || value >= result.Length * 8)
        {
            return;
        }

        result[value >> 3] |= (byte)(1 << ((int)value & 7));
    }

    private static CommonBattleUserDataResponse.BattleUserNpcData CreateSafeStarterNpcData()
        => new()
        {
            NpcId = 0,
            TotalExp = "0",
            MaxDpn = 0,
            NpcCostumeId = 0,
            NpcCostumeFlg = BuildNpcCostumeFlg(null, 0),
            LastSelectSpecial1 = BlueProtocolBytes.BattleDefaultSpecialId,
            LastSelectSpecial2 = 0,
            LastSelectSpecial3 = 0,
            ReleaseSpecialFlg = BlueProtocolBytes.CreateBattleSpecialBitset([BlueProtocolBytes.BattleDefaultSpecialId])
        };
}
