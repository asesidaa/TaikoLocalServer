namespace TaikoLocalServer.Application.Handlers;

public readonly record struct RewardExecutionCommand(
    uint Baid,
    IReadOnlyList<uint> ReleaseSongNoes,
    IReadOnlyList<uint> GetToneNoes,
    IReadOnlyList<uint> GetCostumeNo1s,
    IReadOnlyList<uint> GetCostumeNo2s,
    IReadOnlyList<uint> GetCostumeNo3s,
    IReadOnlyList<uint> GetCostumeNo4s,
    IReadOnlyList<uint> GetCostumeNo5s,
    IReadOnlyList<uint> GetTitleNoes
) : IRequest<CommonRewardExecutionResponse>;

public partial class RewardExecutionCommandHandler(
    ITaikoDbContext context,
    IGameDataCatalog gameDataService,
    ILogger<RewardExecutionCommandHandler> logger)
    : IRequestHandler<RewardExecutionCommand, CommonRewardExecutionResponse>
{
    public partial ValueTask<CommonRewardExecutionResponse> Handle(RewardExecutionCommand request, CancellationToken cancellationToken);
}
