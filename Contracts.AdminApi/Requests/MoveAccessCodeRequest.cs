namespace TaikoLocalServer.Contracts.AdminApi.Requests;

public class MoveAccessCodeRequest
{
    /// <summary>The user the access code moves to. Scores already played stay with the old user.</summary>
    public uint ToBaid { get; set; }
}
