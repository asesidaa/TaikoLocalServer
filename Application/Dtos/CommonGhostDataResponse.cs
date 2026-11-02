namespace TaikoLocalServer.Application.Dtos;

public class CommonGhostDataResponse
{
    public uint Result { get; set; } = 1;
    public byte[] ReleaseInfoFlag { get; set; } = [];
    public byte[] PlayedSongFlag { get; set; } = [];
    public uint TotalWinnings { get; set; }
    public GhostPerfDataInfo GhostPerfData { get; set; } = new();
    public GhostRankData GhostRecordData { get; set; } = new();
    public List<GhostTokenData> AryTokenData { get; set; } = [];

    public class GhostPerfDataInfo
    {
        public int InputMedian { get; set; }
        public uint InputVariance { get; set; }
    }

    public class GhostRankData
    {
        public uint RankId { get; set; }
        public uint WinPoint { get; set; }
        public uint CertifiedLevelId { get; set; }
        public List<GhostWinningsData> AryWinningsData { get; set; } = [];
    }

    public class GhostWinningsData
    {
        public uint LevelId { get; set; }
        public uint Winnings { get; set; }
    }

    public class GhostTokenData
    {
        public uint TokenId { get; set; }
        public uint TokenValue { get; set; }
    }
}
