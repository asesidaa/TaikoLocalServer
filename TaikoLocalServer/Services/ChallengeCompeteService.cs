
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
}
