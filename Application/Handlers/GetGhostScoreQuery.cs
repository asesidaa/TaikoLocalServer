namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetGhostScoreQuery(uint Baid, uint SongNo, uint Level) : IRequest<CommonGhostScoreResponse>;

public partial class GetGhostScoreQueryHandler(
    ITaikoDbContext context,
    ILogger<GetGhostScoreQueryHandler> logger)
    : IRequestHandler<GetGhostScoreQuery, CommonGhostScoreResponse>
{
    public partial ValueTask<CommonGhostScoreResponse> Handle(GetGhostScoreQuery request, CancellationToken cancellationToken);
}
