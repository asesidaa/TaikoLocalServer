namespace TaikoLocalServer.Domain.Entities;

public class GreenFriends
{
    public uint Baid { get; set; }
    public uint FriendBaid { get; set; }
    public string FriendName { get; set; } = string.Empty;
    public virtual UserDatum? Ba { get; set; }
}
