using System.Collections.Immutable;
using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Settings;

namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetInitialDataQuery(GameEra Era) : IRequest<CommonInitialDataCheckResponse>;

public partial class GetInitialDataQueryHandler(IGameDataCatalog gameDataService, 
    ILogger<GetInitialDataQueryHandler>                  logger,
    IOptions<ServerSettings>                             settings) 
    : IRequestHandler<GetInitialDataQuery, CommonInitialDataCheckResponse>
{

    private readonly ServerSettings settings = settings.Value;
    
    public ValueTask<CommonInitialDataCheckResponse> Handle(GetInitialDataQuery request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
        GameEra.Green => HandleGreen(request, cancellationToken),
        GameEra.Blue => HandleBlue(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };

    private partial ValueTask<CommonInitialDataCheckResponse> HandleNijiiro(GetInitialDataQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonInitialDataCheckResponse> HandleGreen(GetInitialDataQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonInitialDataCheckResponse> HandleBlue(GetInitialDataQuery request, CancellationToken cancellationToken);
}
