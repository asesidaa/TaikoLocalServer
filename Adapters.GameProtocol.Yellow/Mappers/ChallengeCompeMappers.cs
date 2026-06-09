using Riok.Mapperly.Abstractions;

namespace TaikoLocalServer.Adapters.GameProtocol.Yellow.Mappers;

[Mapper]
public static partial class ChallengeCompeMappers
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
        var response = MapCompeCore(common);
        response.AryTrackStats.AddRange(common.AryTrackStat.Select(MapTrack));
        return response;
    }

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial ChallengeCompeResponse.CompeData MapCompeCore(
        CommonChallengeCompeResponse.CompeData common);

    private static partial ChallengeCompeResponse.CompeData.TracksData MapTrack(
        CommonChallengeCompeResponse.TracksData common);
}
