using TaikoLocalServer.Application.Dtos.Ac15;

namespace TaikoLocalServer.Application.Ac15.DonChallenge;

public sealed record Ac15DonChallengeTrackDefinition(
    uint TrackNo,
    uint SongNo,
    uint Level,
    uint StageMode,
    byte[] OptionFlg)
{
    public bool Matches(Ac15StageResult stage)
        => stage.SongNo == SongNo
           && stage.Level == Level
           && (StageMode == 2 || stage.StageMode == StageMode);
}

public static class Ac15DonChallengeTrackDefinitions
{
    public static IReadOnlyList<Ac15DonChallengeTrackDefinition> FromTask(Ac15DonChallengeTask task)
    {
        if (task.Rule.EligibleSongNoes.Count == 0)
        {
            return [];
        }

        var minimumLevel = task.Rule.MinimumLevel.GetValueOrDefault(1);
        var tracks = new List<Ac15DonChallengeTrackDefinition>();
        var trackNo = 1u;
        foreach (var songNo in task.Rule.EligibleSongNoes)
        {
            for (var level = minimumLevel; level <= 5; level++)
            {
                // RED playresult uses the 1-based ary_track_stat index as track_no.
                tracks.Add(new Ac15DonChallengeTrackDefinition(
                    trackNo++,
                    songNo,
                    level,
                    StageMode: 0,
                    OptionFlg: []));
            }
        }

        return tracks;
    }
}
