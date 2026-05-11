using Throw;

namespace TaikoLocalServer.Application.Handlers;

public partial class AddTokenCountCommandHandler
{
    private partial async ValueTask<Unit> HandleNijiiro(AddTokenCountCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var user = await context.UserData
            .Include(userDatum => userDatum.Tokens)
            .FirstOrDefaultAsync(datum => datum.Baid == request.Baid, cancellationToken);
        user.ThrowIfNull($"User with baid {request.Baid} does not exist!");

        foreach (var addTokenCountData in request.AryAddTokenCountDatas)
        {
            var tokenId = addTokenCountData.TokenId;
            var addTokenCount = addTokenCountData.AddTokenCount;
            var token = user.Tokens.FirstOrDefault(t => t.Id == tokenId);
            if (token is not null)
            {
                token.Count += addTokenCount;
            }
            else
            {
                user.Tokens.Add(new Token
                {
                    Baid = user.Baid,
                    Id = (int)tokenId,
                    Count = addTokenCount
                });
            }
        }

        context.UserData.Update(user);
        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
