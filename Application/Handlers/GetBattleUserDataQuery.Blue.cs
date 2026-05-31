using System.Globalization;

namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetBattleUserDataQuery(uint Baid) : IRequest<CommonBattleUserDataResponse>;

public sealed class GetBattleUserDataQueryHandler(
    ITaikoDbContext context,
    ILogger<GetBattleUserDataQueryHandler> logger)
    : IRequestHandler<GetBattleUserDataQuery, CommonBattleUserDataResponse>
{
    private const int BattleNpcCostumeBytes = 4;
    private const int BattleNpcSpecialBytes = 16;

    public async ValueTask<CommonBattleUserDataResponse> Handle(
        GetBattleUserDataQuery request,
        CancellationToken cancellationToken)
    {
        _ = logger;
        _ = await context.UserData.FindAsync([request.Baid], cancellationToken)
            ?? throw new InvalidOperationException($"User not found for Blue battleuserdata baid {request.Baid}.");

        var userState = await context.BlueBattleUserStates
            .AsNoTracking()
            .SingleOrDefaultAsync(row => row.Baid == request.Baid, cancellationToken);
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
        var npcStates = await context.BlueBattleNpcStates
            .AsNoTracking()
            .Where(row => row.Baid == request.Baid
                          && row.TotalExp != null
                          && row.MaxDpn != null
                          && row.NpcCostumeId != null
                          && row.NpcCostumeFlg != null
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
                NpcCostumeFlg = BlueProtocolBytes.FixedOrZero(row.NpcCostumeFlg, BattleNpcCostumeBytes),
                LastSelectSpecial1 = row.SelectedSpecialId1!.Value,
                LastSelectSpecial2 = row.SelectedSpecialId2!.Value,
                LastSelectSpecial3 = row.SelectedSpecialId3!.Value,
                ReleaseSpecialFlg = row.ReleaseSpecialFlg is null
                    ? null
                    : BlueProtocolBytes.FixedOrZero(row.ReleaseSpecialFlg, BattleNpcSpecialBytes)
            })
            .ToList();

        return new CommonBattleUserDataResponse
        {
            Result = 1,
            ReleaseInfoFlg = userState?.ReleaseInfoFlg,
            ReleaseBattleStageFlg = userState?.ReleaseBattleStageFlg,
            LastBattleStageId = userState?.LastBattleStageId,
            LastBossLife = userState?.LastBossLife,
            LastNpcId = userState?.LastNpcId,
            NpcDatas = npcs,
            AryTokenDatas = tokens,
            AssignStageId = userState?.AssignStageId
        };
    }
}
