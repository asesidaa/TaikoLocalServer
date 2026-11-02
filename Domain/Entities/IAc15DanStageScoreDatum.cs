namespace TaikoLocalServer.Domain.Entities;

public interface IAc15DanStageScoreDatum
{
    uint Baid { get; set; }

    uint DanId { get; set; }

    bool IsExtra { get; set; }

    uint StageIndex { get; set; }

    uint SongNumber { get; set; }

    uint PlayScore { get; set; }

    uint GoodCount { get; set; }

    uint OkCount { get; set; }

    uint BadCount { get; set; }

    uint DrumrollCount { get; set; }

    uint TotalHitCount { get; set; }

    uint ComboCount { get; set; }

    uint HighScore { get; set; }
}
