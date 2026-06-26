using Microsoft.Extensions.Options;
using TaikoLocalServer.Application.Dtos.Ac15;
using TaikoLocalServer.Application.Settings;

namespace TaikoLocalServer.Application.Handlers;

public readonly record struct UserDataQuery(uint Baid, GameEra Era) : IRequest<CommonUserDataResponse>;
public readonly record struct Ac15UserDataQuery(uint Baid, GameEra Era) : IRequest<Ac15UserDataResponse>;

public partial class UserDataQueryHandler(ITaikoDbContext context, IGameDataCatalog gameDataService, ILogger<UserDataQueryHandler> logger, IOptions<ServerSettings> settings) 
    : IRequestHandler<UserDataQuery, CommonUserDataResponse>,
      IRequestHandler<Ac15UserDataQuery, Ac15UserDataResponse>
{

    private readonly ServerSettings settings = settings.Value;

    public ValueTask<CommonUserDataResponse> Handle(UserDataQuery request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Nijiiro => HandleNijiiro(request, cancellationToken),
        GameEra.Green or GameEra.Blue or GameEra.Yellow or GameEra.Red or GameEra.White or GameEra.Murasaki or GameEra.Kimidori or GameEra.Momoiro => throw new InvalidOperationException($"Use {nameof(Ac15UserDataQuery)} for AC15 userdata era {request.Era}."),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };

    public ValueTask<Ac15UserDataResponse> Handle(Ac15UserDataQuery request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Green => HandleGreen(request, cancellationToken),
        GameEra.Blue => HandleBlue(request, cancellationToken),
        GameEra.Yellow => HandleYellow(request, cancellationToken),
        GameEra.Red => HandleRed(request, cancellationToken),
        GameEra.White => HandleWhite(request, cancellationToken),
        GameEra.Murasaki => HandleMurasaki(request, cancellationToken),
        GameEra.Kimidori => HandleKimidori(request, cancellationToken),
        GameEra.Momoiro => HandleMomoiro(request, cancellationToken),
        GameEra.Nijiiro => throw new InvalidOperationException($"Use {nameof(UserDataQuery)} for Nijiiro userdata."),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };

    private partial ValueTask<CommonUserDataResponse> HandleNijiiro(UserDataQuery request, CancellationToken cancellationToken);
    private partial ValueTask<Ac15UserDataResponse> HandleGreen(Ac15UserDataQuery request, CancellationToken cancellationToken);
    private partial ValueTask<Ac15UserDataResponse> HandleBlue(Ac15UserDataQuery request, CancellationToken cancellationToken);
    private partial ValueTask<Ac15UserDataResponse> HandleYellow(Ac15UserDataQuery request, CancellationToken cancellationToken);
    private partial ValueTask<Ac15UserDataResponse> HandleRed(Ac15UserDataQuery request, CancellationToken cancellationToken);
    private partial ValueTask<Ac15UserDataResponse> HandleWhite(Ac15UserDataQuery request, CancellationToken cancellationToken);
    private partial ValueTask<Ac15UserDataResponse> HandleMurasaki(Ac15UserDataQuery request, CancellationToken cancellationToken);
    private partial ValueTask<Ac15UserDataResponse> HandleKimidori(Ac15UserDataQuery request, CancellationToken cancellationToken);
    private partial ValueTask<Ac15UserDataResponse> HandleMomoiro(Ac15UserDataQuery request, CancellationToken cancellationToken);
}
