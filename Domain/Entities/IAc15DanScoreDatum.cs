using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Domain.Entities;

public interface IAc15DanScoreDatum
{
    uint Baid { get; set; }

    uint DanId { get; set; }

    bool IsExtra { get; set; }

    uint MedleyUniqueId { get; set; }

    uint ArrivalSongCount { get; set; }

    uint SoulGaugeTotal { get; set; }

    uint ComboCountTotal { get; set; }

    Ac15DanClearGrade ClearGrade { get; set; }
}
