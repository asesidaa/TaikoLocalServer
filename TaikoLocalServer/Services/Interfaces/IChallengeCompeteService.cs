namespace TaikoLocalServer.Services.Interfaces;

public interface IChallengeCompeteService
{
    public bool HasChallengeCompete(uint baid);

    public List<ChallengeCompeteDatum> GetInProgressChallengeCompete(uint baid);

    public List<ChallengeCompeteDatum> GetAllChallengeCompete();

    public Task CreateCompete(uint baid, ChallengeCompeteInfo challengeCompeteInfo);

    public Task<bool> ParticipateCompete(uint compId, uint baid);

    public Task CreateChallenge(uint baid, uint targetBaid, ChallengeCompeteInfo challengeCompeteInfo);

    public Task<bool> AnswerChallenge(uint compId, uint baid, bool accept);

    public Task UpdateBestScore(uint baid, SongPlayDatum playData, short option);
}
