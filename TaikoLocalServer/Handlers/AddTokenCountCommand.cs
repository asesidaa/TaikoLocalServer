using GameDatabase.Context;
using GameDatabase.Entities;
using TaikoLocalServer.Models.Application;
using Throw;

namespace TaikoLocalServer.Handlers;

public record AddTokenCountCommand(CommonAddTokenCountRequest Request) : IRequest;

public class AddTokenCountCommandHandler : IRequestHandler<AddTokenCountCommand>
{
    private readonly TaikoDbContext context;

    private readonly ILogger<AddTokenCountCommandHandler> logger;

    public AddTokenCountCommandHandler(TaikoDbContext context, ILogger<AddTokenCountCommandHandler> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    public async Task Handle(AddTokenCountCommand command, CancellationToken cancellationToken)
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

        context.Update(user);
        await context.SaveChangesAsync(cancellationToken);
    }
}