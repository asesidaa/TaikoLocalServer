namespace TaikoLocalServer.Models.Application;

public class CommonSelfBestResponse
{
    public uint Result { get; set; }

    public uint Level { get; set; }

    public List<SelfBestData> ArySelfbestScores { get; set; } = [];

    public class SelfBestData
    {
        public uint SongNo        { get; set; }
        public uint SelfBestScore { get; set; }
        public uint UraBestScore  { get; set; }
        public uint SelfBestScoreRate { get; set; }
        public uint UraBestScoreRate  { get; set; }
    }
}