namespace TaikoLocalServer.Domain.Entities;

public class RedChallengeCompeProgress
{
    public uint Baid { get; set; }
    public string BundleId { get; set; } = string.Empty;
    public uint TaskId { get; set; }
    public uint Slot { get; set; }
    public uint CompeId { get; set; }
    public uint TrackNo { get; set; }
    public uint SongNo { get; set; }
    public uint Level { get; set; }
    public byte[] OptionFlg { get; set; } = [];
    public uint StageMode { get; set; }
    public uint HighScore { get; set; }
    public uint ProgressValue { get; set; }
    public bool Completed { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public virtual UserDatum? Ba { get; set; }
}
