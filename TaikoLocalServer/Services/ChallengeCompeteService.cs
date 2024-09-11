
using GameDatabase.Context;
using SharedProject.Utils;
using Throw;

namespace TaikoLocalServer.Services;

public class ChallengeCompeteService : IChallengeCompeteService
{
    private readonly TaikoDbContext context;
    public ChallengeCompeteService(TaikoDbContext context)
    {
        this.context = context;
    }

    public bool HasChallengeCompete(uint baid)
    {
        return context.ChallengeCompeteData
            .Include(c => c.Participants)
            .Any(data =>
                data.CreateTime < DateTime.Now &&
                data.ExpireTime > DateTime.Now &&
                data.Participants.Any(participant => participant.Baid == baid && participant.IsActive)
            );
    }

    public List<ChallengeCompeteDatum> GetInProgressChallengeCompete(uint baid)
    {
        return context.ChallengeCompeteData
            .Include(c => c.Participants)
            .Where(data =>
                data.CreateTime < DateTime.Now &&
                data.ExpireTime > DateTime.Now &&
                data.Participants.Any(participant => participant.Baid == baid)
            ).ToList();
    }

    public List<ChallengeCompeteDatum> GetAllChallengeCompete()
    {
        return context.ChallengeCompeteData.Where(data => true).ToList();
    }

    public async Task CreateCompete(uint baid, ChallengeCompeteInfo challengeCompeteInfo)
    {
        ChallengeCompeteDatum challengeCompeteData = new()
        {
            CompId = context.ChallengeCompeteData.Any() ? context.ChallengeCompeteData.AsEnumerable().Max(c => c.CompId) + 1 : 1,
            CompeteMode = challengeCompeteInfo.CompeteMode,
            Baid = baid,
            CompeteName = challengeCompeteInfo.Name,
            CompeteDescribe = challengeCompeteInfo.Desc,
            MaxParticipant = challengeCompeteInfo.MaxParticipant,
            CreateTime = DateTime.Now,
            ExpireTime = DateTime.Now.AddDays(challengeCompeteInfo.LastFor),
            RequireTitle = challengeCompeteInfo.RequiredTitle,
            Share = challengeCompeteInfo.ShareType,
            CompeteTarget = challengeCompeteInfo.CompeteTargetType
        };
        await context.AddAsync(challengeCompeteData);
        foreach (var song in challengeCompeteInfo.challengeCompeteSongs)
        {
            ChallengeCompeteSongDatum challengeCompeteSongData = new()
            {
                CompId = challengeCompeteData.CompId,
                SongId = song.SongId,
                Difficulty = song.Difficulty,
                SongOpt = PlaySettingConverter.PlaySettingToShort(song.PlaySetting)
            };
            await context.AddAsync(challengeCompeteSongData);
        }
        ChallengeCompeteParticipantDatum participantDatum = new()
        {
            CompId = challengeCompeteData.CompId,
            Baid = baid,
            IsActive = true
        };
        await context.AddAsync(participantDatum);
    }

    public async Task<bool> ParticipateCompete(uint compId, uint baid)
    {
        var challengeCompete = await context.ChallengeCompeteData.FindAsync(compId);
        challengeCompete.ThrowIfNull($"Challenge not found for CompId {compId}!");

        if (challengeCompete.MaxParticipant <= challengeCompete.Participants.Count()) return false;
        foreach (var participant in challengeCompete.Participants)
        {
            if (participant.Baid == baid) return false;
        }

        ChallengeCompeteParticipantDatum participantDatum = new() 
        { 
            CompId = challengeCompete.CompId,
            Baid = baid,
            IsActive = true,
        };
        await context.AddAsync(participantDatum);

        return true;
    }

