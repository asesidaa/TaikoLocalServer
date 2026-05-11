namespace TaikoLocalServer.Application.Handlers;

public partial class AddMyDonEntryCommandHandler
{
    private partial async ValueTask<CommonMyDonEntryResponse> HandleNijiiro(AddMyDonEntryCommand request, CancellationToken cancellationToken)
    {
        var nextBaid = await context.Cards.Select(card => card.Baid)
            .DefaultIfEmpty()
            .MaxAsync(cancellationToken) + 1;
        var newUser = new UserDatum
        {
            Baid = nextBaid,
            MyDonName = request.Name,
            MyDonNameLanguage = request.Language,
        };
        
        context.UserData.Add(newUser);
        context.UserSaveDataNijiiro.Add(UserSaveDataNijiiroExtensions.CreateDefaultNijiiroSaveData(nextBaid));
        
        var newCard = new Card
        {
            AccessCode = request.AccessCode,
            Baid = nextBaid
        };
        context.Cards.Add(newCard);
        
        var newCredential = new Credential
        {
            Baid = nextBaid,
            Password = "",
            Salt = ""
        };
        context.Credentials.Add(newCredential);
        await context.SaveChangesAsync(cancellationToken);

        var response = new CommonMyDonEntryResponse
        {
            Result = 1,
            Baid = nextBaid,
            MydonName = request.Name,
            MydonNameLanguage = request.Language,
            ComSvrResult = 1,
            AccessCode = request.AccessCode
        };
        return response;
    }
}
