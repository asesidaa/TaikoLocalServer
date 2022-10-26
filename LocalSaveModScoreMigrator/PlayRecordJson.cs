using System.Text.Json.Serialization;
using SharedProject.Enums;

namespace LocalSaveModScoreMigrator;

public class PlayRecordJson
{
    public string SongId { get; set; } = "tmap4";
    public Difficulty Difficulty { get; set; }
    
    [JsonPropertyName("dateTime")]
    public DateTime DateTime { get; set; }
    public uint Score { set; get; }
    public CrownType Crown { get; set; }
    
    [JsonPropertyName("scorerank")]
    public ScoreRank Scorerank { get; set; }
    public uint Good { get; set; }
    public uint Ok { get; set; }
    public uint Bad { get; set; }
    public uint Combo { get; set; }
    public uint Drumroll { get; set; }
}