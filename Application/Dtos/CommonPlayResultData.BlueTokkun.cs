// ReSharper disable InconsistentNaming
namespace TaikoLocalServer.Application.Dtos;

public partial class CommonPlayResultData
{
    public bool IsTokkunPlayResult { get; set; }

    public uint? TokkunTutorialFlg { get; set; }

    public TokkunStageDataDto? TokkunStageData { get; set; }

    public class TokkunStageDataDto
    {
        public string BanacoinDatetime { get; set; } = string.Empty;

        public uint TokkunSongCnt { get; set; }

        public List<uint> TookunSongnoes { get; set; } = [];

        public uint TokkunSpeedchangeCnt { get; set; }

        public uint TokkunAutoplayCnt { get; set; }

        public uint TokkunJumpCnt { get; set; }
    }
}
