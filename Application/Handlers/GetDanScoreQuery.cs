using Throw;

namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetDanScoreQuery(uint Baid, GameEra Era, uint Type, uint[] DanIds) : IRequest<CommonDanScoreDataResponse>;

public partial class GetDanScoreQueryHandler : IRequestHandler<GetDanScoreQuery, CommonDanScoreDataResponse>
{
    private readonly ILogger<GetDanScoreQueryHandler> logger;
    private readonly ITaikoDbContext                   context;


    public GetDanScoreQueryHandler(ILogger<GetDanScoreQueryHandler> logger, ITaikoDbContext context)
    {
        this.logger = logger;
        this.context = context;
    }

    public ValueTask<CommonDanScoreDataResponse> Handle(GetDanScoreQuery request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
        GameEra.Green => HandleGreen(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };

    private partial ValueTask<CommonDanScoreDataResponse> HandleNijiiro(GetDanScoreQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonDanScoreDataResponse> HandleGreen(GetDanScoreQuery request, CancellationToken cancellationToken);
}


