namespace TaikoLocalServer.Domain.Entities;

public interface IAc15DonChallengeProgress
{
    uint Baid { get; set; }
    string BundleId { get; set; }
    uint TaskId { get; set; }
    uint Slot { get; set; }
    uint CompeId { get; set; }
    uint TrackNo { get; set; }
    uint SongNo { get; set; }
    uint Level { get; set; }
    byte[] OptionFlg { get; set; }
    uint StageMode { get; set; }
    uint HighScore { get; set; }
    uint ProgressValue { get; set; }
    bool Completed { get; set; }
    DateTime UpdatedAt { get; set; }
    DateTime? CompletedAt { get; set; }
}

public interface IAc15DonChallengeRawFact
{
    uint Baid { get; set; }
    string BundleId { get; set; }
    uint TaskId { get; set; }
    uint Slot { get; set; }
    uint CompeId { get; set; }
    uint TrackNo { get; set; }
    uint SongNo { get; set; }
    uint Level { get; set; }
    byte[] OptionFlg { get; set; }
    uint StageMode { get; set; }
    uint HighScore { get; set; }
    uint PlayResult { get; set; }
    uint ProgressValue { get; set; }
    bool Completed { get; set; }
    DateTime PlayTime { get; set; }
    DateTime CreatedAt { get; set; }
}
