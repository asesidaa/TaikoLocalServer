namespace TaikoLocalServer.Models.Application;

public class CommonAiScoreResponse
{
    public List<CommonAiBestSectionData> AryBestSectionDatas { get; set; } = new();
}

public class CommonAiBestSectionData
{
    public uint SectionIndex { get; set; }
    public uint Crown { get; set; }
    public uint Score { get; set; }
    public uint GoodCount { get; set; }
    public uint OkCount { get; set; }
    public uint MissCount { get; set; }
    public uint DrumrollCount { get; set; }
}