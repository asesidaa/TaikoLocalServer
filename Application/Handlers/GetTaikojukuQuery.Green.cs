namespace TaikoLocalServer.Application.Handlers;

public partial class GetTaikojukuQueryHandler
{
    public partial ValueTask<CommonTaikojukuResponse> Handle(GetTaikojukuQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Green Taikojuku stub for {Count} requested dans, returning empty", request.RequestedDans.Count);
        return ValueTask.FromResult(new CommonTaikojukuResponse());
    }
}
