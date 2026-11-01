using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Domain.Entities;

/// <summary>The server state an admin set (Server Management › Status). One row (Id 1).</summary>
public class ServerStatus
{
    public int Id { get; set; } = 1;
    public ServerState State { get; set; }
    /// <summary>The state applies from this (UTC); null: right away.</summary>
    public DateTime? Start { get; set; }
    /// <summary>Back to Operational after this (UTC); null: until changed.</summary>
    public DateTime? End { get; set; }

    /// <summary>The state in force at <paramref name="utc"/>: outside the window it's Operational.</summary>
    public ServerState At(DateTime utc) =>
        (Start is null || Start <= utc) && (End is null || utc < End) ? State : ServerState.Operational;
}
