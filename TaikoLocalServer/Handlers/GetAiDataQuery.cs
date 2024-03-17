using GameDatabase.Context;
using Throw;

namespace TaikoLocalServer.Handlers;

public record GetAiDataQuery(uint Baid) : IRequest<CommonAiDataResponse>;

public class GetAiDataQueryHandler : IRequestHandler<GetAiDataQuery, CommonAiDataResponse>
{
    private readonly TaikoDbContext context;
    
    private readonly ILogger<GetAiDataQueryHandler> logger;


    public GetAiDataQueryHandler(TaikoDbContext context, ILogger<GetAiDataQueryHandler> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    public async Task<CommonAiDataResponse> Handle(GetAiDataQuery request, CancellationToken cancellationToken)
    {
        var user = await context.UserData.FirstOrDefaultAsync(datum => datum.Baid == request.Baid);
        user.ThrowIfNull($"User with baid {request.Baid} does not exist!");
        var response = new CommonAiDataResponse
        {
            Result = 1,
            TotalWinnings = (uint)user.AiWinCount,
            InputMedian = "1",
            InputVariance = "0"
        };
        return response;
    }
}
