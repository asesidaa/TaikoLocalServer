namespace TaikoLocalServer.Application.Handlers;

public partial class AddMyDonEntryCommandHandler
{
    private partial async ValueTask<CommonMyDonEntryResponse> HandleYellow(
        AddMyDonEntryCommand request,
        CancellationToken cancellationToken)
    {
        var existingCard = await context.Cards.FindAsync([request.AccessCode], cancellationToken);
        var baid = existingCard?.Baid
            ?? await context.Cards.Select(card => card.Baid)
                .DefaultIfEmpty()
                .MaxAsync(cancellationToken) + 1;

        var userData = await context.UserData.FindAsync([baid], cancellationToken);
        if (userData is null)
        {
            context.UserData.Add(new UserDatum
            {
                Baid = baid,
                MyDonName = request.Name,
                MyDonNameLanguage = request.Language
            });
        }
        else
        {
            userData.MyDonName = request.Name;
            userData.MyDonNameLanguage = request.Language;
        }

        if (await context.UserSaveDataYellow.FindAsync([baid], cancellationToken) is null)
        {
            context.UserSaveDataYellow.Add(UserSaveDataYellowExtensions.CreateDefaultYellowSaveData(baid));
        }

        if (existingCard is null)
        {
            context.Cards.Add(new Card { AccessCode = request.AccessCode, Baid = baid });
        }

        if (await context.Credentials.FindAsync([baid], cancellationToken) is null)
        {
            context.Credentials.Add(new Credential { Baid = baid, Password = string.Empty, Salt = string.Empty });
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created Yellow user {Baid} for access code {AccessCode}", baid, request.AccessCode);

        return new CommonMyDonEntryResponse
        {
            Result = 1,
            Baid = baid,
            MydonName = request.Name,
            MydonNameLanguage = request.Language,
            ComSvrResult = 1,
            AccessCode = request.AccessCode
        };
    }
}
