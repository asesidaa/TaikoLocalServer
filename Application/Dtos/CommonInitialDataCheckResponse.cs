
namespace TaikoLocalServer.Application.Dtos;

public partial class CommonInitialDataCheckResponse
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

    public uint SongHashVer { get; set; }

    public bool IsDanplay { get; set; }

    public bool IsClose { get; set; }

    public bool IsItemshop { get; set; }

    public bool IsGhostbattleplay { get; set; }

    public List<InformationData> AryGreenTelopDatas { get; set; } = [];

    public List<InformationData> AryGreenEventFolderDatas { get; set; } = [];

    public List<InformationData> AryGreenTaikojukuDatas { get; set; } = [];

    public List<InformationData> AryGreenItemShopDatas { get; set; } = [];

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

    public class InformationData
    {
        public uint InfoId { get; set; }

        public uint VerupNo { get; set; }
    }
}
