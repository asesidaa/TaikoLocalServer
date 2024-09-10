using GameDatabase.Context;
using System.Buffers.Binary;

namespace TaikoLocalServer.Handlers;

public record ChallengeCompeteQuery(uint Baid) : IRequest<ChallengeCompeResponse>;

public class ChallengeCompeteQueryHandler(TaikoDbContext context, IChallengeCompeteService challengeCompeteService, ILogger<UserDataQueryHandler> logger)
    : IRequestHandler<ChallengeCompeteQuery, ChallengeCompeResponse>
{

    public async Task<ChallengeCompeResponse> Handle(ChallengeCompeteQuery request, CancellationToken cancellationToken)
    {
        List<ChallengeCompeteDatum> competes = challengeCompeteService.GetInProgressChallengeCompete(request.Baid);
        ChallengeCompeResponse response = new()
        {
            Result = 1
        };

        foreach (var compete in competes)
        {
            ChallengeCompeResponse.CompeData compeData = new()
            {
                CompeId = compete.CompId
            };
            foreach (var song in compete.Songs)
            {
                var songOptions = new byte[2];
                BinaryPrimitives.WriteInt16LittleEndian(songOptions, song.SongOpt);

                uint myHighScore = 0;
                foreach (var bestScore in song.BestScores)
                {
                    if (bestScore.Baid == request.Baid)
                    {
                        myHighScore = bestScore.Score;
                    }
                }

                ChallengeCompeResponse.CompeData.TracksData tracksData = new()
                {
                    SongNo = song.SongId,
                    Level = (uint) song.Difficulty,
                    OptionFlg = songOptions,
                    HighScore = myHighScore
                };
                compeData.AryTrackStats.Add(tracksData);
            }
            switch (compete.CompeteMode)
            {
                case CompeteModeType.Chanllenge:
                    response.AryChallengeStats.Add(compeData);
                    break;
                case CompeteModeType.Compete:
                    response.AryUserCompeStats.Add(compeData);
                    break;
                case CompeteModeType.OfficialCompete:
                    response.AryBngCompeStats.Add(compeData);
                    break;
            }
        }

        return response;
    }
}
