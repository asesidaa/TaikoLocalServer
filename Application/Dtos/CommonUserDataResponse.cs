namespace TaikoLocalServer.Application.Dtos;

public partial class CommonUserDataResponse
{
    public uint Result { get; set; }

    public List<FriendInfo> AryFriendInfo { get; set; } = [];

    public class FriendInfo
    {
        public uint Baid { get; set; }

        public string FriendName { get; set; } = string.Empty;
    }
}
