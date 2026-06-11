namespace TaikoLocalServer.Application.Dtos;

public partial class CommonUserDataResponse
{
    public uint Result { get; set; }

    public List<FriendInfo> AryFriendInfo { get; set; } = [];

    public uint? CategJpopCnt { get; set; }

    public uint? CategAnimeCnt { get; set; }

    public uint? CategDoyoCnt { get; set; }

    public uint? CategVarietyCnt { get; set; }

    public uint? CategClassicCnt { get; set; }

    public uint? CategGameCnt { get; set; }

    public uint? CategNamcoCnt { get; set; }

    public uint? CategVocaloidCnt { get; set; }

    public uint? SongPushedCnt { get; set; }

    public uint? SongFavoriteCnt { get; set; }

    public uint? PrevAreaCode { get; set; }

    public uint? ConsecAreaCnt { get; set; }

    public uint? RecommendSong { get; set; }

    public List<uint> RecommendBestSong { get; set; } = [];

    public bool? DefaultShinSetting { get; set; }

    public uint DispLevelTotal { get; set; }

    public uint? DispTaikojukuDan { get; set; }

    public bool? IsChallengeCompe { get; set; }

    public bool? IsTojiru { get; set; }

    public byte[] OptionFlg { get; set; } = [];

    public uint SongHashVer { get; set; }

    public class FriendInfo
    {
        public uint Baid { get; set; }

        public string FriendName { get; set; } = string.Empty;
    }
}
