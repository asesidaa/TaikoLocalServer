using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Domain.Entities;

public interface IAc15SongPlayDatum
{
    uint Baid { get; set; }

    uint SongId { get; set; }

    Difficulty Difficulty { get; set; }

    CrownType Crown { get; set; }

    uint Score { get; set; }

    uint ScoreRate { get; set; }

    uint GoodCount { get; set; }

    uint OkCount { get; set; }

    uint MissCount { get; set; }

    uint ComboCount { get; set; }

    uint HitCount { get; set; }

    uint PoundCount { get; set; }

    uint StarLevel { get; set; }

    byte[] OptionFlg { get; set; }

    byte[] ToneFlg { get; set; }

    uint PlayMode { get; set; }

    uint StageMode { get; set; }

    bool IsShin { get; set; }

    uint MusicCategory { get; set; }

    uint SelectedFolderId { get; set; }

    bool IsFavorite { get; set; }

    bool IsRecent { get; set; }

    bool IsPapamama { get; set; }

    bool IsPushed { get; set; }

    uint SoulGauge { get; set; }

    uint PlayDan { get; set; }

    uint WaiwaiResult { get; set; }

    uint WaiwaiGauge { get; set; }

    DateTime PlayTime { get; set; }
}
