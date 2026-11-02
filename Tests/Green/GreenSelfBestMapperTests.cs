using TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

namespace TaikoLocalServer.Tests.Green;

public sealed class GreenSelfBestMapperTests
{
    [Fact]
    public void Map_GreenSelfBest_FillsNormalAndShinRepeatedFields()
    {
        var common = new CommonSelfBestResponse
        {
            Result = 1,
            Level = 1,
            ArySelfbestScores =
            [
                new() { SongNo = 873, SelfBestScore = 234560, UraBestScore = 0 }
            ],
            AryShinSelfbestScores =
            [
                new() { SongNo = 873, SelfBestScore = 0, UraBestScore = 0 }
            ]
        };

        var response = SelfBestMappers.Map(common);

        Assert.Equal((uint)1, response.Result);
        Assert.Equal((uint)1, response.Level);
        Assert.Single(response.ArySelfbestScores);
        Assert.Single(response.AryShinSelfbestScores);
        Assert.Equal((uint)873, response.ArySelfbestScores[0].SongNo);
        Assert.Equal((uint)873, response.AryShinSelfbestScores[0].SongNo);
        Assert.Equal((uint)234560, response.ArySelfbestScores[0].SelfBestScore);
        Assert.Equal((uint)0, response.AryShinSelfbestScores[0].SelfBestScore);
    }
}
