using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Domain.Entities;

public partial class SongBestDatumYellow
{
    public uint Baid { get; set; }
    public uint SongId { get; set; }
    public Difficulty Difficulty { get; set; }
    public bool IsShin { get; set; }
    public uint BestScore { get; set; }
    public uint BestRate { get; set; }
    public CrownType BestCrown { get; set; }
    public virtual UserDatum? Ba { get; set; }
}
