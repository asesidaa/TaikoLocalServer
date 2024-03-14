using GameDatabase.Context;
using GameDatabase.Entities;
using TaikoLocalServer.Models.Application;

namespace TaikoLocalServer.Handlers;
public record AddMyDonEntryCommand(string AccessCode, string Name, uint Language) : IRequest<CommonMyDonEntryResponse>;

public class AddMyDonEntryCommandHandler(TaikoDbContext context, ILogger<AddMyDonEntryCommandHandler> logger)
    : IRequestHandler<AddMyDonEntryCommand, CommonMyDonEntryResponse>
{
    public async Task<CommonMyDonEntryResponse> Handle(AddMyDonEntryCommand request, CancellationToken cancellationToken)
    {
        var nextBaid = await context.Cards.Select(card => card.Baid)
            .DefaultIfEmpty(0u)
            .MaxAsync(cancellationToken) + 1;
        var newUser = new UserDatum
        {
            Baid = nextBaid,
            MyDonName = request.Name,
            MyDonNameLanguage = request.Language,
            DisplayDan = true,
            DisplayAchievement = true,
            AchievementDisplayDifficulty = Difficulty.None,
            ColorFace = 0,
            ColorBody = 1,
            ColorLimb = 3,
            FavoriteSongsArray = [],
            ToneFlgArray = [0],
            TitleFlgArray = [],
            CostumeFlgArray = "[[0],[0],[0],[0],[0]]",
            GenericInfoFlgArray = [],
            UnlockedSongIdList = []
        };
        
        context.UserData.Add(newUser);
        
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