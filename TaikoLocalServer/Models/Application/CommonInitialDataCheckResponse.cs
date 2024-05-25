using SharedProject.Models;

namespace TaikoLocalServer.Models.Application;

public class CommonInitialDataCheckResponse
{
    public uint   Result             { get; set; }
    public byte[] DefaultSongFlg     { get; set; } = [];
    public byte[] AchievementSongBit { get; set; } = [];
    public byte[] UraReleaseBit      { get; set; } = [];

    public string SongIntroductionEndDatetime { get; set; } =
        DateTime.Now.AddYears(10).ToString(Constants.DateTimeFormat);

    public List<MovieData>    AryMovieInfoes        { get; set; } = [];
    public List<AiEventData>  AryAiEventDatas       { get; set; } = [];
    public List<VerupNoData1> AryVerupNoData1s      { get; set; } = [];
    public List<VerupNoData2> AryVerupNoData2s      { get; set; } = [];
    public uint[]             AryChassisFunctionIds { get; set; } = [1, 2, 3];
    
    public ulong ServerCurrentDatetime { get; set; } = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds();

    public class AiEventData
    {
        public uint AiEventId { get; set; }
        public uint TokenId   { get; set; }
    }

    public class VerupNoData1
    {
        public uint MasterType { get; set; }
        public uint VerupNo    { get; set; }
    }

    public class VerupNoData2
    {
        public uint                  MasterType          { get; set; }
        public List<InformationData> AryInformationDatas { get; set; } = [];

        public class InformationData
        {
            public uint InfoId  { get; set; }
            public uint VerupNo { get; set; }
        }
    }
}