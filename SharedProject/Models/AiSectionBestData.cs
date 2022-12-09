using SharedProject.Enums;

namespace SharedProject.Models;

public class AiSectionBestData
{
    public int SectionIndex { get; set; }

    public CrownType Crown { get; set; }

    public bool IsWin { get; set; }

    public uint Score { get; set; }

    public uint GoodCount { get; set; }

    public uint OkCount { get; set; }

    public uint MissCount { get; set; }

    public uint DrumrollCount { get; set; }
}