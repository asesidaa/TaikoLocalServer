namespace SharedProject.Models.Requests;

public class AnswerChallengeRequest
{
    public uint CompId { get; set; }
    public uint Baid { get; set; }
    public bool Accept { get; set; }
}
