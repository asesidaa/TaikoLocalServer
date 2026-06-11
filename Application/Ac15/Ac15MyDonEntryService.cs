namespace TaikoLocalServer.Application.Ac15;

public static class Ac15MyDonEntryService
{
    public static async ValueTask<CommonMyDonEntryResponse> HandleAsync<TSave>(
        ITaikoDbContext context,
        string accessCode,
        string name,
        uint language,
        DbSet<TSave> saveDataSet,
        Func<uint, TSave> createDefaultSaveData,
        ILogger logger,
        string eraName,
        CancellationToken cancellationToken)
        where TSave : class
    {
        var existingCard = await context.Cards.FindAsync([accessCode], cancellationToken);
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
                MyDonName = name,
                MyDonNameLanguage = language
            });
        }
        else
        {
            userData.MyDonName = name;
            userData.MyDonNameLanguage = language;
        }

        if (await saveDataSet.FindAsync([baid], cancellationToken) is null)
        {
            saveDataSet.Add(createDefaultSaveData(baid));
        }

        if (existingCard is null)
        {
            context.Cards.Add(new Card { AccessCode = accessCode, Baid = baid });
        }

        if (await context.Credentials.FindAsync([baid], cancellationToken) is null)
        {
            context.Credentials.Add(new Credential { Baid = baid, Password = string.Empty, Salt = string.Empty });
        }

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created {Era} user {Baid} for access code {AccessCode}", eraName, baid, accessCode);

        return new CommonMyDonEntryResponse
        {
            Result = 1,
            Baid = baid,
            MydonName = name,
            MydonNameLanguage = language,
            ComSvrResult = 1,
            AccessCode = accessCode
        };
    }
}
