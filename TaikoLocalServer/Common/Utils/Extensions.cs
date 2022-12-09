using GameDatabase.Entities;
using SharedProject.Utils;

namespace TaikoLocalServer.Common.Utils;

public static class Extensions
{
    public static void UpdateBest(this AiSectionScoreDatum datum,
        PlayResultDataRequest.StageData.AiStageSectionData sectionData)
    {
        var crown = (CrownType)sectionData.Crown;
        if (crown == CrownType.Gold && sectionData.OkCnt == 0) crown = CrownType.Dondaful;

        datum.IsWin = sectionData.IsWin ? sectionData.IsWin : datum.IsWin;
        datum.Crown = ValueHelpers.Max(crown, datum.Crown);
        datum.Score = ValueHelpers.Max(sectionData.Score, datum.Score);
        datum.GoodCount = ValueHelpers.Max(sectionData.GoodCnt, datum.GoodCount);
        datum.OkCount = ValueHelpers.Min(sectionData.OkCnt, datum.OkCount);
        datum.MissCount = ValueHelpers.Min(sectionData.NgCnt, datum.MissCount);
        datum.DrumrollCount = ValueHelpers.Max(sectionData.PoundCnt, datum.DrumrollCount);
    }
}