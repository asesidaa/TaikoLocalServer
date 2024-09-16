
using GameDatabase.Context;
using SharedProject.Models;
using SharedProject.Models.Responses;
using SharedProject.Utils;
using Throw;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TaikoLocalServer.Services;

public class ChallengeCompeteService : IChallengeCompeteService
{
    private readonly TaikoDbContext context;
    private readonly IGameDataService gameDataService;
    private readonly IUserDatumService userDatumService;

    private Dictionary<uint, MusicDetail> musicDetailDict;
    public ChallengeCompeteService(TaikoDbContext context, IGameDataService gameDataService, IUserDatumService userDatumService)
    {
        this.context = context;
        this.gameDataService = gameDataService;
        this.userDatumService = userDatumService;

        this.musicDetailDict = gameDataService.GetMusicDetailDictionary();
    }

    public async Task<bool> HasChallengeCompete(uint baid)
    {
        return await context.ChallengeCompeteData
            .Include(c => c.Participants)
            .Include(c => c.Songs)
                .ThenInclude(s => s.BestScores)
            .AnyAsync(data =>
                data.State == CompeteState.Normal &&
                data.CreateTime < DateTime.Now &&
                data.ExpireTime > DateTime.Now &&
                data.Participants.Any(participant => participant.Baid == baid && participant.IsActive) &&
                (
                    // Only Play Once need there is no Score for current Compete
                    !data.OnlyPlayOnce || data.Songs.Any(song => !song.BestScores.Any(s => s.Baid == baid))
                )
            );
    }

    public async Task<List<ChallengeCompeteDatum>> GetInProgressChallengeCompete(uint baid)
    {
        return await context.ChallengeCompeteData
            .Include(c => c.Participants)
            .Include(c => c.Songs)
                .ThenInclude(s => s.BestScores)
            .Where(data =>
                data.State == CompeteState.Normal &&
                data.CreateTime < DateTime.Now &&
                data.ExpireTime > DateTime.Now &&
                data.Participants.Any(participant => participant.Baid == baid && participant.IsActive) &&
                (
                    // Only Play Once need there is no Score for current Compete
                    !data.OnlyPlayOnce || data.Songs.Any(song => !song.BestScores.Any(s => s.Baid == baid))
                )
            ).ToListAsync();
    }

    public async Task<List<ChallengeCompeteDatum>> GetAllChallengeCompete()
    {
        return await context.ChallengeCompeteData
            .Include(c => c.Participants)
            .Include(c => c.Songs)
                .ThenInclude(s => s.BestScores)
            .Where(data => true).ToListAsync();
    }

    public async Task<ChallengeCompetitionResponse> GetChallengeCompetePage(CompeteModeType mode, uint baid, bool inProgress, int page, int limit, string? search)
    {
        IQueryable<ChallengeCompeteDatum>? query = null;
        string? lowSearch = search != null ? search.ToLower() : null;
        if (mode == CompeteModeType.Chanllenge) 
        {
            query = context.ChallengeCompeteData
                .Include(e => e.Songs).ThenInclude(e => e.BestScores).Include(e => e.Participants)
                .Where(e => e.CompeteMode == CompeteModeType.Chanllenge)
                .Where(e => inProgress == false || (e.CreateTime < DateTime.Now && DateTime.Now < e.ExpireTime))
                .Where(e => baid == 0 || (e.Baid == baid || e.Participants.Any(p => p.Baid == baid)))
                .Where(e => lowSearch == null || (e.CompId.ToString() == lowSearch || e.CompeteName.ToLower().Contains(lowSearch)));
        } 
        else if (mode == CompeteModeType.Compete)
        {
            query = context.ChallengeCompeteData
                .Include(e => e.Songs).ThenInclude(e => e.BestScores).Include(e => e.Participants)
                .Where(e => e.CompeteMode == CompeteModeType.Compete)
                .Where(e => inProgress == false || (e.CreateTime < DateTime.Now && DateTime.Now < e.ExpireTime))
                .Where(e => baid == 0 || (e.Baid == baid || e.Participants.Any(p => p.Baid == baid) || e.Share == ShareType.EveryOne))
                .Where(e => lowSearch == null || (e.CompId.ToString() == lowSearch || e.CompeteName.ToLower().Contains(lowSearch)));
        }
        else if (mode == CompeteModeType.OfficialCompete)
        {
            query = context.ChallengeCompeteData
                .Include(e => e.Songs).ThenInclude(e => e.BestScores).Include(e => e.Participants)
                .Where(e => e.CompeteMode == CompeteModeType.OfficialCompete)
                .Where(e => inProgress == false || (e.CreateTime < DateTime.Now && DateTime.Now < e.ExpireTime))
                .Where(e => lowSearch == null || (e.CompId.ToString() == lowSearch || e.CompeteName.ToLower().Contains(lowSearch)));
        }
        if (query == null) return new ChallengeCompetitionResponse();

        var total = await query.CountAsync();
        var totalPage = total / limit;
        if (total % limit > 0) totalPage += 1;

        var challengeCompeteDatum= await query
            .OrderBy(e => e.CompId).Skip((page - 1) * limit).Take(limit)
            .ToListAsync();

        List<ChallengeCompetition> converted = new();
        foreach (var data in challengeCompeteDatum)
        {
            var challengeCompetition = Mappers.ChallengeCompeMappers.MapData(data);
            challengeCompetition = await FillData(challengeCompetition);
            converted.Add(challengeCompetition);
        }

        return new ChallengeCompetitionResponse
        {
            List = converted,
            Page = page,
            TotalPages = totalPage,
            Total = total
        };
    }

