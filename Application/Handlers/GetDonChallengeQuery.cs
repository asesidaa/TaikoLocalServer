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
        if (request.Era != GameEra.Red)
        {
            logger.LogInformation("Don Challenge AdminApi unavailable for era {Era}", request.Era);
            return UnavailableResponse(request.Era, $"Don Challenge is not available for {request.Era}.");
        }

        var bundle = catalog.Red().DonChallenge.ActiveBundle;
        if (bundle is null)
        {
            return UnavailableResponse(request.Era, "No active Don Challenge is configured for Red.");
        }

        var saveData = await context.UserSaveDataRed
            .AsNoTracking()
            .SingleOrDefaultAsync(row => row.Baid == request.Baid, cancellationToken);
        var progressRows = await context.RedDonChallengeProgress
            .AsNoTracking()
            .Where(row => row.Baid == request.Baid && row.BundleId == bundle.BundleId)
            .ToArrayAsync(cancellationToken);

        return Ac15DonChallengeAdminProjection.BuildResponse(
            GameEra.Red,
            bundle,
            progressRows,
            saveData is null
                ? null
                : new Ac15DonChallengeRewardFlagState(saveData.ReleaseSongFlg, saveData.TitleFlg));
    }

    private DonChallengeAvailabilityResponse BuildAvailability(GameEra era)
    {
        if (era != GameEra.Red)
        {
            return UnavailableAvailability(era, $"Don Challenge is not available for {era}.");
        }

        var bundle = catalog.Red().DonChallenge.ActiveBundle;
        return bundle is null
            ? UnavailableAvailability(era, "No active Don Challenge is configured for Red.")
            : Ac15DonChallengeAdminProjection.BuildAvailability(GameEra.Red, bundle);
    }

    private static DonChallengeAvailabilityResponse UnavailableAvailability(GameEra era, string message)
        => new()
        {
            Era = era.ToString(),
            IsAvailable = false,
            Message = message
        };

    private static DonChallengeResponse UnavailableResponse(GameEra era, string message)
        => new()
        {
            Era = era.ToString(),
            IsAvailable = false,
            Message = message
        };

}
