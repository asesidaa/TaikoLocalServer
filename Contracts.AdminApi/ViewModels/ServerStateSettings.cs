namespace TaikoLocalServer.Contracts.AdminApi.ViewModels;

/// <summary>The server state an admin sets; <see cref="Current"/> is the one in force now (read only).</summary>
public class ServerStateSettings
{
    public ServerState State { get; set; }
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
    public ServerState Current { get; set; }
}
