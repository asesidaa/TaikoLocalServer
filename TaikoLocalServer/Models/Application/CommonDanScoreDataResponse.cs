namespace TaikoLocalServer.Models.Application;

public class CommonDanScoreDataResponse
{
    public uint               Result           { get; set; }
    public List<DanScoreData> AryDanScoreDatas { get; set; } = [];
    
    public class DanScoreData
    {
        public uint                    DanId                 { get; set; }
        public uint                    ArrivalSongCnt        { get; set; }
        public uint                    SoulGaugeTotal        { get; set; }
        public uint                    ComboCntTotal         { get; set; }
        public List<DanScoreDataStage> AryDanScoreDataStages { get; set; } = [];
    }

    public class DanScoreDataStage
    {
        public uint PlayScore { get; set; }
        public uint GoodCnt   { get; set; }
        public uint OkCnt     { get; set; }
        public uint NgCnt     { get; set; }
        public uint PoundCnt  { get; set; }
        public uint HitCnt    { get; set; }
        public uint ComboCnt  { get; set; }
        public uint HighScore { get; set; }
    }
}
