namespace SharedProject.Models.Responses;

public class ChallengeCompetitionResponse
{
    public List<ChallengeCompetition> List { get; set; } = new();
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 0;
    public int Total { get; set; } = 0;

}
