using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaikoLocalServer.Adapters.AdminApi.Controllers;
using TaikoLocalServer.Contracts.AdminApi.Requests;
using TaikoLocalServer.Contracts.AdminApi.Responses;
using TaikoLocalServer.Contracts.AdminApi.ViewModels;

namespace TaikoLocalServer.Tests.Green;

public class GreenAdminApiControllerTests
{
    [Fact]
    public void ScoreFacet_CanRepresentAlternateGreenScore()
    {
        var row = new SongBestData
        {
            SongId = 101,
            Difficulty = Difficulty.Oni,
            BestScore = 900000,
            BestRate = 90,
            BestCrown = CrownType.Clear,
            BestScoreRank = ScoreRank.None,
            AlternateScore = new ScoreFacet
            {
                Label = "Shin",
                BestScore = 930000,
                BestRate = 93,
                BestCrown = CrownType.Gold
            }
        };

        Assert.Equal("Shin", row.AlternateScore.Label);
        Assert.Equal(ScoreRank.None, row.BestScoreRank);
    }
}
