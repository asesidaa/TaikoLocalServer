namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetGhostDataQuery(uint Baid) : IRequest<CommonGhostDataResponse>;

public partial class GetGhostDataQueryHandler(ILogger<GetGhostDataQueryHandler> logger)
    : IRequestHandler<GetGhostDataQuery, CommonGhostDataResponse>
{
    public partial ValueTask<CommonGhostDataResponse> Handle(GetGhostDataQuery request, CancellationToken cancellationToken);
}
