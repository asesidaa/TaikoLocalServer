using TaikoLocalServer.Domain.Enums;

namespace TaikoLocalServer.Domain.Entities;

public partial class SongPlayDatumMurasaki : IAc15SongPlayDatum
{
    public long Id { get; set; }
    public uint Baid { get; set; }
    public uint SongId { get; set; }
    public Difficulty Difficulty { get; set; }
    public CrownType Crown { get; set; }
    public uint Score { get; set; }
    public uint ScoreRate { get; set; }
    public uint GoodCount { get; set; }
    public uint OkCount { get; set; }
    public uint MissCount { get; set; }
    public uint ComboCount { get; set; }
    public uint HitCount { get; set; }
    public uint PoundCount { get; set; }
    public uint StarLevel { get; set; }
    public byte[] OptionFlg { get; set; } = [];
    public byte[] ToneFlg { get; set; } = [];
    public uint PlayMode { get; set; }
    public uint StageMode { get; set; }
    public bool IsShin { get; set; }
    public uint MusicCategory { get; set; }
    public uint SelectedFolderId { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsRecent { get; set; }
    public bool IsPapamama { get; set; }
    public bool IsPushed { get; set; }
    public uint SoulGauge { get; set; }
    public uint PlayDan { get; set; }
    public uint WaiwaiResult { get; set; }
    public uint WaiwaiGauge { get; set; }
    public DateTime PlayTime { get; set; }
    public virtual UserDatum? Ba { get; set; }
}
