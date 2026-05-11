namespace TaikoLocalServer.Application.Dtos;

public class CommonGhostScoreResponse
{
    public uint Result { get; set; } = 1;
    public List<GhostBestSectionData> AryBestSectionData { get; set; } = [];

    public class GhostBestSectionData
    {
        public uint SectionNo { get; set; }
        public uint GoodCnt { get; set; }
        public uint OkCnt { get; set; }
        public uint NgCnt { get; set; }
        public uint PoundCnt { get; set; }
    }
}
