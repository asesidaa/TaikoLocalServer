
namespace TaikoLocalServer.Application.Handlers;
public readonly record struct AddMyDonEntryCommand(GameEra Era, string AccessCode, string Name, uint Language) : IRequest<CommonMyDonEntryResponse>;

#pragma warning disable CS9113 // Parameter is unread.
public class AddMyDonEntryCommandHandler(ITaikoDbContext context, ILogger<AddMyDonEntryCommandHandler> logger)
#pragma warning restore CS9113 // Parameter is unread.
    : IRequestHandler<AddMyDonEntryCommand, CommonMyDonEntryResponse>
{
    public async ValueTask<CommonMyDonEntryResponse> Handle(AddMyDonEntryCommand request, CancellationToken cancellationToken)
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
