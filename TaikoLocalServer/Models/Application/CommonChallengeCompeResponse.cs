namespace TaikoLocalServer.Models.Application;

public class CommonChallengeCompeResponse
{
    public uint Result { get; set; }
    public List<CommonCompeData> AryChallengeStats { get; set; } = [];
    public List<CommonCompeData> AryUserCompeStats { get; set; } = [];
    public List<CommonCompeData> AryBngCompeStats { get; set; } = [];
}

public class CommonCompeData
{
    public uint CompeId { get; set; }
    public List<CommonTracksData> AryTrackStats { get; set; } = [];
}

public class CommonTracksData
{
    public uint SongNo { get; set; }
    public uint Level { get; set; }
    public byte[] OptionFlg { get; set; } = [];
    public uint HighScore { get; set; }
}