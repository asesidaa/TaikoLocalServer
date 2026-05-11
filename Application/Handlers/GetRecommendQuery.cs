namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetRecommendQuery(uint GenderType, uint PlayerAge) : IRequest<CommonRecommendResponse>;

public partial class GetRecommendQueryHandler(ILogger<GetRecommendQueryHandler> logger)
    : IRequestHandler<GetRecommendQuery, CommonRecommendResponse>
{
    public partial ValueTask<CommonRecommendResponse> Handle(GetRecommendQuery request, CancellationToken cancellationToken);
}
