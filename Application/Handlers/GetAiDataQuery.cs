using Throw;

namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetAiDataQuery(uint Baid, GameEra Era) : IRequest<CommonAiDataResponse>;

public partial class GetAiDataQueryHandler : IRequestHandler<GetAiDataQuery, CommonAiDataResponse>
{
    private readonly ITaikoDbContext context;
    
    private readonly ILogger<GetAiDataQueryHandler> logger;


    public GetAiDataQueryHandler(ITaikoDbContext context, ILogger<GetAiDataQueryHandler> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    public ValueTask<CommonAiDataResponse> Handle(GetAiDataQuery request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
        GameEra.Green => HandleGreen(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };

    private partial ValueTask<CommonAiDataResponse> HandleNijiiro(GetAiDataQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonAiDataResponse> HandleGreen(GetAiDataQuery request, CancellationToken cancellationToken);
}