    public Task<ChallengeCompeteDatum?> GetFirstOrDefaultCompete(uint compId)
    {
        return context.ChallengeCompeteData
                .Include(e => e.Songs).ThenInclude(e => e.BestScores).Include(e => e.Participants)
                .Where(c => c.CompId == compId)
                .FirstOrDefaultAsync();
    }

    public async Task CreateCompete(uint baid, ChallengeCompeteCreateInfo challengeCompeteInfo)
    {
        ChallengeCompeteDatum challengeCompeteData = new()
        {
            CompId = context.ChallengeCompeteData.Any() ? context.ChallengeCompeteData.AsEnumerable().Max(c => c.CompId) + 1 : 1,
            CompeteMode = challengeCompeteInfo.CompeteMode,
            State = CompeteState.Normal,
            Baid = baid,
            CompeteName = challengeCompeteInfo.Name,
            CompeteDescribe = challengeCompeteInfo.Desc,
            MaxParticipant = challengeCompeteInfo.MaxParticipant,
            OnlyPlayOnce = challengeCompeteInfo.OnlyPlayOnce,
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
                Speed = song.Speed == -1 ? null : (uint)song.Speed,
                IsInverseOn = song.IsInverseOn == -1 ? null : (song.IsInverseOn != 0),
                IsVanishOn = song.IsVanishOn == -1 ? null : (song.IsVanishOn != 0),
                RandomType = song.RandomType == -1 ? null : (RandomType)song.RandomType,
            };
            await context.AddAsync(challengeCompeteSongData);
        }
        if (baid != 0)
        {
            ChallengeCompeteParticipantDatum participantDatum = new()
            {
                CompId = challengeCompeteData.CompId,
                Baid = baid,
                IsActive = true
            };
            await context.AddAsync(participantDatum);
        }
        await context.SaveChangesAsync();
    }

    public async Task<bool> ParticipateCompete(uint compId, uint baid)
    {
        var challengeCompete = await context.ChallengeCompeteData
            .Include(c => c.Participants).Where(c => c.CompId == compId).FirstOrDefaultAsync();
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
        await context.SaveChangesAsync();

        return true;
    }

    public async Task CreateChallenge(uint baid, uint targetBaid, ChallengeCompeteCreateInfo challengeCompeteInfo)
    {
        ChallengeCompeteDatum challengeCompeteData = new()
        {
            CompId = context.ChallengeCompeteData.Any() ? context.ChallengeCompeteData.AsEnumerable().Max(c => c.CompId) + 1 : 1,
            CompeteMode = CompeteModeType.Chanllenge,
            State = CompeteState.Waiting,
            Baid = baid,
            CompeteName = challengeCompeteInfo.Name,
            CompeteDescribe = challengeCompeteInfo.Desc,
            MaxParticipant = 2,
            OnlyPlayOnce = challengeCompeteInfo.OnlyPlayOnce,
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
                Speed = song.Speed == -1 ? null : (uint)song.Speed,
                IsInverseOn = song.IsInverseOn == -1 ? null : (song.IsInverseOn != 0),
                IsVanishOn = song.IsVanishOn == -1 ? null : (song.IsVanishOn != 0),
                RandomType = song.RandomType == -1 ? null : (RandomType)song.RandomType,
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
        await context.SaveChangesAsync();
    }

    public async Task<bool> AnswerChallenge(uint compId, uint baid, bool accept)
    {
        var challengeCompete = await context.ChallengeCompeteData
            .Include(c => c.Participants).Where(c => c.CompId == compId).FirstOrDefaultAsync();
        challengeCompete.ThrowIfNull($"Challenge not found for CompId {compId}!");

        if (challengeCompete.Baid == baid) return false;
        if (!challengeCompete.Participants.Any(p => p.Baid == baid))
        {
            return false;
        }

        if (accept)
        {
            challengeCompete.State = CompeteState.Normal;
            foreach (var participant in challengeCompete.Participants)
            {
                participant.IsActive = true;
                context.Update(participant);
            }
        }
        else
        {
            challengeCompete.State = CompeteState.Rejected;
        }
        context.Update(challengeCompete);
        await context.SaveChangesAsync();

        return true;
    }

    public async Task UpdateBestScore(uint baid, SongPlayDatum playData, short option)
    {
        List<ChallengeCompeteDatum> challengeCompetes = context.ChallengeCompeteData
            .Include(e => e.Songs)
                .ThenInclude(s => s.BestScores)
            .Include(e => e.Participants)
            .Where(e => e.CreateTime < DateTime.Now && DateTime.Now < e.ExpireTime)
            .Where(e => e.Participants.Any(d => d.Baid == baid && d.IsActive))
            .Where(e => e.Songs.Any(d => d.SongId == playData.SongId && d.Difficulty == playData.Difficulty))
            .Where(e => !e.OnlyPlayOnce || e.Songs.Any(song => !song.BestScores.Any(s => s.Baid == baid)))
            .ToList();
        PlaySetting setting = PlaySettingConverter.ShortToPlaySetting(option);
        foreach (var challengeCompete in challengeCompetes)
        {
            ChallengeCompeteSongDatum? song = challengeCompete.Songs.Find(e => e.SongId == playData.SongId && e.Difficulty == playData.Difficulty);
            if (song == null) continue;
            if (song.Speed != null && song.Speed != setting.Speed) continue;
            if (song.IsVanishOn != null && song.IsVanishOn != setting.IsVanishOn) continue;
            if (song.IsInverseOn != null && song.IsInverseOn != setting.IsInverseOn) continue;
            if (song.RandomType != null && song.RandomType != setting.RandomType) continue;

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
            else if (!challengeCompete.OnlyPlayOnce && bestScore.Score < playData.Score)
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
        await context.AddRangeAsync();
    }

    public async Task<ChallengeCompetition> FillData(ChallengeCompetition challenge)
    {
        UserDatum? holder = await userDatumService.GetFirstUserDatumOrNull(challenge.Baid);
        challenge.Holder = convert(holder);
        foreach (var participant in challenge.Participants)
        {
            if (participant == null) continue;
            UserDatum? user = await userDatumService.GetFirstUserDatumOrNull(participant.Baid);
            participant.UserInfo = convert(user);
        }
        foreach (var song in challenge.Songs)
        {
            if (song == null) continue;
            song.MusicDetail = musicDetailDict.GetValueOrDefault(song.SongId);
            foreach (var score in song.BestScores)
            {
                UserDatum? user = await userDatumService.GetFirstUserDatumOrNull(score.Baid);
                score.UserAppearance = convert(user);
            }
        }

        return challenge;
    }

    private UserAppearance? convert(UserDatum? user)
    {
        if (user == null) return null;
        return new UserAppearance
        {
            Baid = user.Baid,
            MyDonName = user.MyDonName,
            MyDonNameLanguage = user.MyDonNameLanguage,
            Title = user.Title,
            TitlePlateId = user.TitlePlateId,
            Kigurumi = user.CurrentKigurumi,
            Head = user.CurrentHead,
            Body = user.CurrentBody,
            Face = user.CurrentFace,
            Puchi = user.CurrentPuchi,
            FaceColor = user.ColorFace,
            BodyColor = user.ColorBody,
            LimbColor = user.ColorLimb,
        };
    }
}
