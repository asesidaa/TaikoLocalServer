using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Domain.Entities;

public interface IAc15SongBestDatum
{
    uint Baid { get; set; }

    uint SongId { get; set; }

    Difficulty Difficulty { get; set; }

    bool IsShin { get; set; }

    uint BestScore { get; set; }

    uint BestRate { get; set; }

    CrownType BestCrown { get; set; }
}
