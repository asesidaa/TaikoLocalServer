using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Adapters.GameProtocol.Shared.Cabinets;

public static class HeartbeatStat
{
    /// <summary>
    /// The heartbeat's game_svr_stat for the state in force. From the 39.06 exe's HeartBeatChecker (com_svr_stat must be
    /// 1): 1 OK, 2 maintenance, 3 NG (network error), 4 pre-maintenance. CHN is assumed to read it the same way.
    /// </summary>
    public static uint GameSvrStat(this IServerAccess access) => access.Status.At(DateTime.UtcNow) switch
    {
        ServerState.PreMaintenance => 4,
        ServerState.Maintenance => 2,
        ServerState.Offline => 3,
        _ => 1,
    };
}
