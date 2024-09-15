using SharedProject.Models;
using SharedProject.Models.Responses;

namespace TaikoLocalServer.Services.Interfaces;

public interface IChallengeCompeteService
{
    public Task<bool> HasChallengeCompete(uint baid);

    public Task<List<ChallengeCompeteDatum>> GetInProgressChallengeCompete(uint baid);

    public Task<List<ChallengeCompeteDatum>> GetAllChallengeCompete();

    public Task<ChallengeCompetitionResponse> GetChallengeCompetePage(CompeteModeType mode, uint baid, bool inProgress, int page, int limit, string? search);

    public Task CreateCompete(uint baid, ChallengeCompeteCreateInfo challengeCompeteInfo);

    public Task<bool> ParticipateCompete(uint compId, uint baid);

    public Task CreateChallenge(uint baid, uint targetBaid, ChallengeCompeteCreateInfo challengeCompeteInfo);

    public Task<bool> AnswerChallenge(uint compId, uint baid, bool accept);

    public Task UpdateBestScore(uint baid, SongPlayDatum playData, short option);

    public Task<ChallengeCompetition> FillData(ChallengeCompetition challenge);
}
