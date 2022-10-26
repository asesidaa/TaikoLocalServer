using SharedProject.Enums;

namespace GameDatabase.Entities;

public partial class SongBestDatum
{
    public void UpdateBestData(CrownType crown, uint scoreRank, uint playScore, uint scoreRate)
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
        }
            
        if (BestRate < scoreRate)
        {
            BestRate = scoreRate;
        }
    }
}