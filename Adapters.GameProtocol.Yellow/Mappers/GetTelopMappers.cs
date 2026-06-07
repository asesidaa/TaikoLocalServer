namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;

public static class GetTelopMappers
{
    public static GettelopResponse Map(CommonGetTelopResponse common)
    {
        var response = new GettelopResponse { Result = common.Result };

        if (common.VerupNo is { } verupNo)
        {
            response.VerupNo = verupNo;
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
