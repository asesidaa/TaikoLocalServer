using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Domain.Entities;

public class AiScoreDatumNijiiro
{
    public uint Baid { get; set; }

    public uint SongId { get; set; }

    public Difficulty Difficulty { get; set; }

    public bool IsWin { get; set; }

    public List<AiSectionScoreDatumNijiiro> AiSectionScoreData { get; set; } = new();

    public virtual UserDatum? Ba { get; set; }
}