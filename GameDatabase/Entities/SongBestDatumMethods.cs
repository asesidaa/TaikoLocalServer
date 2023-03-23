using SharedProject.Enums;

namespace GameDatabase.Entities;

public partial class SongBestDatum
{
    public void UpdateBestData(CrownType crown, uint scoreRank, uint playScore, uint scoreRate, 
        uint goodCnt, uint okCnt, uint ngCnt, uint comboCnt, uint hitCnt, uint poundCnt, DateTime lastPlayDateTime,
        short option)
    {
        if (BestCrown < crown)
        {
            BestCrown = crown;
        }

        if ((uint)BestScoreRank < scoreRank)
        {
            BestScoreRank = (ScoreRank)scoreRank;
        }

        if (BestScore < playScore)
        {
            BestScore = playScore;
            BestGoodCount = goodCnt;
            BestOkCount = okCnt;
            BestMissCount = ngCnt;
            BestComboCount = comboCnt;
            BestHitCount = hitCnt;
            BestDrumrollCount = poundCnt;
            PlayTime = lastPlayDateTime;
            Option = option;
        }

        if (BestRate < scoreRate)
        {
            BestRate = scoreRate;
        }
    }
}