namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;

public static class ChallengeCompeMappers
{
    public static ChallengeCompeResponse Map(CommonChallengeCompeResponse common)
    {
        var response = new ChallengeCompeResponse { Result = common.Result };
        response.AryChallengeStats.AddRange(common.AryChallengeStat.Select(MapCompe));
        response.AryUserCompeStats.AddRange(common.AryUserCompeStat.Select(MapCompe));
        response.AryBngCompeStats.AddRange(common.AryBngCompeStat.Select(MapCompe));
        return response;
    }

    private static ChallengeCompeResponse.CompeData MapCompe(CommonChallengeCompeResponse.CompeData common)
    {
        var response = new ChallengeCompeResponse.CompeData
        {
            CompeId = common.CompeId
        };
        response.AryTrackStats.AddRange(common.AryTrackStat.Select(MapTrack));
        return response;
    }

    private static ChallengeCompeResponse.CompeData.TracksData MapTrack(CommonChallengeCompeResponse.TracksData common)
        => new()
        {
            SongNo = common.SongNo,
            Level = common.Level,
            OptionFlg = common.OptionFlg,
            StageMode = common.StageMode,
            HighScore = common.HighScore
        };
}
