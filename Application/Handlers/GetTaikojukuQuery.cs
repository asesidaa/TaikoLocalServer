namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetTaikojukuQuery(IReadOnlyList<uint> RequestedDans) : IRequest<CommonTaikojukuResponse>;

public partial class GetTaikojukuQueryHandler(ILogger<GetTaikojukuQueryHandler> logger)
    : IRequestHandler<GetTaikojukuQuery, CommonTaikojukuResponse>
{
    public partial ValueTask<CommonTaikojukuResponse> Handle(GetTaikojukuQuery request, CancellationToken cancellationToken);
}
