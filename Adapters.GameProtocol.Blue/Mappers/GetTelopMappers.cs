namespace TaikoLocalServer.Adapters.GameProtocol.Blue.Mappers;

public static class GetTelopMappers
{
    public static GettelopResponse Map(CommonGetTelopResponse common)
    {
        var response = new GettelopResponse { Result = common.Result };

        if (common.VerupNo is { } verup)
        {
            response.VerupNo = verup;
        }

        if (!string.IsNullOrEmpty(common.StartDatetime))
        {
            response.StartDatetime = common.StartDatetime;
        }

        if (!string.IsNullOrEmpty(common.EndDatetime))
        {
            response.EndDatetime = common.EndDatetime;
        }

        if (!string.IsNullOrEmpty(common.Telop))
        {
            response.Telop = common.Telop;
        }

        return response;
    }
}