    public async Task CreateChallenge(uint baid, uint targetBaid, ChallengeCompeteInfo challengeCompeteInfo)
    {
        ChallengeCompeteDatum challengeCompeteData = new()
        {
            CompId = context.ChallengeCompeteData.Any() ? context.ChallengeCompeteData.AsEnumerable().Max(c => c.CompId) + 1 : 1,
            CompeteMode = challengeCompeteInfo.CompeteMode,
            Baid = baid,
            CompeteName = challengeCompeteInfo.Name,
            CompeteDescribe = challengeCompeteInfo.Desc,
            MaxParticipant = challengeCompeteInfo.MaxParticipant,
            CreateTime = DateTime.Now,
            ExpireTime = DateTime.Now.AddDays(challengeCompeteInfo.LastFor),
            RequireTitle = challengeCompeteInfo.RequiredTitle,
            Share = challengeCompeteInfo.ShareType,
            CompeteTarget = challengeCompeteInfo.CompeteTargetType
        };
        await context.AddAsync(challengeCompeteData);
        foreach (var song in challengeCompeteInfo.challengeCompeteSongs)
        {
            ChallengeCompeteSongDatum challengeCompeteSongData = new()
            {
                CompId = challengeCompeteData.CompId,
                SongId = song.SongId,
                Difficulty = song.Difficulty,
                SongOpt = PlaySettingConverter.PlaySettingToShort(song.PlaySetting)
            };
            await context.AddAsync(challengeCompeteSongData);
        }
        ChallengeCompeteParticipantDatum participantDatum = new()
        {
            CompId = challengeCompeteData.CompId,
            Baid = baid,
            IsActive = false
        };
        await context.AddAsync(participantDatum);
        ChallengeCompeteParticipantDatum targetDatum = new()
        {
            CompId = challengeCompeteData.CompId,
            Baid = targetBaid,
            IsActive = false
        };
        await context.AddAsync(targetDatum);
    }

    public async Task<bool> AnswerChallenge(uint compId, uint baid, bool accept)
    {
        var challengeCompete = await context.ChallengeCompeteData.FindAsync(compId);
        challengeCompete.ThrowIfNull($"Challenge not found for CompId {compId}!");

        if (challengeCompete.Baid == baid) return false;
        bool hasTarget = false;
        foreach (var participant in challengeCompete.Participants)
        {
            if (participant.Baid == baid) hasTarget = true;
        }
        if (!hasTarget) return false;

        if (accept)
        {
            foreach (var participant in challengeCompete.Participants)
            {
                participant.IsActive = true;
                context.Update(participant);
            }
        }
        else
        {
            context.Remove(challengeCompete);
        }

        return true;
    }

    public async Task UpdateBestScore(uint baid, SongPlayDatum playData, short option)
    {
        List<ChallengeCompeteDatum> challengeCompetes = context.ChallengeCompeteData
            .Include(e => e.Songs)
            .Include(e => e.Participants)
            .Where(e => e.CreateTime < DateTime.Now && DateTime.Now < e.ExpireTime)
            .Where(e => e.Participants.Any(d => d.Baid == baid && d.IsActive))
            .Where(e => e.Songs.Any(d => d.SongId == playData.SongId && d.SongOpt == option))
            .ToList();
        foreach (var challengeCompete in challengeCompetes)
        {
            ChallengeCompeteSongDatum? song = challengeCompete.Songs.Find(e => e.SongId == playData.SongId);
            if (song == null || song.Difficulty != playData.Difficulty) continue;

            ChallengeCompeteBestDatum? bestScore = song.BestScores.Find(e => e.Baid == baid);
            if (bestScore == null)
            {
                await context.AddAsync(new ChallengeCompeteBestDatum
                {
                    CompId = song.CompId,
                    Baid = baid,
                    SongId = song.SongId,
                    Difficulty = song.Difficulty,
                    Crown = playData.Crown,
                    Score = playData.Score,
                    ScoreRate = playData.ScoreRate,
                    ScoreRank = playData.ScoreRank,
                    GoodCount = playData.GoodCount,
                    OkCount = playData.OkCount,
                    MissCount = playData.MissCount,
                    ComboCount = playData.ComboCount,
                    HitCount = playData.HitCount,
                    DrumrollCount = playData.DrumrollCount,
                    Skipped = playData.Skipped
                });
            }
            else if (bestScore.Score < playData.Score)
            {
                bestScore.Crown = playData.Crown;
                bestScore.Score = playData.Score;
                bestScore.ScoreRate = playData.ScoreRate;
                bestScore.ScoreRank = playData.ScoreRank;
                bestScore.GoodCount = playData.GoodCount;
                bestScore.OkCount = playData.OkCount;
                bestScore.MissCount = playData.MissCount;
                bestScore.ComboCount = playData.ComboCount;
                bestScore.HitCount = playData.HitCount;
                bestScore.DrumrollCount = playData.DrumrollCount;
                bestScore.Skipped = playData.Skipped;
                context.Update(bestScore);
            }
        }
    }
}
