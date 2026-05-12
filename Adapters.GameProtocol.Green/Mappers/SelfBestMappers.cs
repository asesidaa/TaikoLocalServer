using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Green.Mappers;

[Mapper]
public static partial class SelfBestMappers
{
    public static SelfBestResponse Map(CommonSelfBestResponse common)
    {
        var response = new SelfBestResponse
        {
            Result = common.Result,
            Level = common.Level
        };

        response.ArySelfbestScores.AddRange(common.ArySelfbestScores.Select(row => new SelfBestResponse.SelfBestData
        {
            SongNo = row.SongNo,
            SelfBestScore = row.SelfBestScore,
            UraBestScore = row.UraBestScore
        }));

        response.AryShinSelfbestScores.AddRange(common.AryShinSelfbestScores.Select(row => new SelfBestResponse.SelfBestData
        {
            SongNo = row.SongNo,
            SelfBestScore = row.SelfBestScore,
            UraBestScore = row.UraBestScore
        }));

        return response;
    }
}
