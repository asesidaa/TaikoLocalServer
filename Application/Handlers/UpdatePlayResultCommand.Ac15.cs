using TaikoLocalServer.Application.Ac15;

namespace TaikoLocalServer.Application.Handlers;

public partial class UpdatePlayResultCommandHandler
{
    private static bool CanAddAc15(uint current, uint delta)
        => delta <= uint.MaxValue - current;

    private static DateTime ParseAc15PlayDatetimeOrNow(string playDatetime)
        => Ac15PlayDatetime.ParseOrNow(playDatetime);
}
