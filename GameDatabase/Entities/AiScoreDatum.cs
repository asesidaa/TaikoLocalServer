using SharedProject.Enums;
using SharedProject.Entities;

namespace GameDatabase.Entities;

public class AiScoreDatum
{
    public uint Baid { get; set; }

    public uint SongId { get; set; }

    public Difficulty Difficulty { get; set; }

    public bool IsWin { get; set; }

    public List<AiSectionScoreDatum> AiSectionScoreData { get; set; } = new();

    public virtual UserDatum? Ba { get; set; }
}