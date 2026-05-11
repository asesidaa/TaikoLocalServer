using Throw;

namespace TaikoLocalServer.Application.Handlers;

public partial class GetAiDataQueryHandler
{
    private partial async ValueTask<CommonAiDataResponse> HandleNijiiro(GetAiDataQuery request, CancellationToken cancellationToken)
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
