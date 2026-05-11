using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Settings;

namespace TaikoLocalServer.Application.Handlers;

public readonly record struct UserDataQuery(uint Baid, GameEra Era) : IRequest<CommonUserDataResponse>;

public partial class UserDataQueryHandler(ITaikoDbContext context, IGameDataCatalog gameDataService, ILogger<UserDataQueryHandler> logger, IOptions<ServerSettings> settings) 
    : IRequestHandler<UserDataQuery, CommonUserDataResponse>
{

    private readonly ServerSettings settings = settings.Value;

    public ValueTask<CommonUserDataResponse> Handle(UserDataQuery request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
        GameEra.Green => HandleGreen(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };

    private partial ValueTask<CommonUserDataResponse> HandleNijiiro(UserDataQuery request, CancellationToken cancellationToken);
    private partial ValueTask<CommonUserDataResponse> HandleGreen(UserDataQuery request, CancellationToken cancellationToken);
}
