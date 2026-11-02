using Throw;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public readonly record struct BaidQuery(GameEra Era, string AccessCode) : IRequest<CommonBaidResponse>;
public readonly record struct Ac15BaidQuery(GameEra Era, string AccessCode) : IRequest<Ac15BaidResponse>;

public partial class BaidQueryHandler(
    ITaikoDbContext context,
    ILogger<BaidQueryHandler> logger,
    IGameDataCatalog gameDataService)
    : IRequestHandler<BaidQuery, CommonBaidResponse>,
      IRequestHandler<Ac15BaidQuery, Ac15BaidResponse>
{
    public async ValueTask<CommonBaidResponse> Handle(BaidQuery request, CancellationToken cancellationToken)
    {
        // Taiko.exe 39.06 (the BAIDResponse handler): any Result but 1 is kServerAccess, which card_load.tlb shows as a
        // network error; com_svr_result 300 is kDataFail instead, shown as "this card can't be used". CHN (no such
        // field) still gets the network error.
        if (await RefuseBannedAsync(request.AccessCode, cancellationToken)) return new CommonBaidResponse { Result = 0, ComSvrResult = 300 };
        await MarkScannedAsync(request.AccessCode, cancellationToken);
        return request.Era switch
        {
            GameEra.Nijiiro => await HandleNijiiro(request, cancellationToken),
            _ => throw new InvalidOperationException($"Use {nameof(Ac15BaidQuery)} for AC15 era: {request.Era}")
        };
    }

    public async ValueTask<Ac15BaidResponse> Handle(Ac15BaidQuery request, CancellationToken cancellationToken)
    {
        // shortcut: older games get a plain failure (Result 0); what their cabinets show for it isn't checked yet.
        if (await RefuseBannedAsync(request.AccessCode, cancellationToken)) return new Ac15BaidResponse { Result = 0 };
        await MarkScannedAsync(request.AccessCode, cancellationToken);
        return await HandleAc15(request, cancellationToken);
    }

    private ValueTask<Ac15BaidResponse> HandleAc15(Ac15BaidQuery request, CancellationToken cancellationToken) => request.Era switch
    {
        GameEra.Green => HandleGreen(request, cancellationToken),
        GameEra.Blue => HandleBlue(request, cancellationToken),
        GameEra.Yellow => HandleYellow(request, cancellationToken),
        GameEra.Red => HandleRed(request, cancellationToken),
        GameEra.White => HandleWhite(request, cancellationToken),
        GameEra.Murasaki => HandleMurasaki(request, cancellationToken),
        GameEra.Kimidori => HandleKimidori(request, cancellationToken),
        GameEra.Momoiro => HandleMomoiro(request, cancellationToken),
        _ => throw new InvalidOperationException($"Unsupported era: {request.Era}")
    };

    // A banned card is refused before anything else, so it can't become a new user either.
    private async Task<bool> RefuseBannedAsync(string accessCode, CancellationToken cancellationToken)
    {
        var ban = await context.BannedCards.FindAsync([accessCode], cancellationToken);
        if (ban is null || (ban.Until is not null && ban.Until <= DateTime.UtcNow)) return false;
        ban.RefusedCount++;
        await context.SaveChangesAsync(cancellationToken);
        logger.LogWarning("Refused banned card {AccessCode}", accessCode);
        return true;
    }

    private async Task MarkScannedAsync(string accessCode, CancellationToken cancellationToken)
    {
        if (await context.Cards.FindAsync([accessCode], cancellationToken) is not { } card) return;
        card.LastScanAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// The number AddMyDonEntryCommand will give a new player: after the highest player, not the highest card (a player
    /// whose cards were all removed keeps their number).
    /// </summary>
    private async Task<uint> NextBaidAsync(CancellationToken cancellationToken) =>
        await context.UserData.Select(user => user.Baid).DefaultIfEmpty().MaxAsync(cancellationToken) + 1;

    private partial ValueTask<CommonBaidResponse> HandleNijiiro(BaidQuery request, CancellationToken cancellationToken);
    private partial ValueTask<Ac15BaidResponse> HandleGreen(Ac15BaidQuery request, CancellationToken cancellationToken);
    private partial ValueTask<Ac15BaidResponse> HandleBlue(Ac15BaidQuery request, CancellationToken cancellationToken);
    private partial ValueTask<Ac15BaidResponse> HandleYellow(Ac15BaidQuery request, CancellationToken cancellationToken);
    private partial ValueTask<Ac15BaidResponse> HandleRed(Ac15BaidQuery request, CancellationToken cancellationToken);
    private partial ValueTask<Ac15BaidResponse> HandleWhite(Ac15BaidQuery request, CancellationToken cancellationToken);
    private partial ValueTask<Ac15BaidResponse> HandleMurasaki(Ac15BaidQuery request, CancellationToken cancellationToken);
    private partial ValueTask<Ac15BaidResponse> HandleKimidori(Ac15BaidQuery request, CancellationToken cancellationToken);
    private partial ValueTask<Ac15BaidResponse> HandleMomoiro(Ac15BaidQuery request, CancellationToken cancellationToken);
}
