using TaikoLocalServer.Application.Dtos;
using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Ac15;

public static class Ac15RecommendationService
{
    public static CommonRecommendResponse BuildRecommendResponse(IReadOnlyList<uint> songNoes)
        => new()
        {
            Result = 1,
            RecommendSong = PickRecommendSong(songNoes)
        };

    public static Ac15UserDataRecommendations BuildUserDataRecommendations(IReadOnlyList<uint> songNoes)
        => new()
        {
            RecommendSong = PickRecommendSong(songNoes)
        };

    private static uint PickRecommendSong(IReadOnlyList<uint> songNoes)
    {
        var candidates = songNoes.Where(songNo => songNo > 0).ToArray();
        return candidates.Length == 0
            ? 0u
            : candidates[Random.Shared.Next(candidates.Length)];
    }
}
