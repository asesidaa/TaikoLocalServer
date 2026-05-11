using Throw;

namespace TaikoLocalServer.Application.Handlers;

public readonly record struct GetAiDataQuery(uint Baid, GameEra Era) : IRequest<CommonAiDataResponse>;

public class GetAiDataQueryHandler : IRequestHandler<GetAiDataQuery, CommonAiDataResponse>
{
    private readonly ITaikoDbContext context;
    
    private readonly ILogger<GetAiDataQueryHandler> logger;


    public GetAiDataQueryHandler(ITaikoDbContext context, ILogger<GetAiDataQueryHandler> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    public async ValueTask<CommonAiDataResponse> Handle(GetAiDataQuery request, CancellationToken cancellationToken)
    {
        var user = await context.UserData.FirstOrDefaultAsync(datum => datum.Baid == request.Baid, cancellationToken);
        user.ThrowIfNull($"User with baid {request.Baid} does not exist!");
        var saveData = await context.GetOrCreateNijiiroSaveDataAsync(request.Baid, cancellationToken);
        var response = new CommonAiDataResponse
        {
            Result = 1,
            TotalWinnings = (uint)saveData.AiWinCount,
            InputMedian = "1",
            InputVariance = "0"
        };
        return response;
    }
}
