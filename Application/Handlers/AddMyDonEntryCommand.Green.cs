namespace TaikoLocalServer.Application.Handlers;

public partial class AddMyDonEntryCommandHandler
{
    private partial async ValueTask<CommonMyDonEntryResponse> HandleGreen(
        AddMyDonEntryCommand request,
        CancellationToken cancellationToken)
    {
        var nextBaid = await context.Cards.Select(card => card.Baid)
            .DefaultIfEmpty()
            .MaxAsync(cancellationToken) + 1;

        context.UserData.Add(new UserDatum
        {
            Baid = nextBaid,
            MyDonName = request.Name,
            MyDonNameLanguage = request.Language
        });

        context.UserSaveDataGreen.Add(UserSaveDataGreenExtensions.CreateDefaultGreenSaveData(nextBaid));
        context.Cards.Add(new Card { AccessCode = request.AccessCode, Baid = nextBaid });
        context.Credentials.Add(new Credential { Baid = nextBaid, Password = string.Empty, Salt = string.Empty });

        foreach (var seed in GreenSeedDataService.CreateFakeBestSeeds(
            nextBaid,
            gameDataService.Green().MusicInfoFileOrder))
        {
            context.SongBestDataGreen.Add(seed);
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created Green user {Baid} for access code {AccessCode}", nextBaid, request.AccessCode);

        return new CommonMyDonEntryResponse
        {
            Result = 1,
            Baid = nextBaid,
            MydonName = request.Name,
            MydonNameLanguage = request.Language,
            ComSvrResult = 1,
            AccessCode = request.AccessCode
        };
    }
}
