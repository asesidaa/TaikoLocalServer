namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetBattleUserDataQuery(uint Baid) : IRequest<CommonBattleUserDataResponse>;

public sealed class GetBattleUserDataQueryHandler(
    ITaikoDbContext context,
    ILogger<GetBattleUserDataQueryHandler> logger)
    : IRequestHandler<GetBattleUserDataQuery, CommonBattleUserDataResponse>
{
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

        return new CommonBattleUserDataResponse
        {
            Result = 1,
            ReleaseInfoFlg = userState?.ReleaseInfoFlg,
            ReleaseBattleStageFlg = userState?.ReleaseBattleStageFlg,
            LastBattleStageId = userState?.LastBattleStageId,
            LastBossLife = userState?.LastBossLife,
            LastNpcId = userState?.LastNpcId,
            AryTokenDatas = tokens,
            AssignStageId = userState?.AssignStageId
        };
    }
}
