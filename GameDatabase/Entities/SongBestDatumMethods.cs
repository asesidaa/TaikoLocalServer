using SharedProject.Enums;

namespace GameDatabase.Entities;

public partial class SongBestDatum
{
    public void UpdateBestData(CrownType crown, uint scoreRank, uint playScore, uint scoreRate, short option)
    {
        if (BestCrown < crown)
        {
            BestCrown = crown;
            Option = option;
        }

        if ((uint)BestScoreRank < scoreRank)
        {
            BestScoreRank = (ScoreRank)scoreRank;
            Option = option;
        }

        if (BestScore < playScore)
        {
            BestScore = playScore;
            Option = option;
        }

        if (BestRate < scoreRate)
        {
            BestRate = scoreRate;
            Option = option;
        }
    }
}