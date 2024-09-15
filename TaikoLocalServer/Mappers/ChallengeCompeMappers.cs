using Riok.Mapperly.Abstractions;
using SharedProject.Models;

namespace TaikoLocalServer.Mappers;

[Mapper]
public static partial class ChallengeCompeMappers
{
    public static partial ChallengeCompeResponse MapTo3906(CommonChallengeCompeResponse response);
    public static partial Models.v3209.ChallengeCompeResponse MapTo3209(CommonChallengeCompeResponse response);
    public static partial ChallengeCompetition MapData(ChallengeCompeteDatum data);
}
