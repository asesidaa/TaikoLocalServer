namespace TaikoLocalServer.Application.Dtos;

public class CommonChallengeCompeResponse
{
    public uint Result { get; set; } = 1;
    public List<CompeData> AryChallengeStat { get; set; } = [];
    public List<CompeData> AryUserCompeStat { get; set; } = [];
    public List<CompeData> AryBngCompeStat { get; set; } = [];

    public class CompeData
    {
        public uint CompeId { get; set; }
        public List<TracksData> AryTrackStat { get; set; } = [];
    }

    public class TracksData
    {
        public uint SongNo { get; set; }
        public uint Level { get; set; }
        public byte[] OptionFlg { get; set; } = [];
        public uint StageMode { get; set; }
        public uint HighScore { get; set; }
    }
}
