namespace TaikoLocalServer.Entities;

public class AiSectionScoreDatum
{
    public uint Baid { get; set; }
    
    public uint SongId { get; set; }
    
    public Difficulty Difficulty { get; set; }
    
    public int SectionIndex { get; set; }
    
    public CrownType Crown { get; set; }

    public bool IsWin { get; set; }
    
    public uint Score { get; set; }
    
    public uint GoodCount { get; set; }
    
    public uint OkCount { get; set; }
    
    public uint MissCount { get; set; }

    public uint DrumrollCount { get; set; }

    public AiScoreDatum Parent { get; set; } = null!;
}