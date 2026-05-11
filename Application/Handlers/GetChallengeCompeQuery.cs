namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetChallengeCompeQuery(uint Baid) : IRequest<CommonChallengeCompeResponse>;

public partial class GetChallengeCompeQueryHandler(ILogger<GetChallengeCompeQueryHandler> logger)
    : IRequestHandler<GetChallengeCompeQuery, CommonChallengeCompeResponse>
{
    public partial ValueTask<CommonChallengeCompeResponse> Handle(GetChallengeCompeQuery request, CancellationToken cancellationToken);
}
