using TaikoLocalServer.Application.Ac15.DonChallenge;
using TaikoLocalServer.Contracts.AdminApi.Responses;

namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetDonChallengeAvailabilityQuery(GameEra Era) : IRequest<DonChallengeAvailabilityResponse>;

public readonly record struct GetDonChallengeQuery(GameEra Era, uint Baid) : IRequest<DonChallengeResponse>;

public sealed class GetDonChallengeQueryHandler(
    ITaikoDbContext context,
    IGameDataCatalog catalog,
    ILogger<GetDonChallengeQueryHandler> logger)
    : IRequestHandler<GetDonChallengeAvailabilityQuery, DonChallengeAvailabilityResponse>,
      IRequestHandler<GetDonChallengeQuery, DonChallengeResponse>
{
    public ValueTask<DonChallengeAvailabilityResponse> Handle(
        GetDonChallengeAvailabilityQuery request,
        CancellationToken cancellationToken)
        => ValueTask.FromResult(BuildAvailability(request.Era));

    public async ValueTask<DonChallengeResponse> Handle(
        GetDonChallengeQuery request,
        CancellationToken cancellationToken)
    {
        return request.Era switch
        {
            GameEra.Red => await BuildRedResponseAsync(request.Baid, cancellationToken),
            GameEra.White => await BuildWhiteResponseAsync(request.Baid, cancellationToken),
            _ => BuildUnsupportedResponse(request.Era)
        };
    }

    private async ValueTask<DonChallengeResponse> BuildRedResponseAsync(uint baid, CancellationToken cancellationToken)
    {
        var bundle = catalog.Red().DonChallenge.ActiveBundle;
        if (bundle is null)
        {
            return UnavailableResponse(GameEra.Red, "No active Don Challenge is configured for Red.");
        }

        var saveData = await context.UserSaveDataRed
            .AsNoTracking()
            .SingleOrDefaultAsync(row => row.Baid == baid, cancellationToken);
        var progressRows = await context.RedDonChallengeProgress
            .AsNoTracking()
            .Where(row => row.Baid == baid && row.BundleId == bundle.BundleId)
            .ToArrayAsync(cancellationToken);

        return Ac15DonChallengeAdminProjection.BuildResponse(
            GameEra.Red,
            bundle,
            progressRows,
            saveData is null
                ? null
                : new Ac15DonChallengeRewardFlagState(saveData.ReleaseSongFlg, saveData.TitleFlg));
    }

    private async ValueTask<DonChallengeResponse> BuildWhiteResponseAsync(uint baid, CancellationToken cancellationToken)
    {
        var bundle = catalog.White().DonChallenge.ActiveBundle;
        if (bundle is null)
        {
            return UnavailableResponse(GameEra.White, "No active Don Challenge is configured for White.");
        }

        var saveData = await context.UserSaveDataWhite
            .AsNoTracking()
            .SingleOrDefaultAsync(row => row.Baid == baid, cancellationToken);
        var progressRows = await context.WhiteDonChallengeProgress
            .AsNoTracking()
            .Where(row => row.Baid == baid && row.BundleId == bundle.BundleId)
            .ToArrayAsync(cancellationToken);

        return Ac15DonChallengeAdminProjection.BuildResponse(
            GameEra.White,
            bundle,
            progressRows,
            saveData is null
                ? null
                : new Ac15DonChallengeRewardFlagState(saveData.ReleaseSongFlg, saveData.TitleFlg));
    }

    private DonChallengeAvailabilityResponse BuildAvailability(GameEra era)
        => era switch
        {
            GameEra.Red => BuildEraAvailability(GameEra.Red, catalog.Red().DonChallenge.ActiveBundle),
            GameEra.White => BuildEraAvailability(GameEra.White, catalog.White().DonChallenge.ActiveBundle),
            _ => UnavailableAvailability(era, $"Don Challenge is not available for {era}.")
        };

    private static DonChallengeAvailabilityResponse BuildEraAvailability(
        GameEra era,
        Ac15DonChallengeMonthlyBundle? bundle)
        => bundle is null
            ? UnavailableAvailability(era, $"No active Don Challenge is configured for {era}.")
            : Ac15DonChallengeAdminProjection.BuildAvailability(era, bundle);

    private static DonChallengeAvailabilityResponse UnavailableAvailability(GameEra era, string message)
        => new()
        {
            Era = era.ToString(),
            IsAvailable = false,
            Message = message
        };

    private DonChallengeResponse BuildUnsupportedResponse(GameEra era)
    {
        logger.LogInformation("Don Challenge AdminApi unavailable for era {Era}", era);
        return UnavailableResponse(era, $"Don Challenge is not available for {era}.");
    }

    private static DonChallengeResponse UnavailableResponse(GameEra era, string message)
        => new()
        {
            Era = era.ToString(),
            IsAvailable = false,
            Message = message
        };

}
