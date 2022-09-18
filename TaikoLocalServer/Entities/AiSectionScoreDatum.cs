using SharedProject.Utils;

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

    public void UpdateBest(PlayResultDataRequest.StageData.AiStageSectionData sectionData)
    {
        var crown = (CrownType)sectionData.Crown;
        if (crown == CrownType.Gold && sectionData.OkCnt == 0)
        {
            crown = CrownType.Dondaful;
        }
        
        IsWin = sectionData.IsWin ? sectionData.IsWin : IsWin;
        Crown = ValueHelpers.Max(crown, Crown);
        Score = ValueHelpers.Max(sectionData.Score, Score);
        GoodCount = ValueHelpers.Max(sectionData.GoodCnt, GoodCount);
        OkCount = ValueHelpers.Min(sectionData.OkCnt, OkCount);
        MissCount = ValueHelpers.Min(sectionData.NgCnt, MissCount);
        DrumrollCount = ValueHelpers.Max(sectionData.PoundCnt, DrumrollCount);
    }
}