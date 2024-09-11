﻿using GameDatabase.Context;
using SharedProject.Utils;
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
                short songOpt = PlaySettingConverter.PlaySettingToShort(new()
                {
                    Speed = song.Speed != null ? (uint)song.Speed : 0,
                    IsVanishOn = song.IsVanishOn != null ? (bool)song.IsVanishOn : false,
                    IsInverseOn = song.IsInverseOn != null ? (bool)song.IsInverseOn : false,
                    RandomType = song.RandomType != null ? (RandomType)song.RandomType : RandomType.None,
                });
                BinaryPrimitives.WriteInt16LittleEndian(songOptions, songOpt);

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
